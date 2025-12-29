namespace RssdpPackageTestConsoleApp
{
	class Program
	{
		static async Task Main()
		{
			//Confirm we can create a publisher.
			var publisher = new Rssdp.SsdpDevicePublisher();
			publisher?.Dispose();

			// Confirm we can create a locator and perform a search.
			await SearchAsync();

			Console.WriteLine();
			Console.WriteLine("Done. Press any key to exit.");
			Console.ReadKey();
		}

		private static async Task SearchAsync()
		{
			Console.WriteLine("Searching");
			var searcher = new Rssdp.SsdpDeviceLocator();
			var results = await searcher.SearchAsync();
			foreach (var device in results)
			{
				Console.WriteLine(device.Usn + " " + device.DescriptionLocation);
			}
		}
	}
}