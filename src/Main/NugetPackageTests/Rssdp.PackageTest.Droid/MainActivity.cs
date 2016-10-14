using Android.App;
using Android.Widget;
using Android.OS;

namespace Rssdp.PackageTest.Droid
{
	[Activity(Label = "Rssdp.PackageTest.Droid", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			var publisher = new Rssdp.SsdpDevicePublisher();
			var locator = new Rssdp.SsdpDeviceLocator();

			// Set our view from the "main" layout resource
			// SetContentView (Resource.Layout.Main);
		}
	}
}

