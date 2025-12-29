using Rssdp;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;

namespace RssdpPackageTestMauiApp
{
	public partial class MainPage : ContentPage
	{
		private Rssdp.SsdpDeviceLocator? _DeviceLocator;
		private readonly ObservableCollection<Rssdp.DiscoveredSsdpDevice> _Devices = [];
		private bool _listeningStarted;

		public MainPage()
		{
			InitializeComponent();
			// Locator created after permissions are granted
		}

		public ObservableCollection<Rssdp.DiscoveredSsdpDevice> Devices => _Devices;

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			try
			{
				// Ensure required runtime permission on Android 13+
				if (!await EnsureWifiPermissionAsync())
				{
					await this.DisplayAlert("Permission Required", "Nearby Wi‑Fi Devices permission is required for UPnP discovery.", "OK");
					return;
				}

				// Create locator lazily after permissions
				if (_DeviceLocator == null)
				{
					_DeviceLocator = new Rssdp.SsdpDeviceLocator();
					_DeviceLocator.DeviceAvailable += DeviceLocator_DeviceAvailable;
					_DeviceLocator.DeviceUnavailable += DeviceLocator_DeviceUnavailable;
				}

				// Start discovery only after permissions are granted (once)
				if (!_listeningStarted)
				{
					_DeviceLocator!.StartListeningForNotifications();
					_listeningStarted = true;
				}

				var devices = await _DeviceLocator!.SearchAsync();
				if (!devices.Any())
					await this.DisplayAlert("No Devices Found", "No UPnP devices were found on the network.", "OK");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Discovery error: {ex}");
				await this.DisplayAlert("Startup Error", ex.ToString(), "OK");
			}
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			try
			{
				if (_listeningStarted && _DeviceLocator != null)
				{
					_DeviceLocator.StopListeningForNotifications();
					_listeningStarted = false;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Stop listening error: {ex}");
			}
		}

		private static async Task<bool> EnsureWifiPermissionAsync()
		{
			#if ANDROID
			try
			{
				// Only Android 13+ uses runtime permission for Nearby Wi‑Fi Devices
				if (DeviceInfo.Version.Major >= 13)
				{
					var status = await Permissions.CheckStatusAsync<Permissions.NearbyWifiDevices>();
					if (status != PermissionStatus.Granted)
					{
						status = await Permissions.RequestAsync<Permissions.NearbyWifiDevices>();
					}
					return status == PermissionStatus.Granted;
				}

				// Pre-Android 13: no runtime permission required for SSDP multicast
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Permission check error: {ex}");
				return false;
			}
			#else
			await Task.Delay(1);
			return true;
			#endif
		}

		private void DeviceLocator_DeviceAvailable(object? sender, Rssdp.DeviceAvailableEventArgs e)
		{
			DiscoveredSsdpDevice? existingDevice = null;
			lock (_Devices)
			{
				existingDevice = (from d in _Devices where d.Usn == e.DiscoveredDevice.Usn select d).FirstOrDefault();
			}

			if (existingDevice == null)
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
					lock (_Devices)
					{
						_Devices.Add(e.DiscoveredDevice);
					}
				});
			}
		}

		private void DeviceLocator_DeviceUnavailable(object? sender, Rssdp.DeviceUnavailableEventArgs e)
		{
			DiscoveredSsdpDevice? existingDevice = null;
			lock (_Devices)
			{
				existingDevice = (from d in _Devices where d.Usn == e.DiscoveredDevice.Usn select d).FirstOrDefault();
			}

			if (existingDevice != null)
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
					lock (_Devices)
					{
						_Devices.Remove(e.DiscoveredDevice);
					}
				});
			}
		}

	}
}
