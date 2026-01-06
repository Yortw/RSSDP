using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp;
using Rssdp.Infrastructure;

namespace TestRssdp
{
	[TestClass]
	public class DevicePublisherTests
	{

		#region Argument Checking

		#region Constructors

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void Publisher_Constructor_ThrowsOnNullCommsServer()
		{
			_ = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			_ = new TestDevicePublisher(null);
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void Publisher_Constructor_ThrowsOnNullOS()
		{
			_ = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var server = new MockCommsServer();
			_ = new TestDevicePublisher(server, null, "1.1");
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void Publisher_Constructor_ThrowsOnNullOSVersion()
		{
			_ = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var server = new MockCommsServer();
			_ = new TestDevicePublisher(server, "TestOS", null);
		}

		[ExpectedException(typeof(System.ArgumentException))]
		[TestMethod]
		public void Publisher_Constructor_ThrowsOnEmptyOS()
		{
			_ = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var server = new MockCommsServer();
			_ = new TestDevicePublisher(server, String.Empty, "1.1");
		}

		[ExpectedException(typeof(System.ArgumentException))]
		[TestMethod]
		public void Publisher_Constructor_ThrowsOnEmptyOSVersion()
		{
			_ = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var server = new MockCommsServer();
			_ = new TestDevicePublisher(server, "TestOS", String.Empty);
		}

		#endregion

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void Publisher_AddDevice_ThrowsOnNullDevice()
		{
			var server = new MockCommsServer();
			var publisher = new TestDevicePublisher(server);
			publisher.AddDevice(null);
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void Publisher_RemoveDevice_ThrowsOnNullDevice()
		{
			var server = new MockCommsServer();
			var publisher = new TestDevicePublisher(server);
			publisher.RemoveDevice(null);
		}

		#endregion

		#region Dispose Tests

		[TestMethod]
		public void Publisher_DisposesNonSharedCommsServer()
		{
			var server = new MockCommsServer();
			var publisher = new TestDevicePublisher(server);
			publisher.AddDevice(CreateValidRootDevice());

			publisher.Dispose();
			Assert.IsTrue(server.IsDisposed);
		}

		[TestMethod]
		public void Publisher_DoesNotDisposeSharedCommsServer()
		{
			var server = new MockCommsServer
			{
				IsShared = true
			};
			var publisher = new TestDevicePublisher(server);
			publisher.Dispose();
			Assert.IsFalse(server.IsDisposed);
		}

		[TestMethod]
		public void Publisher_DisposeSetsIsDisposed()
		{
			var server = new MockCommsServer();
			var publisher = new TestDevicePublisher(server);
			publisher.Dispose();
			Assert.IsTrue(publisher.IsDisposed);
		}

		[ExpectedException(typeof(System.ObjectDisposedException))]
		[TestMethod]
		public void Publisher_AddDeviceThrowsWhenDisposed()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var server = new MockCommsServer();
			var publisher = new TestDevicePublisher(server);
			publisher.Dispose();
			publisher.AddDevice(rootDevice);
		}

		[ExpectedException(typeof(System.ObjectDisposedException))]
		[TestMethod]
		public void Publisher_RemoveDeviceThrowsWhenDisposed()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var server = new MockCommsServer();
			var publisher = new TestDevicePublisher(server);
			publisher.Dispose();
			publisher.RemoveDevice(rootDevice);
		}

		#endregion

		#region AddDevice Tests

		[TestMethod]
		public void Publisher_AddDevice_BroadcastsRootUpnpAliveNotification()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);
				Assert.IsTrue(server.SentBroadcasts.Any());

				var sentMessages = GetAllSentBroadcasts(server);
				var aliveNotifications = GetNotificationsByType(sentMessages, "ssdp:alive");

				var upnpRootDeviceNotifications = GetNotificationsForSearchTarget(aliveNotifications, SsdpConstants.UpnpDeviceTypeRootDevice);
				var pnpRootDeviceNotifications = GetNotificationsForSearchTarget(aliveNotifications, SsdpConstants.PnpDeviceTypeRootDevice);

