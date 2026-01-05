using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Diagnostics;

using Rssdp.Infrastructure;

namespace Rssdp
{
	/// <summary>
	/// Aggregates multiple <see cref="SsdpDeviceLocator"/> instances bound to different local adapter IP addresses
	/// to enable discovery across multiple network interfaces.
	/// </summary>
	public sealed class AggregateSsdpDeviceLocator : IDisposable, ISsdpDeviceLocator
	{
		private readonly List<SsdpDeviceLocator> _Locators;
		private readonly List<string> _AdapterIps = new();
		private readonly ISsdpLogger? _Logger;
		private string? _NotificationFilter;
		private bool _IsSearching;
		private readonly ActivitySource _ActivitySource = SsdpConstants.LocatorActivitySource;

		/// <summary>
		/// Raised when a device becomes available or is found by a search request across any aggregated locator.
		/// </summary>
		public event EventHandler<DeviceAvailableEventArgs>? DeviceAvailable;

		/// <summary>
		/// Raised when a device explicitly notifies of shutdown or expires from the cache across any aggregated locator.
		/// </summary>
		public event EventHandler<DeviceUnavailableEventArgs>? DeviceUnavailable;

		/// <summary>
		/// Creates an aggregate device locator that queries multiple network adapters.
		/// </summary>
		/// <param name="localIpAddresses">A collection of local adapter IP addresses to bind individual locators to. Null/empty entries are ignored. Must contain at least one valid address.</param>
		/// <param name="logger">Optional logger for diagnostics.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="localIpAddresses"/> is null.</exception>
		/// <exception cref="ArgumentException">Thrown when no valid IP addresses are provided.</exception>
		public AggregateSsdpDeviceLocator(IEnumerable<string> localIpAddresses, ISsdpLogger? logger = null)
		{
			if (localIpAddresses == null) throw new ArgumentNullException(nameof(localIpAddresses));
			var list = localIpAddresses.Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().ToList();
			if (list.Count == 0) throw new ArgumentException("No IP addresses provided.", nameof(localIpAddresses));

			_Logger = logger;
			_Logger?.LogVerbose($"AggregateSsdpDeviceLocator initializing {list.Count} adapter(s).");

			_Locators = list.Select(ip => new SsdpDeviceLocator(ip)).ToList();
			_AdapterIps.AddRange(list);

			foreach (var locator in _Locators)
			{
				locator.DeviceAvailable += OnDeviceAvailable;
				locator.DeviceUnavailable += OnDeviceUnavailable;
			}
		}

		/// <summary>
		/// Creates an aggregate device locator by discovering local adapter IP addresses.
		/// </summary>
		/// <param name="includeIpv4">If true, include IPv4 addresses.</param>
		/// <param name="includeIpv6">If true, include IPv6 addresses.</param>
		/// <param name="adapterFilter">Optional filter to include only specific adapters. If null, a sensible default filter is applied.</param>
		/// <param name="logger">Optional logger for diagnostics.</param>
		public AggregateSsdpDeviceLocator(bool includeIpv4 = true, bool includeIpv6 = false, Func<NetworkInterface, bool>? adapterFilter = null, ISsdpLogger? logger = null)
			: this(GetAdapterIpAddresses(includeIpv4, includeIpv6, adapterFilter), logger)
		{
		}

		/// <summary>
		/// Indicates whether a search is currently in progress across any aggregated locator.
		/// </summary>
		public bool IsSearching
		{
			get
			{
				return _IsSearching;
			}
		}

		/// <summary>
		/// Gets or sets the filter for notifications. Notifications not matching the filter will not raise <see cref="DeviceAvailable"/> or <see cref="DeviceUnavailable"/>.
		/// The value is applied to all aggregated locators.
		/// </summary>
		public string? NotificationFilter
		{
			get
			{
				return _NotificationFilter;
			}

			set
			{
				_NotificationFilter = value;
				foreach (var locator in _Locators)
				{
					locator.NotificationFilter = value;
				}
			}
		}

		/// <summary>
		/// Starts listening for broadcast notifications of device availability on all aggregated locators.
		/// </summary>
		public void StartListeningForNotifications()
		{
			StartListeningForNotificationsAsync().GetAwaiter().GetResult();
		}

		/// <summary>
		/// Asynchronously starts listening for multicast notifications on all aggregated locators and awaits per-adapter completion.
		/// </summary>
		public async Task StartListeningForNotificationsAsync()
		{
			var tasks = new List<Task>(_Locators.Count);
			for (int i = 0; i < _Locators.Count; i++)
			{
				var locator = _Locators[i];
				var ip = i < _AdapterIps.Count ? _AdapterIps[i] : "unknown";
				tasks.Add(Task.Run(() =>
				{
					try
					{
						locator.StartListeningForNotifications();
					}
					catch (Exception ex)
					{
						_Logger?.LogWarning($"Failed to start notifications on adapter {ip}: {ex.Message}");
						throw;
					}
				}));
			}

			try
			{
				await Task.WhenAll(tasks).ConfigureAwait(false);
			}
			catch (AggregateException agg)
			{
				// Already logged per-adapter; keep aggregate for caller visibility
				_Logger?.LogWarning($"One or more adapters failed to start notifications: {agg.GetBaseException().Message}");
			}
		}

