using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RssdpPackageTestConsoleNet40
{
	class Program
	{
		static void Main(string[] args)
		{
			SearchAsync();
			Console.ReadLine();
		}

		private static async void SearchAsync()
		{
			Console.WriteLine("Searching");
			var locator = new Rssdp.SsdpDeviceLocator(new Rssdp.Infrastructure.SsdpCommunicationsServer(new Rssdp.SocketFactory("192.168.1.57")));
			var results = await locator.SearchAsync();
			foreach (var device in results)
			{
				Console.WriteLine(device.Usn + " " + device.DescriptionLocation);
			}
			Console.WriteLine("Done. Press any key to exit.");
		}
	}
}