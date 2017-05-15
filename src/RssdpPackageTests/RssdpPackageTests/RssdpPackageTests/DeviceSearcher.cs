using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RssdpPackageTests
{
	public class DeviceSearcher
	{
		public async Task<IEnumerable<Rssdp.DiscoveredSsdpDevice>> SearchAsync()
		{
			var locator = new Rssdp.SsdpDeviceLocator();
			return await locator.SearchAsync();
		}
	}
}