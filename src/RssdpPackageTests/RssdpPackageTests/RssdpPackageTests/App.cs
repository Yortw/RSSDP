using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace RssdpPackageTests
{
	public class App : Application
	{
		private ListView listView;

		public App()
		{
			//var publisher = new Rssdp.SsdpDevicePublisher();
			//var device = new Rssdp.SsdpRootDevice()
			//{
			//	DeviceType = "test",
			//	DeviceTypeNamespace = "test",
			//	FriendlyName = "test",
			//	ModelName= "Test",
			//	Location = new Uri("http://192.168.1.58/test"),
			//	Manufacturer = "test",
			//	SerialNumber = "123",
			//	Uuid = System.Guid.NewGuid().ToString()
			//};
			//publisher.AddDevice(device);

			listView = new ListView();

			// The root page of your application
			var content = new ContentPage
			{
				Title = "RssdpPackageTests",
				Content = listView
			};

			MainPage = new NavigationPage(content);

		}

		private async Task SearchAsync(ListView listView)
		{
			var items = new List<string>();
			var searcher = new RssdpPackageTests.DeviceSearcher();
			foreach (var device in await searcher.SearchAsync())
			{
				items.Add(device.DescriptionLocation.ToString());
			}
			listView.ItemsSource = items;
		}

		protected override void OnStart()
		{
			// Handle when your app starts
			var t = SearchAsync(listView);
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}
