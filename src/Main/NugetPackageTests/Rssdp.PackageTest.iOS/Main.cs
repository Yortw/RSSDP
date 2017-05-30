using UIKit;

namespace Rssdp.PackageTest.iOS
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main(string[] args)
		{
			var publisher = new Rssdp.SsdpDevicePublisher();
			var locator = new Rssdp.SsdpDeviceLocator();

			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main(args, null, "AppDelegate");
		}
	}
}