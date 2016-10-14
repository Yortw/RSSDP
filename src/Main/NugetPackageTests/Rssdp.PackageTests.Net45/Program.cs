using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rssdp.PackageTests.Net45
{
	class Program
	{
		static void Main(string[] args)
		{
			var publisher = new Rssdp.SsdpDevicePublisher();
			var locator = new Rssdp.SsdpDeviceLocator();
		}
	}
}
