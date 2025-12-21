using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp;

namespace Test.Rssdp
{
	[TestClass, System.Runtime.InteropServices.GuidAttribute("C921D87F-210B-4C62-BF87-84E6D89DEADB")]
	public class SocketFactoryTests
	{
		[TestMethod]
		public void Ctor_WhenIpAddressIsV6_DeviceNetworkTypeEqualsV61()
		{
			var socketFactory = new SocketFactory(IPAddress.IPv6Any.ToString());

			Assert.AreEqual(DeviceNetworkType.IPv6, socketFactory.DeviceNetworkType);
		}

		[TestMethod]
		public void Ctor_WhenIpAddressIsV6_DeviceNetworkTypeEqualsV62()
		{
			var socketFactory = new SocketFactory("::1");

			Assert.AreEqual(DeviceNetworkType.IPv6, socketFactory.DeviceNetworkType);
		}

		[TestMethod]
		public void Ctor_WhenIpAddressIsV4_DeviceNetworkTypeEqualsV41()
		{
			var socketFactory = new SocketFactory(IPAddress.Any.ToString());

			Assert.AreEqual(DeviceNetworkType.IPv4, socketFactory.DeviceNetworkType);
		}

		[TestMethod]
		public void Ctor_WhenIpAddressIsV4_DeviceNetworkTypeEqualsV42()
		{
			var socketFactory = new SocketFactory("127.0.0.1");

			Assert.AreEqual(DeviceNetworkType.IPv4, socketFactory.DeviceNetworkType);
		}
	}
}
