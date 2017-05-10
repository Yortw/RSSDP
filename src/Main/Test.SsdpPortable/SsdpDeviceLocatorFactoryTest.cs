using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp;
using Rssdp.Aggregatable;

namespace Test.RssdpPortable.DeviceLocator
{
	[TestClass]
	public sealed class SsdpDeviceLocatorFactoryTest
	{
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Create_WhenArgumentIsNull_ThrowInvalidOperationException()
		{
			var deviceLocatorFactory = new SsdpDeviceLocatorFactory();
			deviceLocatorFactory.Create(null, 0);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Create_WhenArgumentIsEmpty_ThrowInvalidOperationException()
		{
			var deviceLocatorFactory = new SsdpDeviceLocatorFactory();
			deviceLocatorFactory.Create(string.Empty, 0);
		}

		[TestMethod]
		public void Create_WhenArgumentIsNotNullAndNotEmpty_DevicePublisherHasBeenCreated()
		{
			var deviceLocatorFactory = new SsdpDeviceLocatorFactory();
			var publisher = deviceLocatorFactory.Create("127.0.0.1", 0);
			Assert.IsNotNull(publisher);
		}
	}
}
