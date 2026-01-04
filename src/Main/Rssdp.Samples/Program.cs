using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rssdp;
using System.Net;
using Rssdp.Infrastructure;

namespace Rssdp.Samples
{
	class Program
	{
		private static SsdpDevicePublisher _DevicePublisher;
		private static SsdpDeviceLocator _BroadcastListener;
		private static HttpListener _HttpServer;

		private static System.Diagnostics.ActivityListener _ActivityListener;

		private static bool _TracingEnabled;

		static void Main()
		{
			ShowMenu();
		}

		private static void ShowMenu()
		{
			WriteOutOptions();

			var key = new ConsoleKeyInfo();

			while (key.Key == 0 || String.Compare(key.KeyChar.ToString(), "X", true) != 0)
			{
				Console.WriteLine();
				Console.Write("Enter command: ");
				key = Console.ReadKey();
				Console.WriteLine();
				Console.WriteLine();

				ProcessCommand(key.KeyChar.ToString().ToUpperInvariant());
			}
		}

		private static void WriteOutOptions()
		{
			Console.WriteLine("RSSDP Samples");
			Console.WriteLine("Commands");
			Console.WriteLine("--------------------------------");
			Console.WriteLine("? to display menu");
			Console.WriteLine("P to publish devices");
			Console.WriteLine("R to search for root devices");
			Console.WriteLine("A to search for all devices");
			Console.WriteLine("B to search for basic devices");
			Console.WriteLine("U to search for published device by UUID");
			Console.WriteLine("L to listen for notifications");
			Console.WriteLine("T to toggle diagnostic activity tracing (currently: " + (_TracingEnabled ? "ON" : "OFF") + ")");
			Console.WriteLine("M to search multiple networks (adapters)");
			Console.WriteLine("X to exit");
			Console.WriteLine();
		}

		private static void ProcessCommand(string command)
		{
			switch (command)
			{
				case "P":
					PublishDevices();
					break;

				case "A":
					SearchForAllDevices().Wait();
					break;

				case "R":
					SearchForRootDevices().Wait();
					break;

				case "B":
					SearchForBasicDevices().Wait();
					break;

				case "U":
					SearchForDevicesByUuid().Wait();
					break;

				case "M":
					SearchForMultipleAdapters().Wait();
					break;

				case "L":
					ListenForBroadcasts();
					break;

				case "T":
					ToggleTracing();
					break;

				case "?":
					WriteOutOptions();
					break;

				default:
					Console.WriteLine("Unknown command. Press ? for a list of valid commands.");
					break;
			}
		}

		private static void ToggleTracing()
		{
			if (_TracingEnabled)
			{
				DisableTracing();
			}
			else
			{
				EnableTracing();
			}
		}

		private static void EnableTracing()
		{
			if (_TracingEnabled) return;

			_ActivityListener = new System.Diagnostics.ActivityListener
			{
				ShouldListenTo = (source) =>
				{
					// Subscribe to RSSDP sources
					return source.Name == SsdpConstants.LocatorActivitySourceName || source.Name == SsdpConstants.PublisherActivitySourceName;
				},
				Sample = (ref System.Diagnostics.ActivityCreationOptions<System.Diagnostics.ActivityContext> options) => System.Diagnostics.ActivitySamplingResult.AllDataAndRecorded,
				ActivityStarted = (activity) =>
				{
					Console.ForegroundColor = ConsoleColor.DarkYellow;
					Console.WriteLine($"[Activity START] {activity.OperationName} | Source={activity.Source.Name} | Kind={activity.Kind} | Id={activity.Id}");
					WriteActivityTags(activity);
					Console.ForegroundColor = ConsoleColor.Gray;
				},
				ActivityStopped = (activity) =>
				{
					Console.ForegroundColor = ConsoleColor.DarkYellow;
					Console.WriteLine($"[Activity STOP ] {activity.OperationName} | Source={activity.Source.Name} | Kind={activity.Kind} | Id={activity.Id} | Status={activity.Status} | Duration={activity.Duration}");
					WriteActivityTags(activity);
					WriteActivityEvents(activity);
					Console.ForegroundColor = ConsoleColor.Gray;
				}
			};

			System.Diagnostics.ActivitySource.AddActivityListener(_ActivityListener);
			_TracingEnabled = true;
			Console.WriteLine("Diagnostic activity tracing ENABLED.");

		}

		private static void DisableTracing()
		{
			if (!_TracingEnabled) return;
			try
			{
				_ActivityListener?.Dispose();
			}
			catch { }
			finally
			{
				_ActivityListener = null;

				_TracingEnabled = false;
				Console.WriteLine("Diagnostic activity tracing DISABLED.");
			}
		}

