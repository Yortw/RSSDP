using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rssdp.Infrastructure;
using Rssdp.Network;

namespace Rssdp.Aggregatable
{
	/// <summary>
	/// Creates a device locators for each available network interface
	/// </summary>
	public sealed class AggregatableDeviceLocator : IAggregatableDeviceLocator
	{
		private readonly IList<ISsdpDeviceLocator> _ssdpDeviceLocators = new List<ISsdpDeviceLocator>();

		/// <param name="networkInfoProvider"></param>
		/// <param name="ssdpDeviceLocatorFactory"></param>
		/// <param name="port">local port for each locators</param>
		public AggregatableDeviceLocator(INetworkInfoProvider networkInfoProvider,
			ISsdpDeviceLocatorFactory ssdpDeviceLocatorFactory,
			int port)
		{
			if (networkInfoProvider == null) throw new ArgumentNullException(nameof(networkInfoProvider));
			if (ssdpDeviceLocatorFactory == null) throw new ArgumentNullException(nameof(ssdpDeviceLocatorFactory));

			var unicastAddresses = networkInfoProvider.GetIpAddressesFromAdapters();
			AddLocator(ssdpDeviceLocatorFactory, unicastAddresses, port);
		}

		/// <param name="unicastAddresses">list of unicast addresses for create locators</param>
		/// <param name="ssdpDeviceLocatorFactory"></param>
		/// <param name="port">local port for each locators</param>
		public AggregatableDeviceLocator(IEnumerable<string> unicastAddresses,
			ISsdpDeviceLocatorFactory ssdpDeviceLocatorFactory,
			int port)
		{
			if (unicastAddresses == null) throw new ArgumentNullException(nameof(unicastAddresses));
			if (ssdpDeviceLocatorFactory == null) throw new ArgumentNullException(nameof(ssdpDeviceLocatorFactory));

			AddLocator(ssdpDeviceLocatorFactory, unicastAddresses, port);
		}

		/// <summary>
		/// Dispose all created <see cref="ISsdpDeviceLocator"/>
		/// </summary>
		public void Dispose()
		{
			foreach (var ssdpDeviceLocator in _ssdpDeviceLocators)
			{
				ssdpDeviceLocator.DeviceAvailable -= OnDeviceAvailable;
				ssdpDeviceLocator.DeviceUnavailable -= OnDeviceUnavailable;
				ssdpDeviceLocator.StopListeningForNotifications();
				ssdpDeviceLocator.Dispose();
			}
		}

		/// <summary>
		/// Event raised when a device becomes available
		/// </summary>
		public event EventHandler<DeviceAvailableEventArgs> DeviceAvailable;

		/// <summary>
		/// Event raised when a device explicitly notifies of shutdown or a device expires from the cache.
		/// </summary>
		public event EventHandler<DeviceUnavailableEventArgs> DeviceUnavailable;

		/// <summary>
		/// Provides all instances of created <see cref="ISsdpDeviceLocator"/>
		/// </summary>
		public IEnumerable<ISsdpDeviceLocator> Locators
		{
			get { return _ssdpDeviceLocators; }
		}

		/// <summary>
		/// Aynchronously performs a search for all devices using the default search timeout, and returns an awaitable task that can be used to retrieve the results.
		/// <remarks>All locators doing search</remarks>
		/// </summary>
		public async Task<IEnumerable<DiscoveredSsdpDevice>> SearchAsync()
		{
			var allDevices = new List<DiscoveredSsdpDevice>();
			foreach (var ssdpDeviceLocator in _ssdpDeviceLocators)
			{
				var devices = await ssdpDeviceLocator.SearchAsync();
				allDevices.AddRange(devices);
			}
			return allDevices;
		}

		/// <summary>
		/// Starts listening for broadcast notifications of service availability on all locators
		/// </summary>
		/// <remarks>
		/// <para>When called the system will listen for 'alive' and 'byebye' notifications. This can speed up searching, as well as provide dynamic notification of new devices appearing on the network, and previously discovered devices disappearing.</para>
		/// </remarks>
		public void StartListeningForNotifications()
		{
			foreach (var ssdpDeviceLocator in _ssdpDeviceLocators)
				ssdpDeviceLocator.StartListeningForNotifications();
		}

		/// <summary>
		/// Stops listening for broadcast notifications of service availability on all locators
		/// </summary>
		/// <remarks>
		/// <para>Does nothing if this instance is not already listening for notifications.</para>
		/// </remarks>
		/// <exception cref="System.ObjectDisposedException">Throw if the <see cref="DisposableManagedObjectBase.IsDisposed"/> property is true.</exception>
		public void StopListening()
		{
			foreach (var ssdpDeviceLocator in _ssdpDeviceLocators)
				ssdpDeviceLocator.StopListeningForNotifications();
		}

		private void AddLocator(ISsdpDeviceLocatorFactory ssdpDeviceLocatorFactory, IEnumerable<string> availableUnicastAddresses, int port)
		{
			foreach (var availableUnicastAddress in availableUnicastAddresses)
			{
				var ssdpDeviceLocator = ssdpDeviceLocatorFactory.Create(availableUnicastAddress, port);

				ssdpDeviceLocator.DeviceAvailable += OnDeviceAvailable;
				ssdpDeviceLocator.DeviceUnavailable += OnDeviceUnavailable;
				_ssdpDeviceLocators.Add(ssdpDeviceLocator);
			}
		}

		private void OnDeviceAvailable(object sender, DeviceAvailableEventArgs deviceAvailableEventArgs)
		{
			DeviceAvailable?.Invoke(this, deviceAvailableEventArgs);
		}

		private void OnDeviceUnavailable(object sender, DeviceUnavailableEventArgs deviceUnavailableEventArgs)
		{
			DeviceUnavailable?.Invoke(this, deviceUnavailableEventArgs);
		}
	}
}