		/// <summary>
		/// Stops listening for broadcast notifications on all aggregated locators.
		/// </summary>
		public void StopListeningForNotifications()
		{
			StopListeningForNotificationsAsync().GetAwaiter().GetResult();
		}

		/// <summary>
		/// Asynchronously stops listening for multicast notifications on all aggregated locators and awaits per-adapter completion.
		/// </summary>
		public async Task StopListeningForNotificationsAsync()
		{
			var tasks = new List<Task>(_Locators.Count);
			for (int i = 0; i < _Locators.Count; i++)
			{
				var locator = _Locators[i];
				var ip = i < _AdapterIps.Count ? _AdapterIps[i] : "unknown";
				tasks.Add(Task.Run(() =>
				{
					try
					{
						locator.StopListeningForNotifications();
					}
					catch (Exception ex)
					{
						_Logger?.LogWarning($"Failed to stop notifications on adapter {ip}: {ex.Message}");
						throw;
					}
				}));
			}

			try
			{
				await Task.WhenAll(tasks).ConfigureAwait(false);
			}
			catch (AggregateException agg)
			{
				_Logger?.LogWarning($"One or more adapters failed to stop notifications: {agg.GetBaseException().Message}");
			}
		}

		/// <summary>
		/// Asynchronously performs a search for all devices using the default search timeout across all aggregated locators.
		/// </summary>
		/// <param name="cancellationToken">A token used to cancel the search across all locators.</param>
		/// <returns>A task whose result is a de-duplicated sequence of discovered devices.</returns>
		public Task<IEnumerable<DiscoveredSsdpDevice>> SearchAsync(CancellationToken cancellationToken = default)
		{
			return SearchInternalAsync(null, null, cancellationToken);
		}

		/// <summary>
		/// Asynchronously performs a search for the specified search target using the default timeout across all aggregated locators.
		/// </summary>
		/// <param name="searchTarget">Search criteria such as device type, root device, or UUID.</param>
		/// <param name="cancellationToken">A token used to cancel the search across all locators.</param>
		/// <returns>A task whose result is a de-duplicated sequence of discovered devices.</returns>
		public Task<IEnumerable<DiscoveredSsdpDevice>> SearchAsync(string searchTarget, CancellationToken cancellationToken = default)
		{
			return SearchInternalAsync(searchTarget, null, cancellationToken);
		}

		/// <summary>
		/// Asynchronously performs a search for the specified search target using the provided timeout across all aggregated locators.
		/// </summary>
		/// <param name="searchTarget">Search criteria such as device type, root device, or UUID.</param>
		/// <param name="searchWaitTime">The time to wait for responses.</param>
		/// <param name="cancellationToken">A token used to cancel the search across all locators.</param>
		/// <returns>A task whose result is a de-duplicated sequence of discovered devices.</returns>
		public Task<IEnumerable<DiscoveredSsdpDevice>> SearchAsync(string searchTarget, TimeSpan searchWaitTime, CancellationToken cancellationToken = default)
		{
			return SearchInternalAsync(searchTarget, searchWaitTime, cancellationToken);
		}

		/// <summary>
		/// Asynchronously performs a search for all devices using the provided timeout across all aggregated locators.
		/// </summary>
		/// <param name="searchWaitTime">The time to wait for responses.</param>
		/// <param name="cancellationToken">A token used to cancel the search across all locators.</param>
		/// <returns>A task whose result is a de-duplicated sequence of discovered devices.</returns>
		public Task<IEnumerable<DiscoveredSsdpDevice>> SearchAsync(TimeSpan searchWaitTime, CancellationToken cancellationToken = default)
		{
			return SearchInternalAsync(null, searchWaitTime, cancellationToken);
		}