		private static void WriteActivityTags(System.Diagnostics.Activity activity)
		{
			if (activity == null) return;
			foreach (var tag in activity.Tags)
			{
				Console.WriteLine($"  tag: {tag.Key} = {tag.Value}");
			}
		}

		private static void WriteActivityEvents(System.Diagnostics.Activity activity)
		{
			if (activity == null) return;
			foreach (var ev in activity.Events)
			{
				Console.WriteLine($"  event: {ev.Name} @ {ev.Timestamp.ToLocalTime()}");
				if (ev.Tags != null)
				{
					foreach (var tag in ev.Tags)
					{
						Console.WriteLine($"    {tag.Key} = {tag.Value}");
					}
				}
			}
		}

		private static void ListenForBroadcasts()
		{
			if (_BroadcastListener != null)
			{
				Console.WriteLine("Closing previous listener...");
				_BroadcastListener.DeviceAvailable -= BroadcastListener_DeviceAvailable;
				_BroadcastListener.DeviceUnavailable -= BroadcastListener_DeviceUnavailable;

				_BroadcastListener.StopListeningForNotifications();
				_BroadcastListener.Dispose();
			}

			Console.WriteLine("Starting broadcast listener");
			_BroadcastListener = new SsdpDeviceLocator();
			_BroadcastListener.DeviceAvailable += BroadcastListener_DeviceAvailable;
			_BroadcastListener.DeviceUnavailable += BroadcastListener_DeviceUnavailable;
			_BroadcastListener.StartListeningForNotifications();
			Console.WriteLine("Now listening for broadcasts");
		}

		static void BroadcastListener_DeviceUnavailable(object sender, DeviceUnavailableEventArgs e)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("ByeBye Broadcast: " + e.DiscoveredDevice.Usn + " @ " + e.DiscoveredDevice.DescriptionLocation);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		static void BroadcastListener_DeviceAvailable(object sender, DeviceAvailableEventArgs e)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Alive Broadcast: " + e.DiscoveredDevice.Usn + " @ " + e.DiscoveredDevice.DescriptionLocation);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		private static void PublishDevices()
		{
			if (_DevicePublisher != null)
			{
				Console.WriteLine("Stopping previous publisher.");
				_DevicePublisher.Dispose();
			}

			_HttpServer?.Close();

			// Create a device publisher
			_DevicePublisher = new SsdpDevicePublisher
			{
				//These settings make RSSDP play nicely with Windows Explorer
				//and some badly behaved clients.
				StandardsMode = SsdpStandardsMode.Relaxed
			};

			// Create the device(s) we want to publish.
			var url = new Uri("http://" + Environment.MachineName + ":8181/");

			var rootDevice = new SsdpRootDevice()
			{
				CacheLifetime = TimeSpan.FromMinutes(30),
				FriendlyName = "Sample RSSDP Device",
				Manufacturer = "RSSDP",
				ModelNumber = "123",
				ModelName = "RSSDP Sample Device",
				ModelDescription = "Test Device from RSSDP Console App",
				ManufacturerUrl = new Uri("https://github.com/Yortw/RSSDP"),
				SerialNumber = "123",
				Uuid = System.Guid.NewGuid().ToString(),
				UrlBase = url
			};
			rootDevice.CustomResponseHeaders.Add(new CustomHttpHeader("X-MachineName", Environment.MachineName));

			var service = new SsdpService()
			{
				Uuid = System.Guid.NewGuid().ToString(),
				ServiceType = "test-service-type",
				ServiceTypeNamespace = "rssdp-test-namespace",
				ControlUrl = new Uri("/test/control", UriKind.Relative),
				EventSubUrl = new Uri("/test/event", UriKind.Relative),
				ScpdUrl = new Uri("/test", UriKind.Relative)
			};
			rootDevice.AddService(service);

			rootDevice.Location = new Uri(url, "ddd");

			//Some 3rd party tools won't show the device unless they can get
			//the device description document, so this sample uses a really simple HTTP
			//server just to serve that.
			StartHttpServerForDdd(rootDevice, url);

			// Now publish by adding them to the publisher.
			_DevicePublisher.AddDevice(rootDevice);

			Console.WriteLine("Publishing devices: ");
			WriteOutDevices(rootDevice);
			Console.WriteLine();
		}

