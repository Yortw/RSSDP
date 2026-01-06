using Rssdp;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace RssdpUwpPackageTests
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a <see cref="Frame">.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private Rssdp.SsdpDeviceLocator? _DeviceLocator;
		private readonly ObservableCollection<Rssdp.DiscoveredSsdpDevice> _Devices = [];
		private bool _listeningStarted;

		public ObservableCollection<Rssdp.DiscoveredSsdpDevice> Devices => _Devices;

		public MainPage()
		{
			InitializeComponent();
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);


			try
			{
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
					await new Windows.UI.Popups.MessageDialog("No UPnP devices were found on the network.", "No Devices Found").ShowAsync();

				var cvs = new Windows.UI.Xaml.Data.CollectionViewSource { Source = Devices };
				DevicesList.ItemsSource = cvs.View;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Discovery error: {ex}");
				await new Windows.UI.Popups.MessageDialog(ex.ToString(), "Startup Error").ShowAsync();
			}
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);

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

		private void DeviceLocator_DeviceAvailable(object? sender, Rssdp.DeviceAvailableEventArgs e)
		{
			DiscoveredSsdpDevice? existingDevice = null;
			lock (_Devices)
			{
				existingDevice = (from d in _Devices where d.Usn == e.DiscoveredDevice.Usn select d).FirstOrDefault();
			}

			if (existingDevice == null)
			{
				_ = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
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
				_ = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
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