		private async Task<IEnumerable<DiscoveredSsdpDevice>> SearchInternalAsync(string? searchTarget, TimeSpan? searchWaitTime, CancellationToken cancellationToken)
		{
			Activity? activity = null;
			try
			{
				if (_ActivitySource.HasListeners())
				{
					activity = _ActivitySource.StartActivity("ssdp.aggregate.search", ActivityKind.Client);
					activity?.SetTag("adapter.count", _Locators.Count);
					if (searchTarget != null) activity?.SetTag("ssdp.st", searchTarget);
					if (searchWaitTime != null) activity?.SetTag("ssdp.waittime", searchWaitTime);
				}

				_IsSearching = true;
				var tasks = new List<Task<IEnumerable<DiscoveredSsdpDevice>>>(_Locators.Count);
				foreach (var locator in _Locators)
				{
					if (searchTarget == null && searchWaitTime == null)
						tasks.Add(locator.SearchAsync(cancellationToken));
					else if (searchTarget == null && searchWaitTime != null)
						tasks.Add(locator.SearchAsync(searchWaitTime.Value, cancellationToken));
					else if (searchTarget != null && searchWaitTime == null)
						tasks.Add(locator.SearchAsync(searchTarget, cancellationToken));
					else
						tasks.Add(locator.SearchAsync(searchTarget!, searchWaitTime!.Value, cancellationToken));
				}

				IEnumerable<DiscoveredSsdpDevice> merged;
				try
				{
					// will throw if any fault
					await Task.WhenAll(tasks).ConfigureAwait(false);
				}
				catch (AggregateException)
				{
					// Inspect individual task statuses below
				}

				// Log any task failures against their adapter IP
				for (int i = 0; i < tasks.Count; i++)
				{
					var task = tasks[i];
					if (task.IsFaulted)
					{
						var ip = i < _AdapterIps.Count ? _AdapterIps[i] : "unknown";
						_Logger?.LogWarning($"Adapter {ip} search failed: {task.Exception?.GetBaseException().Message}");
					}
				}

				var successfulResults = tasks
					.Where(t => t.Status == TaskStatus.RanToCompletion && t.Result != null)
					.SelectMany(t => t.Result);

				if (!successfulResults.Any())
				{
					// If everything failed, rethrow the first exception to preserve failure behavior
					var firstFault = tasks.FirstOrDefault(t => t.IsFaulted);
					if (firstFault?.Exception != null)
					{
						_Logger?.LogError($"All adapter searches failed: {firstFault.Exception.GetBaseException().Message}");
						throw firstFault.Exception.Flatten();
					}
					_Logger?.LogWarning("All adapter searches returned no results.");
					// Otherwise return empty
					return [];
				}

				_Logger?.LogVerbose($"Merging results from {successfulResults.Count()} adapter(s).");
				// Flatten and de-duplicate by USN and DescriptionLocation from successful tasks only.
				merged = successfulResults
					.GroupBy(d => new { d.Usn, d.DescriptionLocation })
					.Select(g => g.First())
					.ToList();

				return merged;
			}
			finally
			{
				_IsSearching = false;

				// Close aggregate search activity
				activity?.Dispose();
			}
		}

		/// <summary>
		/// Disposes the aggregate locator and all underlying locators, detaching event handlers.
		/// </summary>
		public void Dispose()
		{
			foreach (var locator in _Locators)
			{
				try
				{
					locator.DeviceAvailable -= OnDeviceAvailable;
					locator.DeviceUnavailable -= OnDeviceUnavailable;
					locator.Dispose();
				}
				catch
				{
					// Swallow dispose errors to ensure all locators are attempted.
				}
			}
		}

		private static IEnumerable<string> GetAdapterIpAddresses(bool includeIpv4, bool includeIpv6, Func<NetworkInterface, bool>? adapterFilter)
		{
			IEnumerable<NetworkInterface> adapters = NetworkInterface.GetAllNetworkInterfaces();
			Func<NetworkInterface, bool> defaultFilter = ni =>
				ni.OperationalStatus == OperationalStatus.Up &&
				ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
				ni.SupportsMulticast &&
				!ni.IsReceiveOnly &&
				(
					ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
					ni.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet ||
					ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
					ni.NetworkInterfaceType == NetworkInterfaceType.FastEthernetFx ||
					ni.NetworkInterfaceType == NetworkInterfaceType.FastEthernetT
				);

			var filtered = adapters.Where(adapterFilter ?? defaultFilter);
			var ips = new List<string>();
			foreach (var ni in filtered)
			{
				var ipProps = ni.GetIPProperties();
				foreach (var uni in ipProps.UnicastAddresses)
				{
					if (includeIpv4 && uni.Address.AddressFamily == AddressFamily.InterNetwork)
						ips.Add(uni.Address.ToString());
					if (includeIpv6 && uni.Address.AddressFamily == AddressFamily.InterNetworkV6)
						ips.Add(uni.Address.ToString());
				}
			}

			return ips.Distinct();
		}

		private void OnDeviceAvailable(object? sender, DeviceAvailableEventArgs e)
		{
			var handlers = DeviceAvailable;
			if (handlers == null) return;
			foreach (EventHandler<DeviceAvailableEventArgs> handler in handlers.GetInvocationList().Cast<EventHandler<DeviceAvailableEventArgs>>())
			{
				try
				{
					handler(this, e);
				}
				catch (Exception ex)
				{
					_Logger?.LogWarning($"DeviceAvailable handler threw: {ex.GetBaseException().Message}");
				}
			}
		}

		private void OnDeviceUnavailable(object? sender, DeviceUnavailableEventArgs e)
		{
			var handlers = DeviceUnavailable;
			if (handlers == null) return;
			foreach (EventHandler<DeviceUnavailableEventArgs> handler in handlers.GetInvocationList().Cast<EventHandler<DeviceUnavailableEventArgs>>())
			{
				try
				{
					handler(this, e);
				}
				catch (Exception ex)
				{
					_Logger?.LogWarning($"DeviceUnavailable handler threw: {ex.GetBaseException().Message}");
				}
			}
		}
	}
}
