using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp;

namespace Test.RssdpPortable.DevicePublisher
{
	[TestClass]
	public sealed class SsdpDevicePublisherFactoryTest
	{
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Create_WhenArgumentIsNull_ThrowInvalidOperationException()
		{
			var devicePublisherFactory = new SsdpDevicePublisherFactory();
			devicePublisherFactory.Create(null, 0);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Create_WhenArgumentIsEmpty_ThrowInvalidOperationException()
		{
			var devicePublisherFactory = new SsdpDevicePublisherFactory();
			devicePublisherFactory.Create(string.Empty, 0);
		}

		[TestMethod]
		public void Create_WhenArgumentIsNotNullAndNotEmpty_DevicePublisherHasBeenCreated()
		{
			var devicePublisherFactory = new SsdpDevicePublisherFactory();
			var publisher = devicePublisherFactory.Create("127.0.0.1", 0);
			Assert.IsNotNull(publisher);
		}
	}
}
