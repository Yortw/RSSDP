using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rssdp.Infrastructure;

namespace Test.Rssdp
{
	public class MockDeviceLocator : SsdpDeviceLocatorBase
	{
		public MockDeviceLocator() : base(new MockCommsServer())
		{
		}

		public MockDeviceLocator(ISsdpCommunicationsServer commsServer)
			: base(commsServer)
		{
		}
	}
}
