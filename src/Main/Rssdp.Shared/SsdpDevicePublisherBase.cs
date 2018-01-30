using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rssdp.Infrastructure
{
	/// <summary>
	/// Provides the platform independent logic for publishing SSDP devices (notifications and search responses).
	/// </summary>
	public abstract class SsdpDevicePublisherBase : DisposableManagedObjectBase, ISsdpDevicePublisher
	{

		#region Fields & Constants

		private ISsdpCommunicationsServer _CommsServer;
		private string _OSName;
		private string _OSVersion;
		private ISsdpLogger _Log;

		private bool _SupportPnpRootDevice;
		private SsdpStandardsMode _StandardsMode;

		private IList<SsdpRootDevice> _Devices;
		private ReadOnlyEnumerable<SsdpRootDevice> _ReadOnlyDevices;

		private System.Threading.Timer _RebroadcastAliveNotificationsTimer;
		private TimeSpan _RebroadcastAliveNotificationsTimeSpan;
		private DateTime _LastNotificationTime;

		private IDictionary<string, SearchRequest> _RecentSearchRequests;
		private IUpnpDeviceValidator _DeviceValidator;

		private Random _Random;
		private TimeSpan _MinCacheTime;
		private TimeSpan _NotificationBroadcastInterval;

		private const string ServerVersion = "1.0";

		#endregion

		#region Message Format Constants

		private const string DeviceSearchResponseMessageFormat = @"HTTP/1.1 200 OK
EXT:
DATE: {7}
{0}
ST:{1}
SERVER: {4}/{5} UPnP/1.0 RSSDP/{6}
USN:{2}
LOCATION:{3}{8}

"; //Blank line at end important, do not remove.


		private const string AliveNotificationMessageFormat = @"NOTIFY * HTTP/1.1
HOST: {8}:{9}
DATE: {7}
NT: {0}
NTS: ssdp:alive
SERVER: {4}/{5} UPnP/1.0 RSSDP/{6}
USN: {1} 
LOCATION: {2}
{3}{10}

"; //Blank line at end important, do not remove.

		private const string ByeByeNotificationMessageFormat = @"NOTIFY * HTTP/1.1
HOST: {6}:{7}
DATE: {5}
NT: {0}
NTS: ssdp:byebye
SERVER: {2}/{3} UPnP/1.0 RSSDP/{4}
USN: {1}

";

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="communicationsServer">The <see cref="ISsdpCommunicationsServer"/> implementation, used to send and receive SSDP network messages.</param>
		/// <param name="osName">Then name of the operating system running the server.</param>
		/// <param name="osVersion">The version of the operating system running the server.</param>
		protected SsdpDevicePublisherBase(ISsdpCommunicationsServer communicationsServer, string osName, string osVersion) : this(communicationsServer, osName, osVersion, NullLogger.Instance)
		{
		}

		/// <summary>
		/// Full constructor.
		/// </summary>
		/// <param name="communicationsServer">The <see cref="ISsdpCommunicationsServer"/> implementation, used to send and receive SSDP network messages.</param>
		/// <param name="osName">Then name of the operating system running the server.</param>
		/// <param name="osVersion">The version of the operating system running the server.</param>
		/// <param name="log">An implementation of <see cref="ISsdpLogger"/> to be used for logging activity. May be null, in which case no logging is performed.</param>
		protected SsdpDevicePublisherBase(ISsdpCommunicationsServer communicationsServer, string osName, string osVersion, ISsdpLogger log)
		{
			if (communicationsServer == null) throw new ArgumentNullException("communicationsServer");
			if (osName == null) throw new ArgumentNullException("osName");
			if (osName.Length == 0) throw new ArgumentException("osName cannot be an empty string.", "osName");
			if (osVersion == null) throw new ArgumentNullException("osVersion");
			if (osVersion.Length == 0) throw new ArgumentException("osVersion cannot be an empty string.", "osName");

			_Log = log ?? NullLogger.Instance;
			_SupportPnpRootDevice = true;
			_Devices = new List<SsdpRootDevice>();
			_ReadOnlyDevices = new ReadOnlyEnumerable<SsdpRootDevice>(_Devices);
			_RecentSearchRequests = new Dictionary<string, SearchRequest>(StringComparer.OrdinalIgnoreCase);
			_Random = new Random();
			_DeviceValidator = new Upnp10DeviceValidator(); //Should probably inject this later, but for now we only support 1.0.

			_CommsServer = communicationsServer;
			_CommsServer.RequestReceived += CommsServer_RequestReceived;
			_OSName = osName;
			_OSVersion = osVersion;

			_Log.LogInfo("Publisher started.");
			_CommsServer.BeginListeningForBroadcasts();
			_Log.LogInfo("Publisher started listening for broadcasts.");
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Adds a device (and it's children) to the list of devices being published by this server, making them discoverable to SSDP clients.
		/// </summary>
		/// <remarks>
		/// <para>Adding a device causes "alive" notification messages to be sent immediately, or very soon after. Ensure your device/description service is running before adding the device object here.</para>
		/// <para>Devices added here with a non-zero cache life time will also have notifications broadcast periodically.</para>
		/// <para>This method ignores duplicate device adds (if the same device instance is added multiple times, the second and subsequent add calls do nothing).</para>
		/// </remarks>
		/// <param name="device">The <see cref="SsdpDevice"/> instance to add.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="device"/> argument is null.</exception>
		/// <exception cref="System.InvalidOperationException">Thrown if the <paramref name="device"/> contains property values that are not acceptable to the UPnP 1.0 specification.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "t", Justification = "Capture task to local variable supresses compiler warning, but task is not really needed.")]
		public void AddDevice(SsdpRootDevice device)
		{
			if (device == null) throw new ArgumentNullException("device");

			ThrowIfDisposed();

			_DeviceValidator.ThrowIfDeviceInvalid(device);

			TimeSpan minCacheTime = TimeSpan.Zero;
			bool wasAdded = false;
			lock (_Devices)
			{
				if (!_Devices.Contains(device))
				{
					_Devices.Add(device);
					wasAdded = true;
					minCacheTime = GetMinimumNonZeroCacheLifetime();
				}
			}

			if (wasAdded)
			{
				LogDeviceEvent("Device added", device);

				_MinCacheTime = minCacheTime;

				ConnectToDeviceEvents(device);

				SetRebroadcastAliveNotificationsTimer(minCacheTime);

				SendAliveNotifications(device, true);
			}
			else
				LogDeviceEventWarning("AddDevice ignored (duplicate add)", device);
		}

		/// <summary>
		/// Removes a device (and it's children) from the list of devices being published by this server, making them undiscoverable.
		/// </summary>
		/// <remarks>
		/// <para>Removing a device causes "byebye" notification messages to be sent immediately, advising clients of the device/service becoming unavailable. We recommend removing the device from the published list before shutting down the actual device/service, if possible.</para>
		/// <para>This method does nothing if the device was not found in the collection.</para>
		/// </remarks>
		/// <param name="device">The <see cref="SsdpDevice"/> instance to add.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="device"/> argument is null.</exception>
		public void RemoveDevice(SsdpRootDevice device)
		{
			if (device == null) throw new ArgumentNullException("device");

			ThrowIfDisposed();

			bool wasRemoved = false;
			TimeSpan minCacheTime = TimeSpan.Zero;
			lock (_Devices)
			{
				if (_Devices.Contains(device))
				{
					_Devices.Remove(device);
					wasRemoved = true;
					minCacheTime = GetMinimumNonZeroCacheLifetime();
				}
			}

			if (wasRemoved)
			{
				_MinCacheTime = minCacheTime;

				DisconnectFromDeviceEvents(device);

				LogDeviceEvent("Device Removed", device);

				SendByeByeNotifications(device, true);

				SetRebroadcastAliveNotificationsTimer(minCacheTime);
			}
			else
				LogDeviceEventWarning("RemoveDevice ignored (device not in publisher)", device);
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Returns a reference to the injected <see cref="ISsdpLogger"/> instance.
		/// </summary>
		/// <remarks>
		/// <para>Should never return null. If null was injected a reference to an internal null logger should be returned.</para>
		/// </remarks>
		protected ISsdpLogger Log
		{
			get { return _Log; }
		}

		/// <summary>
		/// Returns a read only list of devices being published by this instance.
		/// </summary>
		public IEnumerable<SsdpRootDevice> Devices
		{
			get
			{
				return _ReadOnlyDevices;
			}
		}

		/// <summary>
		/// If true (default) treats root devices as both upnp:rootdevice and pnp:rootdevice types.
		/// </summary>
		/// <remarks>
		/// <para>Enabling this option will cause devices to show up in Microsoft Windows Explorer's network screens (if discovery is enabled etc.). Windows Explorer appears to search only for pnp:rootdeivce and not upnp:rootdevice.</para>
		/// <para>If false, the system will only use upnp:rootdevice for notifiation broadcasts and and search responses, which is correct according to the UPnP/SSDP spec.</para>
		/// </remarks>
		[Obsolete("Set StandardsMode to SsdpStandardsMode.Relaxed instead.")]
		public bool SupportPnpRootDevice
		{
			get { return _SupportPnpRootDevice; }
			set
			{
				if (_SupportPnpRootDevice != value)
				{
					_SupportPnpRootDevice = value;
					_Log.LogInfo("SupportPnpRootDevice set to " + value.ToString());
				}
			}
		}

		/// <summary>
		/// Sets or returns a value from the <see cref="SsdpStandardsMode"/> controlling how strictly the publisher obeys the SSDP standard.
		/// </summary>
		/// <remarks>
		/// <para>Using relaxed mode will process search requests even if the MX header is missing.</para>
		/// </remarks>
		public SsdpStandardsMode StandardsMode
		{
			get { return _StandardsMode; }
			set
			{
				if (_StandardsMode != value)
				{
					_StandardsMode = value;
					_Log.LogInfo("StandardsMode set to " + value.ToString());
				}
			}
		}

		/// <summary>
		/// Sets or returns a fixed interval at which alive notifications for services exposed by this publisher instance are broadcast.
		/// </summary>
		/// <remarks>
		/// <para>If this is set to <see cref="TimeSpan.Zero"/> then the system will follow the process recommended 
		/// by the SSDP spec and calculate a randomised interval based on the cache life times of the published services.
		/// The default and recommended value is TimeSpan.Zero.
		/// </para>
		/// <para>While (zero and) any positive <see cref="TimeSpan"/> value are allowed, the SSDP specification says 
		/// notifications should not be broadcast more often than 15 minutes. If you wish to remain compatible with the SSDP
		/// specification, do not set this property to a value greater than zero but less than 15 minutes.
		/// </para>
		/// </remarks>
		public TimeSpan NotificationBroadcastInterval
		{
			get { return _NotificationBroadcastInterval; }
			set
			{
				if (value.TotalSeconds < 0) throw new ArgumentException("Cannot be less than zero.", nameof(value));

				if (_NotificationBroadcastInterval != value)
				{
					_NotificationBroadcastInterval = value;
					_Log.LogInfo("NotificationBroadcastInterval set to " + value.ToString());
					SetRebroadcastAliveNotificationsTimer(_MinCacheTime);
				}
			}
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Stops listening for requests, stops sending periodic broadcasts, disposes all internal resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_Log.LogInfo("Publisher disposed.");

				DisposeRebroadcastTimer();

				var commsServer = _CommsServer;
				_CommsServer = null;

				if (commsServer != null)
				{
					commsServer.RequestReceived -= this.CommsServer_RequestReceived;
					if (!commsServer.IsShared)
						commsServer.Dispose();
				}

				foreach (var device in this.Devices)
				{
					DisconnectFromDeviceEvents(device);
				}

				_RecentSearchRequests = null;
			}
		}

		#endregion

		#region Private Methods

		#region Search Related Methods

		private void ProcessSearchRequest(string mx, string searchTarget, UdpEndPoint endPoint)
		{
			if (String.IsNullOrEmpty(searchTarget))
			{
				_Log.LogWarning(String.Format("Invalid search request received From {0}, Target is null/empty.", endPoint.ToString()));
				return;
			}

			_Log.LogInfo(String.Format("Search Request Received From {0}, Target = {1}", endPoint.ToString(), searchTarget));

			if (IsDuplicateSearchRequest(searchTarget, endPoint))
			{
				Log.LogWarning("Search Request is Duplicate, ignoring.");
				return;
			}

			//Wait on random interval up to MX, as per SSDP spec.
			//Also, as per UPnP 1.1/SSDP spec ignore missing/bank MX header (strict mode only). If over 120, assume random value between 0 and 120.
			//Using 16 as minimum as that's often the minimum system clock frequency anyway.
			int maxWaitInterval = 0;
			if (String.IsNullOrEmpty(mx))
			{
				//Windows Explorer is poorly behaved and doesn't supply an MX header value.
				if (IsWindowsExplorerSupportEnabled)
					mx = "1";
				else
				{
					_Log.LogWarning("Search Request ignored due to missing MX header. Set StandardsMode to relaxed to respond to these requests.");
					return;
				}
			}

			if (!Int32.TryParse(mx, out maxWaitInterval) || maxWaitInterval <= 0) return;

			if (maxWaitInterval > 120)
				maxWaitInterval = _Random.Next(0, 120);

			//Do not block synchronously as that may tie up a threadpool thread for several seconds.
			TaskEx.Delay(_Random.Next(16, (maxWaitInterval * 1000))).ContinueWith((parentTask) =>
			{
				//Copying devices to local array here to avoid threading issues/enumerator exceptions.
				IEnumerable<SsdpDevice> devices = null;
				devices = GetDevicesMatchingSearchTarget(searchTarget, devices);

				if (devices != null)
					SendSearchResponses(searchTarget, endPoint, devices);
				else
					_Log.LogWarning("Sending search responses for 0 devices (no matching targets).");
			});
		}

		private IEnumerable<SsdpDevice> GetDevicesMatchingSearchTarget(string searchTarget, IEnumerable<SsdpDevice> devices)
		{
			lock (_Devices)
			{
				if (String.Compare(SsdpConstants.SsdpDiscoverAllSTHeader, searchTarget, StringComparison.OrdinalIgnoreCase) == 0)
					devices = GetAllDevicesAsFlatEnumerable().ToArray();
				else if (String.Compare(SsdpConstants.UpnpDeviceTypeRootDevice, searchTarget, StringComparison.OrdinalIgnoreCase) == 0 || (IsWindowsExplorerSupportEnabled && String.Compare(SsdpConstants.PnpDeviceTypeRootDevice, searchTarget, StringComparison.OrdinalIgnoreCase) == 0))
					devices = _Devices.ToArray();
				else if (searchTarget.Trim().StartsWith("uuid:", StringComparison.OrdinalIgnoreCase))
				{
					devices = (
											from device
											in GetAllDevicesAsFlatEnumerable()
											where String.Compare(device.Uuid, searchTarget.Substring(5), StringComparison.OrdinalIgnoreCase) == 0
											select device
										).ToArray();
				}
				else if (searchTarget.StartsWith("urn:", StringComparison.OrdinalIgnoreCase))
				{
					if (searchTarget.Contains(":service:"))
					{
						devices = 
						(
							from device in GetAllDevicesAsFlatEnumerable()
							where
							(
								from s in
								device.Services
								where String.Compare(s.FullServiceType, searchTarget, StringComparison.OrdinalIgnoreCase) == 0
								select s
							).Any()
							select device
						).ToArray();
					}
					else
					{
						devices =
						(
							from device
							in GetAllDevicesAsFlatEnumerable()
							where String.Compare(device.FullDeviceType, searchTarget, StringComparison.OrdinalIgnoreCase) == 0
							select device
						).ToArray();
					}
				}
			}

			return devices;
		}

		private bool IsWindowsExplorerSupportEnabled
		{
			get
			{
#pragma warning disable CS0618 // Type or member is obsolete
				return SupportPnpRootDevice || IsRelaxedStandardsMode;
#pragma warning restore CS0618 // Type or member is obsolete
			}
		}

		private bool IsRelaxedStandardsMode
		{
			get
			{
				return this.StandardsMode != SsdpStandardsMode.Strict;
			}
		}

		private IEnumerable<SsdpDevice> GetAllDevicesAsFlatEnumerable()
		{
			return _Devices.Union(_Devices.SelectManyRecursive<SsdpDevice>((d) => d.Devices));
		}

		private void SendSearchResponses(string searchTarget, UdpEndPoint endPoint, IEnumerable<SsdpDevice> devices)
		{
			_Log.LogInfo(String.Format("Sending search (target = {1}) responses for {0} devices", devices.Count(), searchTarget));

			if (searchTarget.Contains(":service:"))
			{
				foreach (var device in devices)
				{
					SendServiceSearchResponses(device, searchTarget, endPoint);
				}
			}
			else
			{
				foreach (var device in devices)
				{
					SendDeviceSearchResponses(device, searchTarget, endPoint);
				}
			}
		}

		private void SendDeviceSearchResponses(SsdpDevice device, string searchTarget, UdpEndPoint endPoint)
		{
			//http://www.upnp.org/specs/arch/UPnP-arch-DeviceArchitecture-v1.0-20080424.pdf - page 21
			//For ssdp:all - Respond 3+2d+k times for a root device with d embedded devices and s embedded services but only k distinct service types 
			//Root devices - Respond once (special handling when in related/Win Explorer support mode)
			//Udn (uuid) - Response once
			//Device type - response once
			//Service type - respond once per service type 

			bool isRootDevice = (device as SsdpRootDevice) != null;
			bool sendAll = searchTarget == SsdpConstants.SsdpDiscoverAllSTHeader;
			bool sendRootDevices = searchTarget == SsdpConstants.UpnpDeviceTypeRootDevice || searchTarget == SsdpConstants.PnpDeviceTypeRootDevice;

			if (isRootDevice && (sendAll || sendRootDevices))
			{
				SendSearchResponse(SsdpConstants.UpnpDeviceTypeRootDevice, device, GetUsn(device.Udn, SsdpConstants.UpnpDeviceTypeRootDevice), endPoint);
				if (IsWindowsExplorerSupportEnabled)
					SendSearchResponse(SsdpConstants.PnpDeviceTypeRootDevice, device, GetUsn(device.Udn, SsdpConstants.PnpDeviceTypeRootDevice), endPoint);
			}

			if (sendAll || searchTarget.StartsWith("uuid:", StringComparison.Ordinal))
				SendSearchResponse(device.Udn, device, device.Udn, endPoint);

			if (sendAll || searchTarget.Contains(":device:"))
				SendSearchResponse(device.FullDeviceType, device, GetUsn(device.Udn, device.FullDeviceType), endPoint);

			if (searchTarget == SsdpConstants.SsdpDiscoverAllSTHeader)
			{
				//Send 1 search response for each unique service type for all devices found
				var serviceTypes =
					(
						from s
						in device.Services
						select s.FullServiceType
					).Distinct().ToArray();

				foreach (var st in serviceTypes)
				{
					SendServiceSearchResponses(device, st, endPoint);
				}
			}
		}

		private void SendServiceSearchResponses(SsdpDevice device, string searchTarget, UdpEndPoint endPoint)
		{
			//uuid:device-UUID::urn:domain-name:service:serviceType:ver 
			SendSearchResponse(searchTarget, device, device.Udn + "::" + searchTarget, endPoint);
		}

		private static string GetUsn(string udn, string fullDeviceType)
		{
			return String.Format("{0}::{1}", udn, fullDeviceType);
		}

		private void SendSearchResponse(string searchTarget, SsdpDevice device, string uniqueServiceName, UdpEndPoint endPoint)
		{
			var rootDevice = device.ToRootDevice();

			var additionalheaders = FormatCustomHeadersForResponse(device);

			var message = String.Format(DeviceSearchResponseMessageFormat,
					CacheControlHeaderFromTimeSpan(rootDevice),
					searchTarget,
					uniqueServiceName,
					rootDevice.Location,
					_OSName,
					_OSVersion,
					ServerVersion,
					DateTime.UtcNow.ToString("r"),
					additionalheaders
				);

			_CommsServer.SendMessage(System.Text.UTF8Encoding.UTF8.GetBytes(message), endPoint);

			LogDeviceEventVerbose(String.Format("Sent search response ({0}) to {1}", uniqueServiceName, endPoint.ToString()), device);
		}

		private bool IsDuplicateSearchRequest(string searchTarget, UdpEndPoint endPoint)
		{
			var isDuplicateRequest = false;

			var newRequest = new SearchRequest() { EndPoint = endPoint, SearchTarget = searchTarget, Received = DateTime.UtcNow };
			lock (_RecentSearchRequests)
			{
				if (_RecentSearchRequests.ContainsKey(newRequest.Key))
				{
					var lastRequest = _RecentSearchRequests[newRequest.Key];
					if (lastRequest.IsOld())
						_RecentSearchRequests[newRequest.Key] = newRequest;
					else
						isDuplicateRequest = true;
				}
				else
				{
					_RecentSearchRequests.Add(newRequest.Key, newRequest);
					if (_RecentSearchRequests.Count > 10)
						CleanUpRecentSearchRequestsAsync();
				}
			}

			return isDuplicateRequest;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "t", Justification = "Capturing task to local variable avoids compiler warning, but value is otherwise not required.")]
		private void CleanUpRecentSearchRequestsAsync()
		{
			var t = TaskEx.Run(() =>
				{
					lock (_RecentSearchRequests)
					{
						foreach (var requestKey in (from r in _RecentSearchRequests where r.Value.IsOld() select r.Key).ToArray())
						{
							_RecentSearchRequests.Remove(requestKey);
						}
					}
				});
		}

		#endregion

		#region Notification Related Methods

		#region Alive

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void SendAllAliveNotifications(object state)
		{
			try
			{
				if (IsDisposed) return;

				try
				{
					//Only dispose the timer so it gets re-created if we're following
					//the SSDP Spec and randomising the broadcast interval.
					//If we're using a fixed interval, no need to kill the timer as it's 
					//already set to go off on the correct interval.
					if (_NotificationBroadcastInterval == TimeSpan.Zero)
						DisposeRebroadcastTimer();
				}
				finally
				{
					// Must reset this here, otherwise if the next reset interval
					// is calculated to be the same as the previous one we won't
					// reset the timer.
					// Reset it to _NotificationBroadcastInterval which is either TimeSpan.Zero 
					// which will cause the system to calculate a new random interval, or it's the 
					// current fixed interval which is fine.
					_RebroadcastAliveNotificationsTimeSpan = _NotificationBroadcastInterval;
				}

				_Log.LogInfo("Sending Alive Notifications For All Devices");

				_LastNotificationTime = DateTime.Now;

				IEnumerable<SsdpRootDevice> devices;
				lock (_Devices)
				{
					devices = _Devices.ToArray();
				}

				foreach (var device in devices)
				{
					if (IsDisposed) return;

					SendAliveNotifications(device, true);
				}
			}
			catch (Exception ex)
			{
				_Log.LogError("Publisher stopped, exception " + ex.Message);
				Dispose();
			}
			finally
			{
				if (!this.IsDisposed)
					SetRebroadcastAliveNotificationsTimer(_MinCacheTime);
			}
		}

		private void SendAliveNotifications(SsdpDevice device, bool isRoot)
		{
			if (isRoot)
			{
				SendAliveNotification(device, SsdpConstants.UpnpDeviceTypeRootDevice, GetUsn(device.Udn, SsdpConstants.UpnpDeviceTypeRootDevice));
#pragma warning disable CS0618 // Type or member is obsolete
				if (this.SupportPnpRootDevice)
#pragma warning restore CS0618 // Type or member is obsolete
					SendAliveNotification(device, SsdpConstants.PnpDeviceTypeRootDevice, GetUsn(device.Udn, SsdpConstants.PnpDeviceTypeRootDevice));
			}

			SendAliveNotification(device, device.Udn, device.Udn);
			SendAliveNotification(device, device.FullDeviceType, GetUsn(device.Udn, device.FullDeviceType));

			foreach (var service in device.Services)
			{
				SendAliveNotification(device, service);
			}

			foreach (var childDevice in device.Devices)
			{
				SendAliveNotifications(childDevice, false);
			}
		}

		private void SendAliveNotification(SsdpDevice device, string notificationType, string uniqueServiceName)
		{
			string multicastIpAddress = _CommsServer.DeviceNetworkType.GetMulticastIPAddress();

			var multicastMessage = BuildAliveMessage(device, notificationType, uniqueServiceName, multicastIpAddress);

			_CommsServer.SendMessage(multicastMessage, new UdpEndPoint
			{
				IPAddress = multicastIpAddress,
				Port = SsdpConstants.MulticastPort
			});

			LogDeviceEvent(String.Format("Sent alive notification NT={0}, USN={1}", notificationType, uniqueServiceName), device);
		}

		private void SendAliveNotification(SsdpDevice device, SsdpService service)
		{
			SendAliveNotification(device, service.FullServiceType, device.Udn + "::" + service.FullServiceType);
		}

		private byte[] BuildAliveMessage(SsdpDevice device, string notificationType, string uniqueServiceName, string hostAddress)
		{
			var rootDevice = device.ToRootDevice();

			var additionalheaders = FormatCustomHeadersForResponse(device);

			return System.Text.UTF8Encoding.UTF8.GetBytes
			(
				String.Format
				(
					AliveNotificationMessageFormat,
					notificationType,
					uniqueServiceName,
					rootDevice.Location,
					CacheControlHeaderFromTimeSpan(rootDevice),
					_OSName,
					_OSVersion,
					ServerVersion,
					DateTime.UtcNow.ToString("r"),
					hostAddress,
					SsdpConstants.MulticastPort,
					additionalheaders
				)
			);
		}

		#endregion

		#region ByeBye

		private void SendByeByeNotifications(SsdpDevice device, bool isRoot)
		{
			if (isRoot)
			{
				SendByeByeNotification(device, SsdpConstants.UpnpDeviceTypeRootDevice, GetUsn(device.Udn, SsdpConstants.UpnpDeviceTypeRootDevice));
#pragma warning disable CS0618 // Type or member is obsolete
				if (this.SupportPnpRootDevice)
#pragma warning restore CS0618 // Type or member is obsolete
					SendByeByeNotification(device, "pnp:rootdevice", GetUsn(device.Udn, "pnp:rootdevice"));
			}

			SendByeByeNotification(device, device.Udn, device.Udn);
			SendByeByeNotification(device, String.Format("urn:{0}", device.FullDeviceType), GetUsn(device.Udn, device.FullDeviceType));

			foreach (var service in device.Services)
			{
				SendByeByeNotification(device, service);
			}

			foreach (var childDevice in device.Devices)
			{
				SendByeByeNotifications(childDevice, false);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "byebye", Justification = "Correct value for this type of notification in SSDP.")]
		private void SendByeByeNotification(SsdpDevice device, string notificationType, string uniqueServiceName)
		{
			string multicastIpAddress = _CommsServer.DeviceNetworkType.GetMulticastIPAddress();

			var multicastMessage = BuildByeByeMessage(notificationType, uniqueServiceName, multicastIpAddress);

			_CommsServer.SendMessage(multicastMessage, new UdpEndPoint
			{
				IPAddress = multicastIpAddress,
				Port = SsdpConstants.MulticastPort
			});

			LogDeviceEvent(String.Format("Sent byebye notification, NT={0}, USN={1}", notificationType, uniqueServiceName), device);
		}

		private void SendByeByeNotification(SsdpDevice device, SsdpService service)
		{
			SendByeByeNotification(device, service.FullServiceType, device.Udn + "::" + service.FullServiceType);
		}

		private byte[] BuildByeByeMessage(string notificationType, string uniqueServiceName, string hostAddress)
		{
			var message = String.Format(ByeByeNotificationMessageFormat,
					notificationType,
					uniqueServiceName,
					_OSName,
					_OSVersion,
					ServerVersion,
					DateTime.UtcNow.ToString("r"),
					hostAddress,
					SsdpConstants.MulticastPort
				);

			return System.Text.UTF8Encoding.UTF8.GetBytes(message);
		}

		#endregion

		#region Rebroadcast Timer

		private void DisposeRebroadcastTimer()
		{
			var timer = _RebroadcastAliveNotificationsTimer;
			_RebroadcastAliveNotificationsTimer = null;
			if (timer != null)
				timer.Dispose();
		}

		private void SetRebroadcastAliveNotificationsTimer(TimeSpan minCacheTime)
		{
			TimeSpan rebroadCastInterval = TimeSpan.Zero;
			if (this.NotificationBroadcastInterval != TimeSpan.Zero)
			{
				if (_RebroadcastAliveNotificationsTimeSpan == this.NotificationBroadcastInterval) return;

				rebroadCastInterval = this.NotificationBroadcastInterval;
			}
			else
			{
				if (minCacheTime == _RebroadcastAliveNotificationsTimeSpan) return;
				if (minCacheTime == TimeSpan.Zero) return;

				// According to UPnP/SSDP spec, we should randomise the interval at 
				// which we broadcast notifications, to help with network congestion.
				// Specs also advise to choose a random interval up to *half* the cache time.
				// Here we do that, but using the minimum non-zero cache time of any device we are publishing.
				rebroadCastInterval = new TimeSpan(Convert.ToInt64((_Random.Next(1, 50) / 100D) * (minCacheTime.Ticks / 2)));
			}

			DisposeRebroadcastTimer();

			// If we were already setup to rebroadcast sometime in the future,
			// don't just blindly reset the next broadcast time to the new interval
			// as repeatedly changing the interval might end up causing us to over
			// delay in sending the next one.
			var nextBroadcastInterval = rebroadCastInterval;
			if (_LastNotificationTime != DateTime.MinValue)
			{
				nextBroadcastInterval = rebroadCastInterval.Subtract(DateTime.Now.Subtract(_LastNotificationTime));
				if (nextBroadcastInterval.Ticks < 0)
					nextBroadcastInterval = TimeSpan.Zero;
				else if (nextBroadcastInterval > rebroadCastInterval)
					nextBroadcastInterval = rebroadCastInterval;
			}

			_RebroadcastAliveNotificationsTimeSpan = rebroadCastInterval;
			_RebroadcastAliveNotificationsTimer = new System.Threading.Timer(SendAllAliveNotifications, null, nextBroadcastInterval, rebroadCastInterval);

			_Log.LogInfo(String.Format("Rebroadcast Interval = {0}, Next Broadcast At = {1}", rebroadCastInterval.ToString(), nextBroadcastInterval.ToString()));
		}

		private TimeSpan GetMinimumNonZeroCacheLifetime()
		{
			var nonzeroCacheLifetimesQuery = (from device
																				in _Devices
											  where device.CacheLifetime != TimeSpan.Zero
											  select device.CacheLifetime);

			if (nonzeroCacheLifetimesQuery.Any())
				return nonzeroCacheLifetimesQuery.Min();
			else
				return TimeSpan.Zero;
		}

		#endregion

		#endregion

		private static string GetFirstHeaderValue(System.Net.Http.Headers.HttpRequestHeaders httpRequestHeaders, string headerName)
		{
			string retVal = null;
			IEnumerable<String> values = null;
			if (httpRequestHeaders.TryGetValues(headerName, out values) && values != null)
				retVal = values.FirstOrDefault();

			return retVal;
		}

		private static string CacheControlHeaderFromTimeSpan(SsdpRootDevice device)
		{
			if (device.CacheLifetime == TimeSpan.Zero)
				return "CACHE-CONTROL: no-cache";
			else
				return String.Format("CACHE-CONTROL: public, max-age={0}", device.CacheLifetime.TotalSeconds);
		}

		private void LogDeviceEvent(string text, SsdpDevice device)
		{
			_Log.LogInfo(GetDeviceEventLogMessage(text, device));
		}

		private void LogDeviceEventWarning(string text, SsdpDevice device)
		{
			_Log.LogWarning(GetDeviceEventLogMessage(text, device));
		}

		private void LogDeviceEventVerbose(string text, SsdpDevice device)
		{
			_Log.LogVerbose(GetDeviceEventLogMessage(text, device));
		}

		private static string GetDeviceEventLogMessage(string text, SsdpDevice device)
		{
			var rootDevice = device as SsdpRootDevice;
			if (rootDevice != null)
				return text + " " + device.DeviceType + " - " + device.Uuid + " - " + rootDevice.Location;
			else
				return text + " " + device.DeviceType + " - " + device.Uuid;
		}

		private void ConnectToDeviceEvents(SsdpDevice device)
		{
			device.DeviceAdded += device_DeviceAdded;
			device.DeviceRemoved += device_DeviceRemoved;
			device.ServiceAdded += device_ServiceAdded;
			device.ServiceRemoved += device_ServiceRemoved;

			foreach (var childDevice in device.Devices)
			{
				ConnectToDeviceEvents(childDevice);
			}
		}

		private void DisconnectFromDeviceEvents(SsdpDevice device)
		{
			device.DeviceAdded -= device_DeviceAdded;
			device.DeviceRemoved -= device_DeviceRemoved;
			device.ServiceAdded -= device_ServiceAdded;
			device.ServiceRemoved -= device_ServiceRemoved;

			foreach (var childDevice in device.Devices)
			{
				DisconnectFromDeviceEvents(childDevice);
			}
		}

		private static string FormatCustomHeadersForResponse(SsdpDevice device)
		{
			if (device.CustomResponseHeaders.Count == 0) return String.Empty;

			StringBuilder returnValue = new StringBuilder();
			foreach (var header in device.CustomResponseHeaders)
			{
				returnValue.Append("\r\n");

				returnValue.Append(header.ToString());
			}
			return returnValue.ToString();
		}

		private static bool DeviceHasServiceOfType(SsdpDevice device, string fullServiceType)
		{
			int retries = 0;
			while (retries < 5)
			{
				try
				{
					return (from s in device.Services where s.FullServiceType == fullServiceType select s).Any();
				}
				catch (InvalidOperationException) // Collection modified during enumeration
				{
					retries++;
				}
			}

			return true;
		}

		#endregion

		#region Event Handlers

		private void device_DeviceAdded(object sender, DeviceEventArgs e)
		{
			SendAliveNotifications(e.Device, false);
			ConnectToDeviceEvents(e.Device);
		}

		private void device_DeviceRemoved(object sender, DeviceEventArgs e)
		{
			SendByeByeNotifications(e.Device, false);
			DisconnectFromDeviceEvents(e.Device);
		}

		private void device_ServiceAdded(object sender, ServiceEventArgs e)
		{
			//Technically we should only do this once per service type,
			//but if we add services during runtime there is no way to
			//notify anyone except by resending this notification.
			_Log.LogInfo(String.Format("Service added: {0} ({1})", e.Service.ServiceId, e.Service.FullServiceType));

			SendAliveNotification((SsdpDevice)sender, e.Service);
		}

		private void device_ServiceRemoved(object sender, ServiceEventArgs e)
		{
			_Log.LogInfo(String.Format("Service removed: {0} ({1})", e.Service.ServiceId, e.Service.FullServiceType));

			var device = (SsdpDevice)sender;
			//Only say this service type has disappeared if there are no 
			//services of this type left.
			if (!DeviceHasServiceOfType(device, e.Service.FullServiceType))
				SendByeByeNotification(device, e.Service);
		}

		private void CommsServer_RequestReceived(object sender, RequestReceivedEventArgs e)
		{
			if (this.IsDisposed) return;

			if (e.Message.Method.Method == SsdpConstants.MSearchMethod)
			{
				//According to SSDP/UPnP spec, ignore message if missing these headers.
				if (!e.Message.Headers.Contains("MX") && !IsRelaxedStandardsMode)
					_Log.LogWarning("Ignoring search request - missing MX header. Set StandardsMode to relaxed to process these search requests.");
				else if (!e.Message.Headers.Contains("MAN") && !IsRelaxedStandardsMode)
					_Log.LogWarning("Ignoring search request - missing MAN header. Set StandardsMode to relaxed to process these search requests.");
				else
					ProcessSearchRequest(GetFirstHeaderValue(e.Message.Headers, "MX"), GetFirstHeaderValue(e.Message.Headers, "ST"), e.ReceivedFrom);
			}
			else if (String.Compare(e.Message.Method.Method, "NOTIFY", StringComparison.OrdinalIgnoreCase) != 0)
				_Log.LogWarning(String.Format("Unknown request \"{0}\"received, ignoring.", e.Message.Method.Method));
		}

		#endregion

		#region Private Classes

		private class SearchRequest
		{
			public UdpEndPoint EndPoint { get; set; }
			public DateTime Received { get; set; }
			public string SearchTarget { get; set; }

			public string Key
			{
				get { return this.SearchTarget + ":" + this.EndPoint.ToString(); }
			}

			public bool IsOld()
			{
				return DateTime.UtcNow.Subtract(this.Received).TotalMilliseconds > 500;
			}
		}

		#endregion

	}
}