		private static void StartHttpServerForDdd(SsdpRootDevice rootDevice, Uri url)
		{
			_HttpServer = new HttpListener();
			var t = new System.Threading.Thread(ServeDeviceDescriptionDocument)
			{
				IsBackground = true
			};
			t.Start(rootDevice);

			rootDevice.UrlBase = url;
			_HttpServer.Prefixes.Add("http://+:8181/");
			try
			{
				_HttpServer.Start();
				Console.WriteLine("DDD Published at " + url.ToString());
			}
			catch (Exception)
			{
				Console.WriteLine("Permission denied starting http listener. Please run app as elevated admin, or run the following command from elevated command prompt, changing domain\\user to be a valid Windows user account");
				Console.WriteLine("netsh http add urlacl url=http://+:8181/MyUri user=DOMAIN\\user");
			}
		}

		private static void ServeDeviceDescriptionDocument(object rootDevice)
		{
			try
			{
				var device = rootDevice as SsdpRootDevice;
				var dddBuffer = System.Text.UTF8Encoding.UTF8.GetBytes(device.ToDescriptionDocument());

				while (device != null && _HttpServer != null)
				{
					var context = _HttpServer.GetContext();
					context.Response.OutputStream.Write(dddBuffer, 0, dddBuffer.Length);
					context.Response.OutputStream.Flush();
					context.Response.OutputStream.Close();
				}
			}
			catch
			{ }
		}

		private static void WriteOutDevices(SsdpDevice device)
		{
			Console.WriteLine(device.Udn + " - " + device.FullDeviceType);
			foreach (var childDevice in device.Devices)
			{
				WriteOutDevices(childDevice);
			}
		}

		private static async Task SearchForRootDevices()
		{
			Console.WriteLine("Searching for root devices...");

			using (var deviceLocator = new SsdpDeviceLocator())
			{
				var results = await deviceLocator.SearchAsync(Rssdp.Infrastructure.SsdpConstants.UpnpDeviceTypeRootDevice);
				foreach (var device in results)
				{
					WriteOutDevices(device);
				}
			}
		}

		private static async Task SearchForAllDevices()
		{
			Console.WriteLine("Searching for all devices...");

			using (var deviceLocator = new SsdpDeviceLocator())
			{
				var results = await deviceLocator.SearchAsync();
				foreach (var device in results)
				{
					WriteOutDevices(device);
				}
			}
		}

		private static async Task SearchForMultipleAdapters()
		{
			Console.WriteLine("Searching for all devices on multiple adapters...");

			var adapters = 
			(
				from a in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
				where a.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up
				&& a.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback
				&& a.SupportsMulticast
				&& !a.IsReceiveOnly
				&&
				(
					a.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet
					|| a.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211
					|| a.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.GigabitEthernet
					|| a.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.FastEthernetFx
					|| a.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.FastEthernetT
				)
				select a
			).ToList();

			var addresses = 
			(
				from ip in adapters.SelectMany((a) => a.GetIPProperties().UnicastAddresses)
				where ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
					|| ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
				select ip.Address.ToString()
			).ToList();

			var adapterListText = String.Join(", ", addresses);
			Console.WriteLine($"Searching on adapters with the following adddreses; {adapterListText}");

			using (var deviceLocator = new AggregateSsdpDeviceLocator(addresses))
			{
				var results = await deviceLocator.SearchAsync();
				foreach (var device in results)
				{
					WriteOutDevices(device);
				}
			}
		}

		private static async Task SearchForDevicesByUuid()
		{
			if (_DevicePublisher == null || !_DevicePublisher.Devices.Any())
			{
				Console.WriteLine("No devices being published. Use the (P)ublish command first.");
				return;
			}

			var uuid = _DevicePublisher.Devices.First().Uuid;
			Console.WriteLine("Searching for device with uuid of " + uuid);

			using (var deviceLocator = new SsdpDeviceLocator())
			{
				var results = await deviceLocator.SearchAsync("uuid:" + uuid);
				foreach (var device in results)
				{
					WriteOutDevices(device);
				}
			}
		}

		private static async Task SearchForBasicDevices()
		{
			Console.WriteLine("Searching for upnp basic devices...");

			using (var deviceLocator = new SsdpDeviceLocator())
			{
				var results = await deviceLocator.SearchAsync(String.Format("urn:{0}:device:{1}:1", Infrastructure.SsdpConstants.UpnpDeviceTypeNamespace, Infrastructure.SsdpConstants.UpnpDeviceTypeBasicDevice));
				foreach (var device in results)
				{
					WriteOutDevices(device);
				}
			}
		}

		private static void WriteOutDevices(DiscoveredSsdpDevice device)
		{
			Console.WriteLine(device.Usn + " - " + device.NotificationType + "\r\n\t @ " + device.DescriptionLocation);
		}

	}
}