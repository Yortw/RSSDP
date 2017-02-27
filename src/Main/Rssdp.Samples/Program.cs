using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rssdp;
using Rssdp.Aggregatable;
using Rssdp.Infrastructure;

namespace Rssdp.Samples
{
	class Program
	{
		private static SsdpDevicePublisher _devicePublisher;
		private static SsdpDeviceLocator _broadcastListener;
		private static IAggregatableDevicePublisher _aggregatableDevicePublisher;
		private static IAggregatableDeviceLocator _aggregatableDeviceLocator;

		static void Main(string[] args)
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
			Console.WriteLine("--------------------------------");
			Console.WriteLine("Q to publish device on all interfaces (aggregatable publisher)");
			Console.WriteLine("W to listen for notifications on all interfaces (aggregatable locator)");
			Console.WriteLine("E to search for all devices on all interfaces (aggregatable locator)");
			Console.WriteLine("--------------------------------");
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

				case "L":
					ListenForBroadcasts();
					break;

				case "Q":
					PublishOnAllInterfaces();
					break;

				case "W":
					ListenForBroadcastsOnAllInterfaces();
					break;

				case "E":
					SearchForAllDevicesOnAllInterfaces().Wait();
					break;

				case "?":
					WriteOutOptions();
					break;

				default:
					Console.WriteLine("Unknown command. Press ? for a list of valid commands.");
					break;
			}
		}

		private static void ListenForBroadcasts()
		{
			if (_broadcastListener != null)
			{
				Console.WriteLine("Closing previous listener...");
				_broadcastListener.DeviceAvailable -= _BroadcastListener_DeviceAvailable;
				_broadcastListener.DeviceUnavailable -= _BroadcastListener_DeviceUnavailable;

				_broadcastListener.StopListeningForNotifications();
				_broadcastListener.Dispose();
			}

			Console.WriteLine("Starting broadcast listener");
			_broadcastListener = new SsdpDeviceLocator();
			_broadcastListener.DeviceAvailable += _BroadcastListener_DeviceAvailable;
			_broadcastListener.DeviceUnavailable += _BroadcastListener_DeviceUnavailable;
			_broadcastListener.StartListeningForNotifications();
			Console.WriteLine("Now listening for broadcasts");
		}

		private static void PublishOnAllInterfaces()
		{
			if (_aggregatableDevicePublisher != null)
			{
				Console.WriteLine("Stopping previous publisher.");
				_aggregatableDevicePublisher.Dispose();
			}

			// Create a device publisher
			_aggregatableDevicePublisher = new AggregatableDevicePublisher(
				new NetworkInfoProvider(), 
				new SsdpDevicePublisherFactory(),
				45454);

			// Create the device(s) we want to publish.
			var rootDevice = new SsdpRootDevice
			{
				CacheLifetime = TimeSpan.FromMinutes(30),
				FriendlyName = "Sample RSSDP Device",
				Manufacturer = "RSSDP",
				ModelNumber = "123",
				ModelName = "RSSDP Sample Device",
				SerialNumber = "123",
				Uuid = System.Guid.NewGuid().ToString()
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

			// Now publish by adding them to the all publishers.
			_aggregatableDevicePublisher.AddDevice(rootDevice);

			Console.WriteLine($"Has been created a {_aggregatableDevicePublisher.Publishers.Count()} publishers");

			Console.WriteLine("Publishing devices: ");
			WriteOutDevices(rootDevice);
			Console.WriteLine();
		}

		private static void ListenForBroadcastsOnAllInterfaces()
		{
			if (_aggregatableDeviceLocator != null)
			{
				Console.WriteLine("Closing previous listener...");
				_aggregatableDeviceLocator.DeviceAvailable -= _AggregatableListener_DeviceAvailable;
				_aggregatableDeviceLocator.DeviceUnavailable -= _AggregatableListener_DeviceUnavailable;

				_aggregatableDeviceLocator.StopListening();
				_aggregatableDeviceLocator.Dispose();
			}

			Console.WriteLine("Starting broadcast listener on all interfaces");
			_aggregatableDeviceLocator = new AggregatableDeviceLocator();
			_aggregatableDeviceLocator.DeviceAvailable += _AggregatableListener_DeviceAvailable;
			_aggregatableDeviceLocator.DeviceUnavailable += _AggregatableListener_DeviceUnavailable;
			_aggregatableDeviceLocator.StartListening();
			Console.WriteLine("Now listening for broadcasts on all interfaces");
		}

		static void _AggregatableListener_DeviceAvailable(object sender, DeviceAvailableEventArgs e)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Alive Broadcast: " + e.DiscoveredDevice.Usn + " @ " + e.DiscoveredDevice.DescriptionLocation);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		static void _AggregatableListener_DeviceUnavailable(object sender, DeviceUnavailableEventArgs e)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("ByeBye Broadcast: " + e.DiscoveredDevice.Usn + " @ " + e.DiscoveredDevice.DescriptionLocation);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		static void _BroadcastListener_DeviceUnavailable(object sender, DeviceUnavailableEventArgs e)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("ByeBye Broadcast: " + e.DiscoveredDevice.Usn + " @ " + e.DiscoveredDevice.DescriptionLocation);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		static void _BroadcastListener_DeviceAvailable(object sender, DeviceAvailableEventArgs e)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Alive Broadcast: " + e.DiscoveredDevice.Usn + " @ " + e.DiscoveredDevice.DescriptionLocation);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		private static void PublishDevices()
		{
			if (_devicePublisher != null)
			{
				Console.WriteLine("Stopping previous publisher.");
				_devicePublisher.Dispose();
			}

			// Create a device publisher
			_devicePublisher = new SsdpDevicePublisher("100.72.6.253");

			// Create the device(s) we want to publish.
			var rootDevice = new SsdpRootDevice()
			{
				CacheLifetime = TimeSpan.FromMinutes(30),
				FriendlyName = "Sample RSSDP Device",
				Manufacturer = "RSSDP",
				ModelNumber = "123",
				ModelName = "RSSDP Sample Device",
				SerialNumber = "123",
				Uuid = System.Guid.NewGuid().ToString()
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

			// Now publish by adding them to the publisher.
			_devicePublisher.AddDevice(rootDevice);

			Console.WriteLine("Publishing devices: ");
			WriteOutDevices(rootDevice);
			Console.WriteLine();
		}

		private static void WriteOutDevices(SsdpDevice device)
		{
			Console.WriteLine(device.Udn + " - " + device.FullDeviceType);
			foreach (var childDevice in device.Devices)
			{
				WriteOutDevices(device);
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

		private static async Task SearchForAllDevicesOnAllInterfaces()
		{
			Console.WriteLine("Searching for all devices on all interfaces...");

			using (var deviceLocator = new AggregatableDeviceLocator())
			{
				var results = await deviceLocator.SearchAsync();
				foreach (var device in results)
					WriteOutDevices(device);
			}
		}

		private static async Task SearchForDevicesByUuid()
		{
			if (_devicePublisher == null || !_devicePublisher.Devices.Any())
			{
				Console.WriteLine("No devices being published. Use the (P)ublish command first.");
				return;
			}

			var uuid = _devicePublisher.Devices.First().Uuid;
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