using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rssdp.PackageTest.NetCore
{
	public class Program
	{
		/// <summary>
		/// Defines the entry point of the application.
		/// </summary>
		/// <param name="args">The arguments.</param>
		public static void Main(string[] args)
		{
			var publisher = new Rssdp.SsdpDevicePublisher();
			var locator = new Rssdp.SsdpDeviceLocator();
		}
	}
}
