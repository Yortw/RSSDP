using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RssdpPackageTestConsoleNet45
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
			var searcher = new RssdpPackageTests.DeviceSearcher();
			var results = await searcher.SearchAsync();
			foreach (var device in results)
			{
				Console.WriteLine(device.Usn + " " + device.DescriptionLocation);
			}
			Console.WriteLine("Done. Press any key to exit.");
		}
	}
}
