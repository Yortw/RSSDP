using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp;

namespace Test.RssdpPortable
{
	[TestClass]
	public class SsdpDeviceTests
	{

		#region AddDevice Tests

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void SsdpDevice_AddDevice_ThrowsArgumentNullOnNullDevice()
		{
			var rootDevice = new SsdpRootDevice();
			rootDevice.AddDevice(null);
		}

		[TestMethod]
		public void SsdpDevice_AddDevice_RaisesDeviceAdded()
		{
			var rootDevice = new SsdpRootDevice();
			
			bool eventRaised = false;
			SsdpDevice eventDevice = null;
			rootDevice.DeviceAdded += (sender, e) =>
				{
					eventRaised = true;
					eventDevice = e.Device;
				};

			var embeddedDevice = new SsdpDevice(rootDevice);
			rootDevice.AddDevice(embeddedDevice);
			Assert.IsTrue(eventRaised);
			Assert.AreEqual(embeddedDevice, eventDevice);
		}

		[TestMethod]
		public void SsdpDevice_AddDevice_DuplicateAddDoesNothing()
		{
			var rootDevice = new SsdpRootDevice();

			var embeddedDevice = new SsdpDevice(rootDevice);
			rootDevice.AddDevice(embeddedDevice);
			rootDevice.AddDevice(embeddedDevice);
			Assert.AreEqual(1, rootDevice.Devices.Count());
		}

		#endregion

		#region RemoveDevice Tests

		[TestMethod]
		public void SsdpDevice_RemoveDevice_RaisesDeviceRemoved()
		{
			var rootDevice = new SsdpRootDevice();

			bool eventRaised = false;
			SsdpDevice eventDevice = null;
			rootDevice.DeviceRemoved += (sender, e) =>
			{
				eventRaised = true;
				eventDevice = e.Device;
			};

			var embeddedDevice = new SsdpDevice(rootDevice) { Uuid = System.Guid.NewGuid().ToString() };
			rootDevice.AddDevice(embeddedDevice);
			rootDevice.RemoveDevice(embeddedDevice);

			Assert.IsTrue(eventRaised);
			Assert.AreEqual(embeddedDevice, eventDevice);
		}

		[TestMethod]
		public void SsdpDevice_RemoveDevice_DuplicateRemoveDoesNothing()
		{
			var rootDevice = new SsdpRootDevice();

			var embeddedDevice = new SsdpDevice(rootDevice) { Uuid = System.Guid.NewGuid().ToString() };
			rootDevice.AddDevice(embeddedDevice);
			Assert.AreEqual(1, rootDevice.Devices.Count());
			rootDevice.RemoveDevice(embeddedDevice);
			rootDevice.RemoveDevice(embeddedDevice);
			Assert.AreEqual(0, rootDevice.Devices.Count());
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void SsdpDevice_RemoveDevice_ThrowsArgumentNullOnNullDevice()
		{
			var rootDevice = new SsdpRootDevice();
			rootDevice.RemoveDevice(null);
		}

		#endregion

		#region Constructor Tests

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void SsdpDevice_ConstructorThrowsArgumentNullIfNotRootDevice()
		{
			var device = new SsdpDevice(null);
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void DeviceEventArgs_ConstructorThrowsOnNullDevice()
		{
			var args = new DeviceEventArgs(null);
		}

		#endregion

		#region DeviceTypeNamespace Tests

		[TestMethod]
		public void SsdpDevice_NullDeviceTypeNamespaceReturnsNull()
		{
			var rootDevice = new SsdpRootDevice();
			rootDevice.DeviceTypeNamespace = null;
			Assert.AreEqual(null, rootDevice.DeviceTypeNamespace);
		}

		[TestMethod]
		public void SsdpDevice_EmptyDeviceTypeNamespaceReturnsEmpty()
		{
			var rootDevice = new SsdpRootDevice();
			rootDevice.DeviceTypeNamespace = String.Empty;
			Assert.AreEqual(String.Empty, rootDevice.DeviceTypeNamespace);
		}

		#endregion

		#region DeviceType Tests

		[TestMethod]
		public void SsdpDevice_NullDeviceTypeReturnsNull()
		{
			var rootDevice = new SsdpRootDevice();
			rootDevice.DeviceType = null;
			Assert.AreEqual(null, rootDevice.DeviceType);
		}

		[TestMethod]
		public void SsdpDevice_EmptyDeviceTypeReturnsEmpty()
		{
			var rootDevice = new SsdpRootDevice();
			rootDevice.DeviceType = String.Empty;
			Assert.AreEqual(String.Empty, rootDevice.DeviceType);
		}

		[TestMethod]
		public void SsdpDevice_FullDeviceTypesReturnsStringWithNullValues()
		{
			var rootDevice = new SsdpRootDevice();
			rootDevice.DeviceType = null;
			rootDevice.DeviceTypeNamespace = null;
			Assert.AreEqual("urn::device::1", rootDevice.FullDeviceType);
		}

		#endregion

		#region RootDevice Tests

		[TestMethod]
		public void SsdpDevice_RootDeviceReturnsSelfFromRootDeviceProperty()
		{
			var rootDevice = new SsdpRootDevice();
			Assert.AreEqual(rootDevice, rootDevice.RootDevice);
		}

		[TestMethod]
		public void SsdpDevice_DeviceReturnsAssignedRootDevice()
		{
			var rootDevice = new SsdpRootDevice();
			var device = new SsdpDevice(rootDevice);

			Assert.AreEqual(rootDevice, device.RootDevice);
		}

		#endregion

	}
}