				Assert.AreEqual(1, publisher.Devices.Count());
				Assert.IsTrue(upnpRootDeviceNotifications.Count() >= 1);
				Assert.AreEqual(0, pnpRootDeviceNotifications.Count());
			}
		}

		[TestMethod]
		public void Publisher_AddDevice_BroadcastsRootPnpAliveNotification()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = true;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);

				Assert.IsTrue(server.SentBroadcasts.Any());

				var sentMessages = GetAllSentBroadcasts(server);
				var aliveNotifications = GetNotificationsByType(sentMessages, "ssdp:alive");

				var upnpRootDeviceNotifications = GetNotificationsForSearchTarget(aliveNotifications, SsdpConstants.UpnpDeviceTypeRootDevice);
				var pnpRootDeviceNotifications = GetNotificationsForSearchTarget(aliveNotifications, SsdpConstants.UpnpDeviceTypeRootDevice);

				Assert.IsTrue(upnpRootDeviceNotifications.Count() >= 1);
				Assert.IsTrue(pnpRootDeviceNotifications.Count() >= 1);

			}
		}

		[TestMethod]
		public void Publisher_AddDevice_BroadcastsUdnNotification()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = true;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(10000);

				//Initial signal is just for first broadcast,
				//wait for others to be sent.
				//System.Threading.Thread.Sleep(100);
				Assert.IsTrue(server.SentBroadcasts.Any());

				var sentMessages = GetAllSentBroadcasts(server);
				var aliveNotifications = GetNotificationsByType(sentMessages, "ssdp:alive");

				var udnDeviceNotifications = GetNotificationsForSearchTarget(aliveNotifications, rootDevice.Udn);

				Assert.IsTrue(udnDeviceNotifications.Count() >= 1);
			}
		}

		[TestMethod]
		public void Publisher_AddDevice_BroadcastsDeviceTypeAliveNotification()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = true;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);

				Assert.IsTrue(server.SentBroadcasts.Any());

				var sentMessages = GetAllSentBroadcasts(server);
				var aliveNotifications = GetNotificationsByType(sentMessages, "ssdp:alive");

				var deviceTypeNotifications = GetNotificationsForSearchTarget(aliveNotifications, rootDevice.FullDeviceType);

				Assert.IsTrue(deviceTypeNotifications.Count() >= 1);
			}
		}

		[TestMethod]
		public void Publisher_AddDevice_AddTreeSendsChildNotifications()
		{
			var rootDevice = CreateValidRootDevice();
			var embeddedDevice = CreateValidEmbeddedDevice(rootDevice);
			rootDevice.AddDevice(embeddedDevice);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = true;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);
				Assert.IsTrue(server.SentBroadcasts.Any());

				var sentMessages = GetAllSentBroadcasts(server);
				var aliveNotifications = GetNotificationsByType(sentMessages, "ssdp:alive");

				var deviceTypeNotifications = GetNotificationsForSearchTarget(aliveNotifications, embeddedDevice.FullDeviceType);
				var udnNotifications = GetNotificationsForSearchTarget(aliveNotifications, embeddedDevice.Udn);
				var rootDeviceNotificationsForEmbeddedDevice = GetNotificationsForSearchTarget(aliveNotifications, SsdpConstants.UpnpDeviceTypeRootDevice).Where((n) =>
					{
						return n.Headers.GetValues("USN").First() == embeddedDevice.Udn + "::" + embeddedDevice.FullDeviceType;
					});

				Assert.AreEqual(1, rootDevice.Devices.Count());
				Assert.IsTrue(deviceTypeNotifications.Count() >= 1);
				Assert.IsTrue(udnNotifications.Count() >= 1);
				Assert.AreEqual(0, rootDeviceNotificationsForEmbeddedDevice.Count());
			}
		}

		[TestMethod]
		public void Publisher_AddDevice_AddTreeSendsGrandchildNotifications()
		{
			var rootDevice = CreateValidRootDevice();
			var parentDevice = CreateValidEmbeddedDevice(rootDevice);
			rootDevice.AddDevice(parentDevice);
			var embeddedDevice = CreateValidEmbeddedDevice(rootDevice);
			parentDevice.AddDevice(embeddedDevice);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = true;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(10000);

				System.Threading.Thread.Sleep(100);
				Assert.IsTrue(server.SentBroadcasts.Any());

				var sentMessages = GetAllSentBroadcasts(server);
				var aliveNotifications = GetNotificationsByType(sentMessages, "ssdp:alive");

				var deviceTypeNotifications = GetNotificationsForSearchTarget(aliveNotifications, embeddedDevice.FullDeviceType);
				var udnNotifications = GetNotificationsForSearchTarget(aliveNotifications, embeddedDevice.Udn);
				var rootDeviceNotificationsForEmbeddedDevice = GetNotificationsForSearchTarget(aliveNotifications, SsdpConstants.UpnpDeviceTypeRootDevice).Where((n) =>
				{
					return n.Headers.GetValues("USN").First() == embeddedDevice.Udn + "::" + embeddedDevice.FullDeviceType;
				});

				Assert.AreEqual(1, parentDevice.Devices.Count());
				Assert.IsTrue(deviceTypeNotifications.Count() >= 1);
				Assert.IsTrue(udnNotifications.Count() >= 1);
				Assert.AreEqual(0, rootDeviceNotificationsForEmbeddedDevice.Count());
			}
		}

		[TestMethod]
		public void Publisher_AddDevice_AddChildSendsNotifications()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = true;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);

				server.WaitForMockBroadcast(10000);
				//System.Threading.Thread.Sleep(100);
				Assert.IsTrue(server.SentBroadcasts.Any());

				server.SentBroadcasts.Clear();

				var embeddedDevice = CreateValidEmbeddedDevice(rootDevice);
				rootDevice.AddDevice(embeddedDevice);

				server.WaitForMockBroadcast(10000);
				//System.Threading.Thread.Sleep(100);
				Assert.IsTrue(server.SentBroadcasts.Any());

				var sentMessages = GetAllSentBroadcasts(server);
				var aliveNotifications = GetNotificationsByType(sentMessages, "ssdp:alive");

				var deviceTypeNotifications = GetNotificationsForSearchTarget(aliveNotifications, embeddedDevice.FullDeviceType);
				var udnNotifications = GetNotificationsForSearchTarget(aliveNotifications, embeddedDevice.Udn);
				var rootDeviceNotificationsForEmbeddedDevice = GetNotificationsForSearchTarget(aliveNotifications, SsdpConstants.UpnpDeviceTypeRootDevice).Where((n) =>
				{
					return n.Headers.GetValues("USN").First() == embeddedDevice.Udn + "::" + embeddedDevice.FullDeviceType;
				});

				Assert.IsTrue(deviceTypeNotifications.Count() >= 1);
				Assert.IsTrue(udnNotifications.Count() >= 1);
				Assert.AreEqual(0, rootDeviceNotificationsForEmbeddedDevice.Count());
			}
		}

		[TestMethod]
		public void Publisher_AddDevice_AddGrandchildSendsNotifications()
		{
			var rootDevice = CreateValidRootDevice();
			var parentDevice = CreateValidEmbeddedDevice(rootDevice);
			rootDevice.AddDevice(parentDevice);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = true;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);

				server.WaitForMockBroadcast(10000);
				//System.Threading.Thread.Sleep(100);
				Assert.IsTrue(server.SentBroadcasts.Any());

				server.SentBroadcasts.Clear();

				var embeddedDevice = CreateValidEmbeddedDevice(rootDevice);
				parentDevice.AddDevice(embeddedDevice);

				server.WaitForMockBroadcast(10000);
				//System.Threading.Thread.Sleep(100);
				Assert.IsTrue(server.SentBroadcasts.Any());

				var sentMessages = GetAllSentBroadcasts(server);
				var aliveNotifications = GetNotificationsByType(sentMessages, "ssdp:alive");

				var deviceTypeNotifications = GetNotificationsForSearchTarget(aliveNotifications, embeddedDevice.FullDeviceType);
				var udnNotifications = GetNotificationsForSearchTarget(aliveNotifications, embeddedDevice.Udn);
				var rootDeviceNotificationsForEmbeddedDevice = GetNotificationsForSearchTarget(aliveNotifications, SsdpConstants.UpnpDeviceTypeRootDevice).Where((n) =>
				{
					return n.Headers.GetValues("USN").First() == embeddedDevice.Udn + "::" + embeddedDevice.FullDeviceType;
				});

				Assert.IsTrue(deviceTypeNotifications.Count() >= 1);
				Assert.IsTrue(udnNotifications.Count() >= 1);
				Assert.AreEqual(0, rootDeviceNotificationsForEmbeddedDevice.Count());
			}
		}

		[TestMethod]
		public void Publisher_AddDevice_DuplicateAddDoesNothing()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);
				publisher.AddDevice(rootDevice);

				Assert.AreEqual(1, publisher.Devices.Count());
			}
		}

		#endregion

		#region RemoveDevice Tests

		[TestMethod]
		public void Publisher_RemoveDevice_SendsRootUpnpByeByeNotification()
		{
			var rootDevice = CreateValidRootDevice();
			rootDevice.CacheLifetime = TimeSpan.FromMinutes(1);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);

				Assert.IsTrue(server.SentBroadcasts.Any());
				server.SentBroadcasts.Clear();
				publisher.RemoveDevice(rootDevice);

				var sentMessages = GetAllSentBroadcasts(server);
				var byebyeNotifications = GetNotificationsByType(sentMessages, "ssdp:byebye");

				var upnpRootDeviceNotifications = GetNotificationsForSearchTarget(byebyeNotifications, SsdpConstants.UpnpDeviceTypeRootDevice);
				var pnpRootDeviceNotifications = GetNotificationsForSearchTarget(byebyeNotifications, SsdpConstants.PnpDeviceTypeRootDevice);

				Assert.AreEqual(0, publisher.Devices.Count());
				Assert.IsTrue(upnpRootDeviceNotifications.Count() >= 1);
				Assert.AreEqual(0, pnpRootDeviceNotifications.Count());
			}
		}

		[TestMethod]
		public void Publisher_RemoveDevice_SendsRootPnpByeByeNotification()
		{
			var rootDevice = CreateValidRootDevice();
			rootDevice.CacheLifetime = TimeSpan.FromMinutes(1);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);

				Assert.IsTrue(server.SentBroadcasts.Any());
				server.SentBroadcasts.Clear();
				publisher.RemoveDevice(rootDevice);

				var sentMessages = GetAllSentBroadcasts(server);
				var byebyeNotifications = GetNotificationsByType(sentMessages, "ssdp:byebye");

				var upnpRootDeviceNotifications = GetNotificationsForSearchTarget(byebyeNotifications, SsdpConstants.UpnpDeviceTypeRootDevice);
				var pnpRootDeviceNotifications = GetNotificationsForSearchTarget(byebyeNotifications, SsdpConstants.PnpDeviceTypeRootDevice);

				Assert.AreEqual(0, publisher.Devices.Count());
				Assert.IsTrue(upnpRootDeviceNotifications.Count() >= 1);
				Assert.IsTrue(pnpRootDeviceNotifications.Count() >= 1);
			}
		}

		[TestMethod]
		public void Publisher_RemoveDevice_BroadcastsByeByeUdnNotification()
		{
			var rootDevice = CreateValidRootDevice();
			rootDevice.CacheLifetime = TimeSpan.FromMinutes(1);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);

				Assert.IsTrue(server.SentBroadcasts.Any());
				server.SentBroadcasts.Clear();
				publisher.RemoveDevice(rootDevice);

				var sentMessages = GetAllSentBroadcasts(server);
				var byebyeNotifications = GetNotificationsByType(sentMessages, "ssdp:byebye");

				var udnRootDeviceNotifications = GetNotificationsForSearchTarget(byebyeNotifications, rootDevice.Udn);

				Assert.IsTrue(udnRootDeviceNotifications.Count() >= 1);
			}
		}

		[TestMethod]
		public void Publisher_RemoveDevice_BroadcastsByeByeDeviceTypeNotification()
		{
			var rootDevice = CreateValidRootDevice();
			rootDevice.CacheLifetime = TimeSpan.FromMinutes(1);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);

				Assert.IsTrue(server.SentBroadcasts.Any());
				server.SentBroadcasts.Clear();
				publisher.RemoveDevice(rootDevice);

				var sentMessages = GetAllSentBroadcasts(server);
				var byebyeNotifications = GetNotificationsByType(sentMessages, "ssdp:byebye");

				var deviceTypeRootDeviceNotifications = GetNotificationsForSearchTarget(byebyeNotifications, String.Format("urn:{0}", rootDevice.FullDeviceType));

				Assert.IsTrue(deviceTypeRootDeviceNotifications.Count() >= 1);
			}
		}

		[TestMethod]
		public void Publisher_RemoveDevice_RemoveTreeSendsChildNotifications()
		{
			var rootDevice = CreateValidRootDevice();
			var embeddedDevice = CreateValidEmbeddedDevice(rootDevice);
			rootDevice.AddDevice(embeddedDevice);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);
				Assert.IsTrue(server.SentBroadcasts.Any());
				server.SentBroadcasts.Clear();

				publisher.RemoveDevice(rootDevice);

				var sentMessages = GetAllSentBroadcasts(server);
				var byebyeNotifications = GetNotificationsByType(sentMessages, "ssdp:byebye");

				var udnRootDeviceNotifications = GetNotificationsForSearchTarget(byebyeNotifications, embeddedDevice.Udn);
				var deviceTypeRootDeviceNotifications = GetNotificationsForSearchTarget(byebyeNotifications, String.Format("urn:{0}", embeddedDevice.FullDeviceType));

				Assert.IsTrue(udnRootDeviceNotifications.Count() >= 1);
				Assert.IsTrue(deviceTypeRootDeviceNotifications.Count() >= 1);
			}
		}

		[TestMethod]
		public void Publisher_RemoveDevice_RemoveTreeSendsGrandchildNotifications()
		{
			var rootDevice = CreateValidRootDevice();
			var parentDevice = CreateValidEmbeddedDevice(rootDevice);
			rootDevice.AddDevice(parentDevice);
			var embeddedDevice = CreateValidEmbeddedDevice(rootDevice);
			parentDevice.AddDevice(embeddedDevice);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);
				Assert.IsTrue(server.SentBroadcasts.Any());
				server.SentBroadcasts.Clear();

				publisher.RemoveDevice(rootDevice);

				var sentMessages = GetAllSentBroadcasts(server);
				var byebyeNotifications = GetNotificationsByType(sentMessages, "ssdp:byebye");

				var udnRootDeviceNotifications = GetNotificationsForSearchTarget(byebyeNotifications, embeddedDevice.Udn);
				var deviceTypeRootDeviceNotifications = GetNotificationsForSearchTarget(byebyeNotifications, String.Format("urn:{0}", embeddedDevice.FullDeviceType));

				Assert.IsTrue(udnRootDeviceNotifications.Count() >= 1);
				Assert.IsTrue(deviceTypeRootDeviceNotifications.Count() >= 1);
			}
		}

		[TestMethod]
		public void Publisher_RemoveDevice_RemoveChildSendsNotifications()
		{
			var rootDevice = CreateValidRootDevice();
			var embeddedDevice = CreateValidEmbeddedDevice(rootDevice);
			rootDevice.AddDevice(embeddedDevice);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);

				server.WaitForMockBroadcast(10000);
				//System.Threading.Thread.Sleep(100);
				Assert.IsTrue(server.SentBroadcasts.Any());

				server.SentBroadcasts.Clear();

				rootDevice.RemoveDevice(embeddedDevice);

				var sentMessages = GetAllSentBroadcasts(server);
				var byebyeNotifications = GetNotificationsByType(sentMessages, "ssdp:byebye");

				var deviceTypeNotifications = GetNotificationsForSearchTarget(byebyeNotifications, String.Format("urn:{0}", embeddedDevice.FullDeviceType));
				var udnNotifications = GetNotificationsForSearchTarget(byebyeNotifications, embeddedDevice.Udn);
				var rootDeviceNotificationsForEmbeddedDevice = GetNotificationsForSearchTarget(byebyeNotifications, SsdpConstants.UpnpDeviceTypeRootDevice).Where((n) =>
				{
					return n.Headers.GetValues("USN").First() == embeddedDevice.Udn + "::" + embeddedDevice.FullDeviceType;
				});

				Assert.IsTrue(deviceTypeNotifications.Count() >= 1);
				Assert.IsTrue(udnNotifications.Count() >= 1);
				Assert.AreEqual(0, rootDeviceNotificationsForEmbeddedDevice.Count());
			}
		}

		[TestMethod]
		public void Publisher_RemoveDevice_RemoveGrandchildSendsNotifications()
		{
			var rootDevice = CreateValidRootDevice();
			var parentDevice = CreateValidEmbeddedDevice(rootDevice);
			rootDevice.AddDevice(parentDevice);
			var embeddedDevice = CreateValidEmbeddedDevice(rootDevice);
			parentDevice.AddDevice(embeddedDevice);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);

				server.WaitForMockBroadcast(10000);
				//System.Threading.Thread.Sleep(100);
				Assert.IsTrue(server.SentBroadcasts.Any());

				server.SentBroadcasts.Clear();

				parentDevice.RemoveDevice(embeddedDevice);

				var sentMessages = GetAllSentBroadcasts(server);
				var byebyeNotifications = GetNotificationsByType(sentMessages, "ssdp:byebye");

				var deviceTypeNotifications = GetNotificationsForSearchTarget(byebyeNotifications, String.Format("urn:{0}", embeddedDevice.FullDeviceType));
				var udnNotifications = GetNotificationsForSearchTarget(byebyeNotifications, embeddedDevice.Udn);
				var rootDeviceNotificationsForEmbeddedDevice = GetNotificationsForSearchTarget(byebyeNotifications, SsdpConstants.UpnpDeviceTypeRootDevice).Where((n) =>
				{
					return n.Headers.GetValues("USN").First() == embeddedDevice.Udn + "::" + embeddedDevice.FullDeviceType;
				});

				Assert.IsTrue(deviceTypeNotifications.Count() >= 1);
				Assert.IsTrue(udnNotifications.Count() >= 1);
				Assert.AreEqual(0, rootDeviceNotificationsForEmbeddedDevice.Count());
			}
		}

		#endregion

		#region AddService Tests

		[TestMethod]
		public void Publisher_AddSevice_BroadcastsServiceTypeNotification()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);
				Assert.IsTrue(server.SentBroadcasts.Any());
				server.SentBroadcasts.Clear();

				var service = new SsdpService()
				{
					ControlUrl = new Uri("/test/control", UriKind.Relative),
					ScpdUrl = new Uri("/test", UriKind.Relative),
					EventSubUrl = new Uri("/test/events", UriKind.Relative),
					ServiceType = "testservicetype",
					ServiceTypeNamespace = "my-namespace",
					ServiceVersion = 1,
					Uuid = System.Guid.NewGuid().ToString()
				};

				rootDevice.AddService(service);
				server.WaitForMockBroadcast(5000);

				var sentMessages = GetAllSentBroadcasts(server);
				var aliveNotifications = GetNotificationsByType(sentMessages, "ssdp:alive");

				var serviceTypeNotifications = GetNotificationsForSearchTarget(aliveNotifications, service.FullServiceType);

				Assert.AreEqual(1, serviceTypeNotifications.Count());
			}
		}

		[TestMethod]
		public void Publisher_AddSevice_ChildDeviceBroadcastsServiceTypeNotification()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);
				var embeddedDevice = CreateValidEmbeddedDevice(rootDevice);
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);
				Assert.IsTrue(server.SentBroadcasts.Any());
				server.SentBroadcasts.Clear();

				var service = new SsdpService()
				{
					ControlUrl = new Uri("/test/control", UriKind.Relative),
					ScpdUrl = new Uri("/test", UriKind.Relative),
					EventSubUrl = new Uri("/test/events", UriKind.Relative),
					ServiceType = "testservicetype",
					ServiceTypeNamespace = "my-namespace",
					ServiceVersion = 1,
					Uuid = System.Guid.NewGuid().ToString()
				};

				embeddedDevice.AddService(service);
				server.WaitForMockBroadcast(5000);

				var sentMessages = GetAllSentBroadcasts(server);
				var aliveNotifications = GetNotificationsByType(sentMessages, "ssdp:alive");

				var serviceTypeNotifications = GetNotificationsForSearchTarget(aliveNotifications, service.FullServiceType);

				Assert.AreEqual(1, serviceTypeNotifications.Count());
			}
		}

		#endregion

		#region RemoveService Tests

		[TestMethod]
		public void Publisher_RemoveService_SendsServiceTypeUpnpByeByeNotification()
		{
			var rootDevice = CreateValidRootDevice();
			rootDevice.CacheLifetime = TimeSpan.FromMinutes(1);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(10000);

				var service = new SsdpService()
				{
					ControlUrl = new Uri("/test/control", UriKind.Relative),
					ScpdUrl = new Uri("/test", UriKind.Relative),
					EventSubUrl = new Uri("/test/events", UriKind.Relative),
					ServiceType = "testservicetype",
					ServiceTypeNamespace = "my-namespace",
					ServiceVersion = 1,
					Uuid = System.Guid.NewGuid().ToString()
				};

				rootDevice.AddService(service);
				server.WaitForMockBroadcast(1000);

				Assert.IsTrue(server.SentBroadcasts.Any());
				server.SentBroadcasts.Clear();
				rootDevice.RemoveService(service);

				var sentMessages = GetAllSentBroadcasts(server);
				var byebyeNotifications = GetNotificationsByType(sentMessages, "ssdp:byebye");

				var serviceNotifications = GetNotificationsForSearchTarget(byebyeNotifications, service.FullServiceType);

				Assert.AreEqual(1, serviceNotifications.Count());
			}
		}

		[TestMethod]
		public void Publisher_RemoveService_DoesNotSendByeByeWhenServiceOfSameTypeRemains()
		{
			var rootDevice = CreateValidRootDevice();
			rootDevice.CacheLifetime = TimeSpan.FromMinutes(1);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(10000);

				var service = new SsdpService()
				{
					ControlUrl = new Uri("/test/control", UriKind.Relative),
					ScpdUrl = new Uri("/test", UriKind.Relative),
					EventSubUrl = new Uri("/test/events", UriKind.Relative),
					ServiceType = "testservicetype",
					ServiceTypeNamespace = "my-namespace",
					ServiceVersion = 1,
					Uuid = System.Guid.NewGuid().ToString()
				};

				var service2 = new SsdpService()
				{
					ControlUrl = new Uri("/test/control", UriKind.Relative),
					ScpdUrl = new Uri("/test", UriKind.Relative),
					EventSubUrl = new Uri("/test/events", UriKind.Relative),
					ServiceType = "testservicetype",
					ServiceTypeNamespace = "my-namespace",
					ServiceVersion = 1,
					Uuid = System.Guid.NewGuid().ToString()
				};

				rootDevice.AddService(service);
				rootDevice.AddService(service2);

				Assert.IsTrue(server.SentBroadcasts.Any());
				server.SentBroadcasts.Clear();
				rootDevice.RemoveService(service);
				server.WaitForMockBroadcast(1000);

				var sentMessages = GetAllSentBroadcasts(server);
				var byebyeNotifications = GetNotificationsByType(sentMessages, "ssdp:byebye");

				var serviceNotifications = GetNotificationsForSearchTarget(byebyeNotifications, service.FullServiceType);

				Assert.AreEqual(0, serviceNotifications.Count());
			}
		}

		#endregion

		#region Periodic Alive Notifications

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Publisher_NotificationBroadcastInterval_ThrowsOnNegativeTimeSpan()
		{
			var rootDevice = CreateValidRootDevice();
			rootDevice.CacheLifetime = TimeSpan.FromMinutes(1);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.NotificationBroadcastInterval = TimeSpan.FromSeconds(-1);
			}
		}

		[TestMethod]
		public void Publisher_SendsPeriodicAliveNotifications()
		{
			var rootDevice = CreateValidRootDevice();
			rootDevice.CacheLifetime = TimeSpan.FromMinutes(1);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);

				Assert.IsTrue(server.SentBroadcasts.Any(), "No broadcasts");
				server.SentBroadcasts.Clear();

				server.WaitForMockBroadcast(35000);

				var sentMessages = GetAllSentBroadcasts(server);
				var aliveNotifications = GetNotificationsByType(sentMessages, "ssdp:alive");

				var upnpRootDeviceNotifications = GetNotificationsForSearchTarget(aliveNotifications, SsdpConstants.UpnpDeviceTypeRootDevice);
				var pnpRootDeviceNotifications = GetNotificationsForSearchTarget(aliveNotifications, SsdpConstants.PnpDeviceTypeRootDevice);
				var udnDeviceNotifications = GetNotificationsForSearchTarget(aliveNotifications, rootDevice.Udn);
				var deviceTypeNotifications = GetNotificationsForSearchTarget(aliveNotifications, rootDevice.FullDeviceType);

				Assert.IsTrue(upnpRootDeviceNotifications.Count() >= 1, "Incorrect number of upnp root device notifications");
				Assert.AreEqual(0, pnpRootDeviceNotifications.Count(), "Incorrect number of pnp root device notifications");
				Assert.IsTrue(udnDeviceNotifications.Count() >= 1, "Incorrect number of udn device notifications");
				Assert.IsTrue(deviceTypeNotifications.Count() >= 1, "Incorrect number of device type notifications");
			}
		}

		[TestMethod]
		public void Publisher_AliveNotifications_ContainCustomHeaders()
		{
			var rootDevice = CreateValidRootDevice();
			rootDevice.CacheLifetime = TimeSpan.FromMinutes(1);
			rootDevice.CustomResponseHeaders.Add(new CustomHttpHeader("TestHeader", "I'm Here"));

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);

				Assert.IsTrue(server.SentBroadcasts.Any(), "No broadcasts");
				server.SentBroadcasts.Clear();

				server.WaitForMockBroadcast(35000);

				var sentMessages = GetAllSentBroadcasts(server);
				var aliveNotifications = GetNotificationsByType(sentMessages, "ssdp:alive");

				var deviceTypeNotifications = GetNotificationsForSearchTarget(aliveNotifications, rootDevice.FullDeviceType);

				Assert.IsTrue(deviceTypeNotifications.First().Headers.Contains("TestHeader"));
			}
		}

		[TestMethod]
		public void Publisher_SendsPeriodicAliveNotifications_RepeatsNotifications()
		{
			var rootDevice = CreateValidRootDevice();
			rootDevice.CacheLifetime = TimeSpan.FromMinutes(1);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);

				publisher.StandardsMode = SsdpStandardsMode.Strict;
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);

				Assert.IsTrue(server.SentBroadcasts.Any());
				server.SentBroadcasts.Clear();

				server.WaitForMockBroadcast(35000);
				//System.Threading.Thread.Sleep(100);
				Assert.IsTrue(server.SentBroadcasts.Any());
				server.SentBroadcasts.Clear();
				server.WaitForMockBroadcast(35000);

				var sentMessages = GetAllSentBroadcasts(server);
				var aliveNotifications = GetNotificationsByType(sentMessages, "ssdp:alive");

				var upnpRootDeviceNotifications = GetNotificationsForSearchTarget(aliveNotifications, SsdpConstants.UpnpDeviceTypeRootDevice);
				var pnpRootDeviceNotifications = GetNotificationsForSearchTarget(aliveNotifications, SsdpConstants.PnpDeviceTypeRootDevice);
				var udnDeviceNotifications = GetNotificationsForSearchTarget(aliveNotifications, rootDevice.Udn);
				var deviceTypeNotifications = GetNotificationsForSearchTarget(aliveNotifications, rootDevice.FullDeviceType);

				Assert.IsTrue(upnpRootDeviceNotifications.Count() >= 1);
				Assert.AreEqual(0, pnpRootDeviceNotifications.Count());
				Assert.IsTrue(udnDeviceNotifications.Count() >= 1);
				Assert.IsTrue(deviceTypeNotifications.Count() >= 1);
			}
		}

		[TestMethod]
		public void Publisher_SendsPeriodicAliveNotifications_UsingConfiguredInterval()
		{
			var rootDevice = CreateValidRootDevice();
			rootDevice.CacheLifetime = TimeSpan.FromMinutes(30);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.NotificationBroadcastInterval = TimeSpan.FromSeconds(30);
				
				publisher.AddDevice(rootDevice);
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);

				Assert.IsTrue(server.SentBroadcasts.Any());
				server.SentBroadcasts.Clear();

				server.WaitForMockBroadcast(35000);

				var sentMessages = GetAllSentBroadcasts(server);
				var aliveNotifications = GetNotificationsByType(sentMessages, "ssdp:alive");

				var upnpRootDeviceNotifications = GetNotificationsForSearchTarget(aliveNotifications, SsdpConstants.UpnpDeviceTypeRootDevice);
				var pnpRootDeviceNotifications = GetNotificationsForSearchTarget(aliveNotifications, SsdpConstants.PnpDeviceTypeRootDevice);
				var udnDeviceNotifications = GetNotificationsForSearchTarget(aliveNotifications, rootDevice.Udn);
				var deviceTypeNotifications = GetNotificationsForSearchTarget(aliveNotifications, rootDevice.FullDeviceType);

				Assert.IsTrue(upnpRootDeviceNotifications.Count() >= 1);
				Assert.AreEqual(0, pnpRootDeviceNotifications.Count());
				Assert.IsTrue(udnDeviceNotifications.Count() >= 1);
				Assert.IsTrue(deviceTypeNotifications.Count() >= 1);
			}
		}

		[TestMethod]
		public void Publisher_SendsRepeatPeriodicAliveNotifications_UsingConfiguredInterval()
		{
			var rootDevice = CreateValidRootDevice();
			rootDevice.CacheLifetime = TimeSpan.FromMinutes(30);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.NotificationBroadcastInterval = TimeSpan.FromSeconds(15);

				publisher.AddDevice(rootDevice);
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);

				Assert.IsTrue(server.SentBroadcasts.Any());
				server.SentBroadcasts.Clear();

				server.WaitForMockBroadcast(35000);
				//System.Threading.Thread.Sleep(100);


				var sentMessages = GetAllSentBroadcasts(server);
				var aliveNotifications = GetNotificationsByType(sentMessages, "ssdp:alive");

				var upnpRootDeviceNotifications = GetNotificationsForSearchTarget(aliveNotifications, SsdpConstants.UpnpDeviceTypeRootDevice);
				var pnpRootDeviceNotifications = GetNotificationsForSearchTarget(aliveNotifications, SsdpConstants.PnpDeviceTypeRootDevice);
				var udnDeviceNotifications = GetNotificationsForSearchTarget(aliveNotifications, rootDevice.Udn);
				var deviceTypeNotifications = GetNotificationsForSearchTarget(aliveNotifications, rootDevice.FullDeviceType);

				Assert.IsTrue(upnpRootDeviceNotifications.Count() >= 1);
				Assert.AreEqual(0, pnpRootDeviceNotifications.Count());
				Assert.IsTrue(udnDeviceNotifications.Count() >= 1);
				Assert.IsTrue(deviceTypeNotifications.Count() >= 1);
			}
		}

		[TestMethod]
		public void Publisher_RemoveLastCachableDeviceStopsPeriodicAliveNotifications()
		{
			var rootDevice = CreateValidRootDevice();
			rootDevice.CacheLifetime = TimeSpan.FromMinutes(1);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(10000);

				//System.Threading.Thread.Sleep(100);

				Assert.IsTrue(server.SentBroadcasts.Any());
				publisher.RemoveDevice(rootDevice);
				server.SentBroadcasts.Clear();

				server.WaitForMockBroadcast(35000);

				var sentMessages = GetAllSentBroadcasts(server);
				Assert.AreEqual(0, sentMessages.Count());
			}
		}

		#endregion

		#region Search Response Tests

		[TestMethod]
		public void Publisher_SearchResponse_RespondsToSearchWithoutMXHeaderInRelaxedMode()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.StandardsMode = SsdpStandardsMode.Relaxed;

				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessageWithoutMXHeader(rootDevice.FullDeviceType);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(0, searchResponses.Where((r) => !r.IsSuccessStatusCode).Count());
				Assert.IsTrue(GetResponses(searchResponses, rootDevice.FullDeviceType).Count() >= 1);
				Assert.AreEqual(0, searchResponses.Where((r) => !r.Headers.GetValues("USN").First().StartsWith(rootDevice.Udn)).Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_RespondsToSearchWithoutMXHeaderInDefaultMode()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.StandardsMode = SsdpStandardsMode.Default;

				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessageWithoutMXHeader(rootDevice.FullDeviceType);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(0, searchResponses.Where((r) => !r.IsSuccessStatusCode).Count());
				Assert.IsTrue(GetResponses(searchResponses, rootDevice.FullDeviceType).Count() >= 1, "No response for full device type");
				Assert.AreEqual(0, searchResponses.Where((r) => !r.Headers.GetValues("USN").First().StartsWith(rootDevice.Udn)).Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_RespondsToUpnpRootDeviceSearch()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.StandardsMode = SsdpStandardsMode.Strict;
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessage(SsdpConstants.UpnpDeviceTypeRootDevice);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(0, searchResponses.Where((r) => !r.IsSuccessStatusCode).Count());
				Assert.AreEqual(1, GetResponses(searchResponses, SsdpConstants.UpnpDeviceTypeRootDevice).Count());
				Assert.AreEqual(0, GetResponsesNotMatchingTarget(searchResponses, SsdpConstants.PnpDeviceTypeRootDevice).Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_AddCustomHeaders()
		{
			var rootDevice = CreateValidRootDevice();

			var testHeader = new CustomHttpHeader("machinename", Environment.MachineName);
			rootDevice.CustomResponseHeaders.Add(testHeader);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.StandardsMode = SsdpStandardsMode.Strict;
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessage(SsdpConstants.UpnpDeviceTypeRootDevice);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);

				Assert.AreEqual(0, searchResponses.Where((r) => !r.IsSuccessStatusCode).Count());
				Assert.IsTrue(GetResponses(searchResponses, SsdpConstants.UpnpDeviceTypeRootDevice).Count() == 1);
				Assert.IsTrue(GetResponses(searchResponses, SsdpConstants.PnpDeviceTypeRootDevice).Count() == 0);
				Assert.AreEqual(0, searchResponses.Where((r) => !r.Headers.GetValues("USN").First().StartsWith(rootDevice.Udn)).Count());
				Assert.AreEqual(0, searchResponses.Where((r) => !r.Headers.GetValues(testHeader.Name).First().StartsWith(testHeader.Value)).Count());
			}

		}

		[TestMethod]
		public void Publisher_SearchResponse_RespondsToPnpRootDeviceSearch()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessage(SsdpConstants.PnpDeviceTypeRootDevice);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);
				//System.Threading.Thread.Sleep(100);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(0, searchResponses.Where((r) => !r.IsSuccessStatusCode).Count());
				Assert.AreEqual(2, searchResponses.Count());
				Assert.AreEqual(1, GetResponses(searchResponses, SsdpConstants.UpnpDeviceTypeRootDevice).Count());
				Assert.AreEqual(1, GetResponses(searchResponses, SsdpConstants.PnpDeviceTypeRootDevice).Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_RespondsToUdnSearch()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessage(rootDevice.Udn);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);
				//System.Threading.Thread.Sleep(100);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(0, searchResponses.Where((r) => !r.IsSuccessStatusCode).Count());
				Assert.AreEqual(1, searchResponses.Count());
				Assert.AreEqual(1, GetResponses(searchResponses, rootDevice.Udn).Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_RespondsToDeviceTypeSearch()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessage(rootDevice.FullDeviceType);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(0, searchResponses.Where((r) => !r.IsSuccessStatusCode).Count());
				Assert.AreEqual(1, searchResponses.Count());
				Assert.AreEqual(1, GetResponses(searchResponses, rootDevice.FullDeviceType).Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_NoResponseToBadSearchTarget()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessage("blah");

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(0, searchResponses.Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_NoResponseToBlankSearchTarget()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessage(String.Empty);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(0, searchResponses.Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_ResponseWithMissngManHeaderInDefaultMode()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.StandardsMode = SsdpStandardsMode.Default;
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessageWithoutManHeader(SsdpConstants.UpnpDeviceTypeRootDevice);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);

				Assert.AreEqual(0, searchResponses.Where((r) => !r.IsSuccessStatusCode).Count());
				Assert.AreEqual(1, GetResponses(searchResponses, SsdpConstants.UpnpDeviceTypeRootDevice).Count());
				Assert.AreEqual(1, GetResponses(searchResponses, SsdpConstants.PnpDeviceTypeRootDevice).Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_ResponseWithMissngManHeaderInRelaxedMode()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.StandardsMode = SsdpStandardsMode.Relaxed;
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessageWithoutManHeader(SsdpConstants.UpnpDeviceTypeRootDevice);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(0, searchResponses.Where((r) => !r.IsSuccessStatusCode).Count());
				Assert.AreEqual(1, GetResponses(searchResponses, SsdpConstants.UpnpDeviceTypeRootDevice).Count());
				Assert.AreEqual(1, GetResponses(searchResponses, SsdpConstants.PnpDeviceTypeRootDevice).Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_NoResponseWithMissngManHeaderInStrictMode()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.StandardsMode = SsdpStandardsMode.Strict;
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessageWithoutManHeader(SsdpConstants.UpnpDeviceTypeRootDevice);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(0, searchResponses.Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_IgnoresDuplicateSearchRequest()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessage(SsdpConstants.UpnpDeviceTypeRootDevice);

				server.MockReceiveMessage(searchRequest);
				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);
				//System.Threading.Thread.Sleep(500);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(0, searchResponses.Where((r) => !r.IsSuccessStatusCode).Count());
				Assert.IsTrue(GetResponses(searchResponses, SsdpConstants.UpnpDeviceTypeRootDevice).Count() == 1);
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_RespondsToNonDuplicateSearchRequest()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessage(SsdpConstants.UpnpDeviceTypeRootDevice);
				ReceivedUdpData searchRequest2 = GetSearchRequestMessage(SsdpConstants.PnpDeviceTypeRootDevice);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);
				server.MockReceiveMessage(searchRequest2);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(0, searchResponses.Where((r) => !r.IsSuccessStatusCode).Count());
				Assert.IsTrue(searchResponses.Count() == 4);
				Assert.IsTrue(GetResponses(searchResponses, SsdpConstants.UpnpDeviceTypeRootDevice).Count() == 2);
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_DoesNotIgnoreDelayedDuplicateSearchRequest()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.StandardsMode = SsdpStandardsMode.Strict;
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessage(SsdpConstants.UpnpDeviceTypeRootDevice);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMessageToProcess(5000);
				server.WaitForMockMessage(1500);
				server.SentMessages.Clear();

				System.Threading.Thread.Sleep(500); //Delay the second message
				server.MockReceiveMessage(searchRequest);

				var started = DateTime.Now;
				var searchResponses = GetSentMessages(server.SentMessages).ToArray();
				while (searchResponses.Length < 5 && DateTime.Now.Subtract(started).TotalSeconds < 10)
				{
					server.WaitForMessageToProcess(100);
					server.WaitForMockMessage(100);
					searchResponses = searchResponses.Union(GetSentMessages(server.SentMessages).ToArray()).ToArray();
				}
				//System.Threading.Thread.Sleep(1000);

				Assert.AreEqual(0, searchResponses.Where((r) => !r.IsSuccessStatusCode).Count());
				Assert.IsTrue(GetResponses(searchResponses, SsdpConstants.UpnpDeviceTypeRootDevice).Count() > 0);
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_SendsChildResponses()
		{
			var rootDevice = CreateValidRootDevice();
			var childDevice = CreateValidEmbeddedDevice(rootDevice);
			rootDevice.AddDevice(childDevice);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.StandardsMode = SsdpStandardsMode.Strict;
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessage(childDevice.Udn);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);
				var uuidResponses = GetResponses(searchResponses, childDevice.Udn);
				Assert.AreEqual(1, uuidResponses.Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_SendsGrandchildResponses()
		{
			var rootDevice = CreateValidRootDevice();
			var parentDevice = CreateValidEmbeddedDevice(rootDevice);
			rootDevice.AddDevice(parentDevice);
			var childDevice = CreateValidEmbeddedDevice(rootDevice);
			parentDevice.AddDevice(childDevice);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessage(childDevice.Udn);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);
				//System.Threading.Thread.Sleep(100);

				var searchResponses = GetSentMessages(server.SentMessages);
				var uuidResponses = GetResponses(searchResponses, childDevice.Udn);
				Assert.AreEqual(1, uuidResponses.Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_RespondsToAllSearch()
		{
			var rootDevice = CreateValidRootDevice();
			var service = new SsdpService()
			{
				ControlUrl = new Uri("/test/control", UriKind.Relative),
				ScpdUrl = new Uri("/test", UriKind.Relative),
				EventSubUrl = new Uri("/test/events", UriKind.Relative),
				ServiceType = "testservicetype",
				ServiceTypeNamespace = "my-namespace",
				ServiceVersion = 1,
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddService(service);

			var parentDevice = CreateValidEmbeddedDevice(rootDevice);
			rootDevice.AddDevice(parentDevice);
			var childDevice = CreateValidEmbeddedDevice(rootDevice);
			parentDevice.AddDevice(childDevice);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessage("ssdp:all");

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);
				//System.Threading.Thread.Sleep(500);

				var searchResponses = GetSentMessages(server.SentMessages);
				var rootUuidResponses = GetResponses(searchResponses, childDevice.Udn);
				var parentUuidResponses = GetResponses(searchResponses, childDevice.Udn);
				var childUuidResponses = GetResponses(searchResponses, childDevice.Udn);
				var deviceTypeResponses = GetResponses(searchResponses, childDevice.FullDeviceType).Union(GetResponses(searchResponses, rootDevice.DeviceType));
				var rootDeviceResponses = GetResponses(searchResponses, SsdpConstants.UpnpDeviceTypeRootDevice);
				var pnpRootDeviceResponses = GetResponses(searchResponses, SsdpConstants.UpnpDeviceTypeRootDevice);
				var serviceResponses = GetResponses(searchResponses, service.FullServiceType);

				Assert.AreEqual(1, rootUuidResponses.Count());
				Assert.AreEqual(1, parentUuidResponses.Count());
				Assert.AreEqual(1, childUuidResponses.Count());
				Assert.AreEqual(2, deviceTypeResponses.Count());
				Assert.AreEqual(1, rootDeviceResponses.Count());
				Assert.AreEqual(1, pnpRootDeviceResponses.Count());
				Assert.AreEqual(1, serviceResponses.Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_IgnoresNullMessageReceipt()
		{
			var rootDevice = CreateValidRootDevice();
			var parentDevice = CreateValidEmbeddedDevice(rootDevice);
			rootDevice.AddDevice(parentDevice);
			var childDevice = CreateValidEmbeddedDevice(rootDevice);
			parentDevice.AddDevice(childDevice);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessage("ssdp:all");
				searchRequest.Buffer = null;

				server.MockReceiveMessage(searchRequest);
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_IgnoresEmptyMessageReceipt()
		{
			var rootDevice = CreateValidRootDevice();
			var parentDevice = CreateValidEmbeddedDevice(rootDevice);
			rootDevice.AddDevice(parentDevice);
			var childDevice = CreateValidEmbeddedDevice(rootDevice);
			parentDevice.AddDevice(childDevice);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessage(childDevice.Udn);
				searchRequest.Buffer = new byte[] { };

				server.MockReceiveMessage(searchRequest);
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_RespondsToServiceTypeDeviceSearch()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.StandardsMode = SsdpStandardsMode.Strict;
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete

				var service = new SsdpService()
				{
					ControlUrl = new Uri("/test/control", UriKind.Relative),
					ScpdUrl = new Uri("/test", UriKind.Relative),
					EventSubUrl = new Uri("/test/events", UriKind.Relative),
					ServiceType = "testservicetype",
					ServiceTypeNamespace = "my-namespace",
					ServiceVersion = 1,
					Uuid = System.Guid.NewGuid().ToString()
				};

				rootDevice.AddService(service);
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(1000);
				server.SentBroadcasts.Clear();
				server.SentMessages.Clear();

				ReceivedUdpData searchRequest = GetSearchRequestMessage(service.FullServiceType);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(1, GetResponses(searchResponses, service.FullServiceType).Count());
				Assert.AreEqual(1, searchResponses.Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_RespondsToServiceTypeDeviceSearch_WithEmbeddedDeviceService()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.StandardsMode = SsdpStandardsMode.Strict;
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete

				var edevice = CreateValidEmbeddedDevice(rootDevice);
				rootDevice.AddDevice(edevice);

				var service = new SsdpService()
				{
					ControlUrl = new Uri("/test/control", UriKind.Relative),
					ScpdUrl = new Uri("/test", UriKind.Relative),
					EventSubUrl = new Uri("/test/events", UriKind.Relative),
					ServiceType = "testservicetype",
					ServiceTypeNamespace = "my-namespace",
					ServiceVersion = 1,
					Uuid = System.Guid.NewGuid().ToString()
				};

				edevice.AddService(service);
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(1000);
				server.SentBroadcasts.Clear();
				server.SentMessages.Clear();

				ReceivedUdpData searchRequest = GetSearchRequestMessage(service.FullServiceType);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(1, GetResponses(searchResponses, service.FullServiceType).Count());
				Assert.AreEqual(1, searchResponses.Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_RespondsToServiceTypeDeviceSearchOncePerServiceType()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.StandardsMode = SsdpStandardsMode.Strict;
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete

				var service = new SsdpService()
				{
					ControlUrl = new Uri("/test/control", UriKind.Relative),
					ScpdUrl = new Uri("/test", UriKind.Relative),
					EventSubUrl = new Uri("/test/events", UriKind.Relative),
					ServiceType = "testservicetype",
					ServiceTypeNamespace = "my-namespace",
					ServiceVersion = 1,
					Uuid = System.Guid.NewGuid().ToString()
				};

				var service2 = new SsdpService()
				{
					ControlUrl = new Uri("/test2/control", UriKind.Relative),
					ScpdUrl = new Uri("/test2", UriKind.Relative),
					EventSubUrl = new Uri("/test2/events", UriKind.Relative),
					ServiceType = "testservicetype",
					ServiceTypeNamespace = "my-namespace",
					ServiceVersion = 1,
					Uuid = System.Guid.NewGuid().ToString()
				};

				rootDevice.AddService(service);
				rootDevice.AddService(service2);
				publisher.AddDevice(rootDevice);
				server.WaitForMockBroadcast(1000);
				server.SentBroadcasts.Clear();
				server.SentMessages.Clear();

				ReceivedUdpData searchRequest = GetSearchRequestMessage(service.FullServiceType);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(1, GetResponses(searchResponses, service.FullServiceType).Count());
				Assert.AreEqual(1, searchResponses.Count());
			}
		}

		#region MX Header Checks

		[TestMethod]
		public void Publisher_SearchResponse_RespondsToRequestWithEmptyMXHeaderIfPnpSupportEnabled()
		{
			var rootDevice = CreateValidRootDevice();
			var parentDevice = CreateValidEmbeddedDevice(rootDevice);
			rootDevice.AddDevice(parentDevice);
			var childDevice = CreateValidEmbeddedDevice(rootDevice);
			parentDevice.AddDevice(childDevice);

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = true;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessageWithCustomMXHeader(childDevice.Udn, String.Empty);
				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);
				//System.Threading.Thread.Sleep(500);

				var searchResponses = GetSentMessages(server.SentMessages);
				var uuidResponses = GetResponses(searchResponses, childDevice.Udn);
				Assert.AreEqual(1, uuidResponses.Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_NoResponseWithMissngMXHeaderInStandardsMode()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.StandardsMode = SsdpStandardsMode.Strict;
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessageWithoutMXHeader(SsdpConstants.UpnpDeviceTypeRootDevice);

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(0, searchResponses.Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_NoResponseWithNegativeMXHeader()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessageWithCustomMXHeader(rootDevice.Udn, "A");

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(0, searchResponses.Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_NoResponseWithNonNumericMXHeader()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessageWithCustomMXHeader(rootDevice.Udn, "-1");

				server.MockReceiveMessage(searchRequest);
				server.WaitForMockMessage(1500);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(0, searchResponses.Count());
			}
		}

		[TestMethod]
		public void Publisher_SearchResponse_RandomisesMxHeaderGreaterThan120()
		{
			var rootDevice = CreateValidRootDevice();

			var server = new MockCommsServer();
			using (var publisher = new TestDevicePublisher(server))
			{
				publisher.StandardsMode = SsdpStandardsMode.Strict;
#pragma warning disable CS0618 // Type or member is obsolete
				publisher.SupportPnpRootDevice = false;
#pragma warning restore CS0618 // Type or member is obsolete
				publisher.AddDevice(rootDevice);

				ReceivedUdpData searchRequest = GetSearchRequestMessageWithCustomMXHeader(rootDevice.Udn, "125");

				server.MockReceiveMessage(searchRequest);
				Assert.IsTrue(server.WaitForMockMessage(120000));
				//System.Threading.Thread.Sleep(500);

				var searchResponses = GetSentMessages(server.SentMessages);
				Assert.AreEqual(1, searchResponses.Count());
			}
		}

		#endregion

		#endregion

		#region Support Methods

		private static SsdpRootDevice CreateValidRootDevice()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};
			return rootDevice;
		}

		private SsdpEmbeddedDevice CreateValidEmbeddedDevice(SsdpRootDevice rootDevice)
		{
			var uuid = Guid.NewGuid().ToString();

			var retVal = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Test Embedded Device " + uuid,
				Manufacturer = "Test Manufacturer",
				ModelName = "Test Model",
				Uuid = uuid
			};

			rootDevice.AddDevice(retVal);
			return retVal;
		}

		private IEnumerable<System.Net.Http.HttpRequestMessage> GetNotificationsForSearchTarget(IEnumerable<System.Net.Http.HttpRequestMessage> requests, string searchTarget)
		{
			return (from n in requests where n.Method.Method == "NOTIFY" && (String.IsNullOrEmpty(searchTarget) || n.Headers.GetValues("NT").First() == searchTarget) select n);
		}

		private IEnumerable<System.Net.Http.HttpRequestMessage> GetNotificationsByType(IEnumerable<System.Net.Http.HttpRequestMessage> requests, string notificationType)
		{
			return (from n in requests where n.Headers.GetValues("NTS").First() == notificationType select n).ToArray();
		}

		private IEnumerable<System.Net.Http.HttpRequestMessage> GetAllSentBroadcasts(MockCommsServer server)
		{
			var parser = new HttpRequestParser();

			var retVal = new List<System.Net.Http.HttpRequestMessage>();
			while (server.SentBroadcasts.Any())
			{
				var sentBroadcast = server.SentBroadcasts.Dequeue();
				retVal.Add(parser.Parse(System.Text.UTF8Encoding.ASCII.GetString(sentBroadcast.Buffer)));
			}
			return retVal;
		}

		private IEnumerable<System.Net.Http.HttpResponseMessage> GetSentMessages(Queue<ReceivedUdpData> queue)
		{
			var parser = new HttpResponseParser();

			var retVal = new List<System.Net.Http.HttpResponseMessage>();
			while (queue.Any())
			{
				var receivedResponse = queue.Dequeue();
				retVal.Add(parser.Parse(System.Text.UTF8Encoding.ASCII.GetString(receivedResponse.Buffer)));
			}
			return retVal;
		}

		private ReceivedUdpData GetSearchRequestMessage(string searchTarget)
		{
			var retVal = new ReceivedUdpData()
			{
				Buffer = System.Text.UTF8Encoding.UTF8.GetBytes(String.Format(@"M-SEARCH * HTTP/1.1
HOST: {0}:{1}
MAN: ""ssdp:discover""
MX: 1
ST: {2}

",
	 SsdpConstants.MulticastLocalAdminAddress,
	 SsdpConstants.MulticastPort,
	 searchTarget)),
				ReceivedFrom = new UdpEndPoint("192.168.1.100", 1701)
			};
			retVal.ReceivedBytes = retVal.Buffer.Length;

			return retVal;
		}

		private ReceivedUdpData GetSearchRequestMessageWithoutMXHeader(string searchTarget)
		{
			var retVal = new ReceivedUdpData()
			{
				Buffer = System.Text.UTF8Encoding.UTF8.GetBytes(String.Format(@"M-SEARCH * HTTP/1.1
HOST: {0}:{1}
MAN: ""ssdp:discover""
ST: {2}

",
	 SsdpConstants.MulticastLocalAdminAddress,
	 SsdpConstants.MulticastPort,
	 searchTarget)),
				ReceivedFrom = new UdpEndPoint("192.168.1.100", 1701)
			};
			retVal.ReceivedBytes = retVal.Buffer.Length;

			return retVal;
		}

		private ReceivedUdpData GetSearchRequestMessageWithCustomMXHeader(string searchTarget, string mxHeder)
		{
			var retVal = new ReceivedUdpData()
			{
				Buffer = System.Text.UTF8Encoding.UTF8.GetBytes(String.Format(@"M-SEARCH * HTTP/1.1
HOST: {0}:{1}
MX:{3}
MAN: ""ssdp:discover""
ST: {2}

",
	 SsdpConstants.MulticastLocalAdminAddress,
	 SsdpConstants.MulticastPort,
	 searchTarget,
	 mxHeder)),
				ReceivedFrom = new UdpEndPoint("192.168.1.100", 1701)
			};
			retVal.ReceivedBytes = retVal.Buffer.Length;

			return retVal;
		}

		private ReceivedUdpData GetSearchRequestMessageWithoutManHeader(string searchTarget)
		{
			var retVal = new ReceivedUdpData()
			{
				Buffer = System.Text.UTF8Encoding.UTF8.GetBytes(String.Format(@"M-SEARCH * HTTP/1.1
HOST: {0}:{1}
MX: 1
ST: {2}

",
	 SsdpConstants.MulticastLocalAdminAddress,
	 SsdpConstants.MulticastPort,
	 searchTarget)),
				ReceivedFrom = new UdpEndPoint("192.168.1.100", 1701)
			};
			retVal.ReceivedBytes = retVal.Buffer.Length;

			return retVal;
		}

		private IEnumerable<System.Net.Http.HttpResponseMessage> GetResponses(IEnumerable<System.Net.Http.HttpResponseMessage> searchResponses, string searchTarget)
		{
			return (from r in searchResponses where r.Headers.GetValues("ST").First() == searchTarget select r);
		}

		private IEnumerable<System.Net.Http.HttpResponseMessage> GetResponsesNotMatchingTarget(IEnumerable<System.Net.Http.HttpResponseMessage> searchResponses, string searchTarget)
		{
			return (from r in searchResponses where r.Headers.GetValues("ST").First() == searchTarget select r);
		}

		#endregion

	}
}