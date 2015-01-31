using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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

		private bool _SupportPnpRootDevice;

		private IList<SsdpRootDevice> _Devices;
		private ReadOnlyEnumerable<SsdpRootDevice> _ReadOnlyDevices;

		private System.Threading.Timer _RebroadcastAliveNotificationsTimer;
		private TimeSpan _RebroadcastAliveNotificationsTimeSpan;
		private DateTime _LastNotificationTime;

		private IDictionary<string, SearchRequest> _RecentSearchRequests;
		private IUpnpDeviceValidator _DeviceValidator;

		private Random _Random;
		private TimeSpan _MinCacheTime;

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
LOCATION:{3}

"; //Blank line at end important, do not remove.


		private const string AliveNotificationMessageFormat = @"NOTIFY * HTTP/1.1
HOST: 239.255.255.250:1900
Date: {7}
NT: {0}
NTS: ssdp:alive
SERVER: {4}/{5} UPnP/1.0 RSSDP/{6}
USN: {1} 
LOCATION: {2}
{3}

"; //Blank line at end important, do not remove.

		private const string ByeByeNotificationMessageFormat = @"NOTIFY * HTTP/1.1
HOST: 239.255.255.250:1900
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
		protected SsdpDevicePublisherBase(ISsdpCommunicationsServer communicationsServer, string osName, string osVersion)
		{
			if (communicationsServer == null) throw new ArgumentNullException("communicationsServer");
			if (osName == null) throw new ArgumentNullException("osName");
			if (osName.Length == 0) throw new ArgumentException("osName cannot be an empty string.", "osName");
			if (osVersion == null) throw new ArgumentNullException("osVersion");
			if (osVersion.Length == 0) throw new ArgumentException("osVersion cannot be an empty string.", "osName");

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

			_CommsServer.BeginListeningForBroadcasts();
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "t", Justification="Capture task to local variable supresses compiler warning, but task is not really needed.")]
		public void AddDevice(SsdpRootDevice device)
		{
			if (device == null) throw new ArgumentNullException("device");

			ThrowIfDisposed();

			_DeviceValidator.ThrowIfDeviceInvalid(device);

			TimeSpan minCacheTime;
			bool wasAdded = false;
			lock (_Devices){
				if (!_Devices.Contains(device))
				{
					_Devices.Add(device);
					wasAdded = true;
					minCacheTime = GetMinimumNonZeroCacheLifetime();
				}
			}

			if (wasAdded)
			{
				_MinCacheTime = minCacheTime;

				ConnectToDeviceEvents(device);

				WriteTrace("Device Added", device);

				SetRebroadcastAliveNotificationsTimer(minCacheTime);

				SendAliveNotifications(device, true);
			}
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
			TimeSpan minCacheTime;
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

				WriteTrace("Device Removed", device);

				SendByeByeNotifications(device, true);

				SetRebroadcastAliveNotificationsTimer(minCacheTime);
			}
		}

		#endregion

		#region Public Properties

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
		public bool SupportPnpRootDevice
		{
			get { return _SupportPnpRootDevice; }
			set
			{
				_SupportPnpRootDevice = value;
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
				var commsServer = _CommsServer;
				_CommsServer = null;

				if (commsServer != null)
				{
					commsServer.RequestReceived -= this.CommsServer_RequestReceived;
					if (!commsServer.IsShared)
						commsServer.Dispose();
				}

				DisposeRebroadcastTimer();

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
				WriteTrace(String.Format("Invalid search request received From {0}, Target is null/empty.", endPoint.ToString()));
				return;
			}

			WriteTrace(String.Format("Search Request Received From {0}, Target = {1}", endPoint.ToString(), searchTarget));

			if (IsDuplicateSearchRequest(searchTarget, endPoint))
			{
				WriteTrace("Search Request is Duplicate, ignoring.");
				return;
			}

			//Wait on random interval up to MX, as per SSDP spec.
			//Also, as per UPnP 1.1/SSDP spec ignore missing/bank MX header. If over 120, assume random value between 0 and 120.
			//Using 16 as minimum as that's often the minimum system clock frequency anyway.
			int maxWaitInterval = 0;
			if (String.IsNullOrEmpty(mx))
			{
				//Windows Explorer is poorly behaved and doesn't supply an MX header value.
				if (this.SupportPnpRootDevice)
					mx = "1";
				else
					return;
			}

			if (!Int32.TryParse(mx, out maxWaitInterval) || maxWaitInterval <= 0) return;

			if (maxWaitInterval > 120)
				maxWaitInterval = _Random.Next(0, 120);

			//Do not block synchronously as that may tie up a threadpool thread for several seconds.
			TaskEx.Delay(_Random.Next(16, (maxWaitInterval * 1000))).ContinueWith((parentTask) =>
				{
					//Copying devices to local array here to avoid threading issues/enumerator exceptions.
					IEnumerable<SsdpDevice> devices = null;
					lock (_Devices)
					{
						if (String.Compare(SsdpConstants.SsdpDiscoverAllSTHeader, searchTarget, StringComparison.OrdinalIgnoreCase) == 0)
							devices = GetAllDevicesAsFlatEnumerable().ToArray();
						else if (String.Compare(SsdpConstants.UpnpDeviceTypeRootDevice, searchTarget, StringComparison.OrdinalIgnoreCase) == 0 || (this.SupportPnpRootDevice && String.Compare(SsdpConstants.PnpDeviceTypeRootDevice, searchTarget, StringComparison.OrdinalIgnoreCase) == 0))
							devices = _Devices.ToArray();
						else if (searchTarget.Trim().StartsWith("uuid:", StringComparison.OrdinalIgnoreCase))
							devices = (from device in GetAllDevicesAsFlatEnumerable() where String.Compare(device.Uuid, searchTarget.Substring(5), StringComparison.OrdinalIgnoreCase) == 0 select device).ToArray();
						else if (searchTarget.StartsWith("urn:", StringComparison.OrdinalIgnoreCase))
							devices = (from device in GetAllDevicesAsFlatEnumerable() where String.Compare(device.FullDeviceType, searchTarget.Substring(4), StringComparison.OrdinalIgnoreCase) == 0 select device).ToArray();
					}

					if (devices != null)
					{
						WriteTrace(String.Format("Sending {0} search responses", devices.Count()));

						foreach (var device in devices)
						{
							SendDeviceSearchResponses(device, endPoint);
						}
					}
					else
						WriteTrace(String.Format("Sending 0 search responses."));
				});
		}

		private IEnumerable<SsdpDevice> GetAllDevicesAsFlatEnumerable()
		{
			return _Devices.Union(_Devices.SelectManyRecursive<SsdpDevice>((d) => d.Devices));
		}

		private void SendDeviceSearchResponses(SsdpDevice device, UdpEndPoint endPoint)
		{
			bool isRootDevice = (device as SsdpRootDevice) != null;
			if (isRootDevice)
			{
				SendSearchResponse(SsdpConstants.UpnpDeviceTypeRootDevice, device, GetUsn(device.Udn, SsdpConstants.UpnpDeviceTypeRootDevice), endPoint);
				if (this.SupportPnpRootDevice)
					SendSearchResponse(SsdpConstants.PnpDeviceTypeRootDevice, device, GetUsn(device.Udn, SsdpConstants.PnpDeviceTypeRootDevice), endPoint);
			}

			SendSearchResponse(device.Udn, device, device.Udn, endPoint);

			SendSearchResponse(device.FullDeviceType, device, GetUsn(device.Udn, device.FullDeviceType), endPoint);
		}

		private static string GetUsn(string udn, string fullDeviceType)
		{
			return String.Format("{0}::{1}", udn, fullDeviceType);
		}

		private void SendSearchResponse(string searchTarget, SsdpDevice device, string uniqueServiceName, UdpEndPoint endPoint)
		{
			var message = String.Format(DeviceSearchResponseMessageFormat,
					CacheControlHeaderFromTimeSpan(device.RootDevice),
					searchTarget,
					uniqueServiceName,
					device.RootDevice.Location,
					_OSName,
					_OSVersion,
					ServerVersion,
					DateTime.UtcNow.ToString("r")
				);

			_CommsServer.SendMessage(System.Text.UTF8Encoding.UTF8.GetBytes(message), endPoint);

			WriteTrace(String.Format("Sent search response to " + endPoint.ToString()), device);
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

		private void SendAllAliveNotifications(object state)
		{
			try
			{
				if (IsDisposed) return;

				DisposeRebroadcastTimer();

				WriteTrace("Sending Alive Notifications For All Devices");

				_LastNotificationTime = DateTime.Now;

				IEnumerable<SsdpRootDevice> devices;
				lock (_Devices)
				{
					devices = _Devices.ToArray();
				}

				foreach (var device in devices)
				{
					SendAliveNotifications(device, true);
				}
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
				if (this.SupportPnpRootDevice)
					SendAliveNotification(device, SsdpConstants.PnpDeviceTypeRootDevice, GetUsn(device.Udn, SsdpConstants.PnpDeviceTypeRootDevice));
			}

			SendAliveNotification(device, device.Udn, device.Udn);
			SendAliveNotification(device, device.FullDeviceType, GetUsn(device.Udn, device.FullDeviceType));

			foreach (var childDevice in device.Devices)
			{
				SendAliveNotifications(childDevice, false);
			}
		}

		private void SendAliveNotification(SsdpDevice device, string notificationType, string uniqueServiceName)
		{
			var message = String.Format(AliveNotificationMessageFormat,
					notificationType,
					uniqueServiceName,
					device.RootDevice.Location,
					CacheControlHeaderFromTimeSpan(device.RootDevice),
					_OSName,
					_OSVersion,
					ServerVersion,
					DateTime.UtcNow.ToString("r")
				);

			_CommsServer.SendMulticastMessage(System.Text.UTF8Encoding.UTF8.GetBytes(message));

			WriteTrace(String.Format("Sent alive notification"), device);
		}

		#endregion

		#region ByeBye

		private void SendByeByeNotifications(SsdpDevice device, bool isRoot)
		{
			if (isRoot)
			{
				SendByeByeNotification(device, SsdpConstants.UpnpDeviceTypeRootDevice, GetUsn(device.Udn, SsdpConstants.UpnpDeviceTypeRootDevice));
				if (this.SupportPnpRootDevice)
					SendByeByeNotification(device, "pnp:rootdevice", GetUsn(device.Udn, "pnp:rootdevice"));
			}

			SendByeByeNotification(device, device.Udn, device.Udn);
			SendByeByeNotification(device, String.Format("urn:{0}", device.FullDeviceType), GetUsn(device.Udn, device.FullDeviceType));

			foreach (var childDevice in device.Devices)
			{
				SendByeByeNotifications(childDevice, false);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "byebye", Justification = "Correct value for this type of notification in SSDP.")]
		private void SendByeByeNotification(SsdpDevice device, string notificationType, string uniqueServiceName)
		{
			var message = String.Format(ByeByeNotificationMessageFormat,
					notificationType,
					uniqueServiceName,
					_OSName,
					_OSVersion,
					ServerVersion,
					DateTime.UtcNow.ToString("r")
				);

			_CommsServer.SendMulticastMessage(System.Text.UTF8Encoding.UTF8.GetBytes(message));

			WriteTrace(String.Format("Sent byebye notification"), device);
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
			if (minCacheTime == _RebroadcastAliveNotificationsTimeSpan) return;

			DisposeRebroadcastTimer();

			if (minCacheTime == TimeSpan.Zero) return;

			// According to UPnP/SSDP spec, we should randomise the interval at 
			// which we broadcast notifications, to help with network congestion.
			// Specs also advise to choose a random interval up to *half* the cache time.
			// Here we do that, but using the minimum non-zero cache time of any device we are publishing.
			var rebroadCastInterval = new TimeSpan(Convert.ToInt64((_Random.Next(1, 50) / 100D) *  (minCacheTime.Ticks / 2)));			

			// If we were already setup to rebroadcast someime in the future,
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

			WriteTrace(String.Format("Rebroadcast Interval = {0}, Next Broadcast At = {1}", rebroadCastInterval.ToString(), nextBroadcastInterval.ToString()));
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

		private static void WriteTrace(string text)
		{
			System.Diagnostics.Debug.WriteLine(text, "SSDP Publisher");
		}

		private static void WriteTrace(string text, SsdpDevice device)
		{
			var rootDevice = device as SsdpRootDevice;
			if (rootDevice != null)
				WriteTrace(text + " " + device.DeviceType + " - " + device.Uuid + " - " + rootDevice.Location);
			else
				WriteTrace(text + " " + device.DeviceType + " - " + device.Uuid);
		}

		private void ConnectToDeviceEvents(SsdpDevice device)
		{
			device.DeviceAdded += device_DeviceAdded;
			device.DeviceRemoved += device_DeviceRemoved;

			foreach (var childDevice in device.Devices)
			{
				ConnectToDeviceEvents(childDevice);
			}
		}

		private void DisconnectFromDeviceEvents(SsdpDevice device)
		{
			device.DeviceAdded -= device_DeviceAdded;
			device.DeviceRemoved -= device_DeviceRemoved;

			foreach (var childDevice in device.Devices)
			{
				DisconnectFromDeviceEvents(childDevice);
			}
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

		private void CommsServer_RequestReceived(object sender, RequestReceivedEventArgs e)
		{
			if (this.IsDisposed) return;

			if (e.Message.Method.Method == SsdpConstants.MSearchMethod)
			{
				//According to SSDP/UPnP spec, ignore message if missing these headers.
				if (!e.Message.Headers.Contains("MX"))
					WriteTrace("Ignoring search request - missing MX header.");
				else if (!e.Message.Headers.Contains("MAN"))
					WriteTrace("Ignoring search request - missing MAN header.");
				else
					ProcessSearchRequest(GetFirstHeaderValue(e.Message.Headers, "MX"), GetFirstHeaderValue(e.Message.Headers, "ST"), e.ReceivedFrom);
			}
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