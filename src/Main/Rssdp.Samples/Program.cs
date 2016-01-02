using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rssdp;

namespace Rssdp.Samples
{
	class Program
	{
		private static SsdpDevicePublisher _DevicePublisher;
		private static SsdpDeviceLocator _BroadcastListener;

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
			Console.WriteLine("B to search for basic devices");
			Console.WriteLine("U to search for published device by UUID");
			Console.WriteLine("L to listen for notifications");
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
			if (_BroadcastListener != null)
			{
				Console.WriteLine("Closing previous listener...");
				_BroadcastListener.DeviceAvailable -= _BroadcastListener_DeviceAvailable;
				_BroadcastListener.DeviceUnavailable -= _BroadcastListener_DeviceUnavailable;
				
				_BroadcastListener.StopListeningForNotifications();
				_BroadcastListener.Dispose();
			}

			Console.WriteLine("Starting broadcast listener");
			_BroadcastListener = new SsdpDeviceLocator();
			_BroadcastListener.DeviceAvailable += _BroadcastListener_DeviceAvailable;
			_BroadcastListener.DeviceUnavailable += _BroadcastListener_DeviceUnavailable;
			_BroadcastListener.StartListeningForNotifications();
			Console.WriteLine("Now listening for broadcasts");
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
			if (_DevicePublisher != null)
			{
				Console.WriteLine("Stopping previous publisher.");
				_DevicePublisher.Dispose();
			}

			// Create a device publisher
			_DevicePublisher = new SsdpDevicePublisher();
			
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

			// Now publish by adding them to the publisher.
			_DevicePublisher.AddDevice(rootDevice);

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

		private static async Task SearchForDevicesByUuid()
		{
			if (_DevicePublisher == null || !_DevicePublisher.Devices.Any())
			{
				Console.WriteLine("No devices being published. Use the (P)ublish command first.");
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
			Console.WriteLine(device.Usn + " - " + device.NotificationType +  "\r\n\t @ " + device.DescriptionLocation);
		}

	}
}