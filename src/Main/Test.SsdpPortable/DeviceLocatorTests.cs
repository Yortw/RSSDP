using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp;
using Rssdp.Infrastructure;

namespace Test.RssdpPortable
{
	[TestClass]
	public class DeviceLocatorTests
	{

		#region Constructor Tests

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod()]
		public void DeviceLocator_Constructor_ThrowsOnNullCommsServer()
		{
			var deviceLocator = new MockDeviceLocator(null);
		}

		#endregion

		#region Notifications Tests

		[ExpectedException(typeof(System.ObjectDisposedException))]
		[TestMethod()]
		public void DeviceLocator_Notifications_StartListeningThrowsIfDisposed()
		{
			var deviceLocator = new MockDeviceLocator();
			deviceLocator.Dispose();

			deviceLocator.StartListeningForNotifications();
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_ReceivesAliveNotifications()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);
			DiscoveredSsdpDevice device = null;
			bool newlyDiscovered = false;
			var receivedNotification = false;

			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceAvailable += (sender, args) =>
					{
						device = args.DiscoveredDevice;
						newlyDiscovered = args.IsNewlyDiscovered;
						receivedNotification = true;
						eventSignal.Set();
					};
				deviceLocator.StartListeningForNotifications();

				server.MockReceiveBroadcast(GetMockAliveNotification());
				eventSignal.WaitOne(10000);
			}

			Assert.IsTrue(receivedNotification);
			Assert.IsNotNull(device);
			Assert.IsTrue(newlyDiscovered);
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_DoesNotRaiseDeviceAvailableIfDisposed()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);
			DiscoveredSsdpDevice device = null;
			bool newlyDiscovered = false;

			var receivedNotification = false;
			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceAvailable += (sender, args) =>
				{
					device = args.DiscoveredDevice;
					newlyDiscovered = args.IsNewlyDiscovered;
					receivedNotification = true;
				};
				deviceLocator.StartListeningForNotifications();

				server.MockReceiveBroadcast(GetMockAliveNotification());
				server.Dispose();
				eventSignal.WaitOne(1000);
			}

			Assert.IsFalse(receivedNotification);
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_ReceivesByeByeNotificationsForKnownDevice()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);
			var receivedNotification = false;
			DiscoveredSsdpDevice device = null;
			bool expired = false;

			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceUnavailable += (sender, args) =>
				{
					device = args.DiscoveredDevice;
					expired = args.Expired;
					receivedNotification = true;
					eventSignal.Set();
				};
				deviceLocator.StartListeningForNotifications();
				server.MockReceiveBroadcast(GetMockAliveNotification());
				server.WaitForMessageToProcess(10000);

				server.MockReceiveBroadcast(GetMockByeByeNotification());
				eventSignal.WaitOne(10000);
			}

			Assert.IsTrue(receivedNotification);
			Assert.IsNotNull(device);
			Assert.IsFalse(expired);
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_ReceivesByeByeNotificationsForUnknownDevice()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);
			var receivedNotification = false;
			DiscoveredSsdpDevice device = null;
			bool expired = false;

			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceUnavailable += (sender, args) =>
				{
					device = args.DiscoveredDevice;
					expired = args.Expired;
					receivedNotification = true;
					eventSignal.Set();
				};
				deviceLocator.StartListeningForNotifications();

				server.MockReceiveBroadcast(GetMockByeByeNotification());
				eventSignal.WaitOne(10000);
			}

			Assert.IsTrue(receivedNotification);
			Assert.IsNotNull(device);
			Assert.IsFalse(expired);
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_DoesNotRaiseDeviceUnavailableIfDisposed()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);
			var receivedNotification = false;
			DiscoveredSsdpDevice device = null;
			bool expired = false;

			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceUnavailable += (sender, args) =>
				{
					device = args.DiscoveredDevice;
					expired = args.Expired;
					receivedNotification = true;
					eventSignal.Set();
				};
				deviceLocator.StartListeningForNotifications();

				server.MockReceiveBroadcast(GetMockByeByeNotification());
				server.Dispose();
				eventSignal.WaitOne(1000);
			}

			Assert.IsFalse(receivedNotification);
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_StopListeningNoLongerReceivesNotifications()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);
			deviceLocator.StartListeningForNotifications();
			deviceLocator.StopListeningForNotifications();

			var receivedNotification = false;
			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceAvailable += (sender, args) =>
				{
					receivedNotification = true;
					eventSignal.Set();
				};

				server.MockReceiveBroadcast(GetMockAliveNotification());
				eventSignal.WaitOne(1000);
			}

			Assert.IsFalse(receivedNotification);
		}

		[ExpectedException(typeof(System.ObjectDisposedException))]
		[TestMethod()]
		public void DeviceLocator_Notifications_Notifications_StopListeningThrowsIfDisposed()
		{
			var deviceLocator = new MockDeviceLocator();
			deviceLocator.Dispose();

			deviceLocator.StopListeningForNotifications();
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_IgnoresNonNotifyRequest()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);
			var receivedNotification = false;
			DiscoveredSsdpDevice device = null;
			bool expired = false;

			deviceLocator.DeviceUnavailable += (sender, args) =>
			{
				device = args.DiscoveredDevice;
				expired = args.Expired;
				receivedNotification = true;
			};
			deviceLocator.StartListeningForNotifications();

			server.MockReceiveBroadcast(GetMockNonNotifyRequest());
			server.WaitForMessageToProcess(10000);
			server.Dispose();

			Assert.IsFalse(receivedNotification);
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_DoesNotRaiseDeviceAvailableWithUnmatchedNotificationFilter()
		{
			var publishedDevice = CreateDeviceTree();

			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);
			var discoveredDevices = new List<DiscoveredSsdpDevice>();

			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceAvailable += (sender, args) =>
				{
					discoveredDevices.Add(args.DiscoveredDevice);
					eventSignal.Set();
				};
				deviceLocator.NotificationFilter = publishedDevice.Devices.First().Udn;
				deviceLocator.StartListeningForNotifications();

				server.MockReceiveBroadcast(GetMockAliveNotification(publishedDevice));
				eventSignal.WaitOne(1000);
				server.MockReceiveBroadcast(GetMockAliveNotification(publishedDevice.Devices.First()));
				eventSignal.WaitOne(1000);
				server.MockReceiveBroadcast(GetMockAliveNotification(publishedDevice.Devices.First().Devices.First()));
				eventSignal.WaitOne(1000);
			}

			Assert.IsTrue(discoveredDevices.Any());
			Assert.IsFalse(discoveredDevices.Any((d) => { return !d.Usn.StartsWith(publishedDevice.Devices.First().Udn); }));
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_RaisesDeviceAvailableWithMatchedNotificationFilter()
		{
			var publishedDevice = CreateDeviceTree();

			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);
			var discoveredDevices = new List<DiscoveredSsdpDevice>();

			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceAvailable += (sender, args) =>
				{
					discoveredDevices.Add(args.DiscoveredDevice);
					eventSignal.Set();
				};
				deviceLocator.NotificationFilter = "uuid: " + System.Guid.NewGuid().ToString();
				deviceLocator.StartListeningForNotifications();

				server.MockReceiveBroadcast(GetMockAliveNotification(publishedDevice));
				eventSignal.WaitOne(1000);
				server.MockReceiveBroadcast(GetMockAliveNotification(publishedDevice.Devices.First()));
				eventSignal.WaitOne(1000);
				server.MockReceiveBroadcast(GetMockAliveNotification(publishedDevice.Devices.First().Devices.First()));
				eventSignal.WaitOne(1000);
			}

			Assert.IsFalse(discoveredDevices.Any());
		}

		[TestMethod]
		public void DeviceLocator_Notifications_RaisesDeviceUnavailableWithMatchedNotificationFilter()
		{
			var publishedDevice = CreateDeviceTree();

			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);
			var discoveredDevices = new List<DiscoveredSsdpDevice>();

			using (var eventSignal = new System.Threading.ManualResetEvent(false))
			{
				deviceLocator.DeviceUnavailable += (sender, args) =>
				{
					discoveredDevices.Add(args.DiscoveredDevice);
					eventSignal.Set();
				};
				deviceLocator.NotificationFilter = publishedDevice.Devices.First().Udn;
				deviceLocator.StartListeningForNotifications();

				server.MockReceiveBroadcast(GetMockByeByeNotification(publishedDevice));
				server.WaitForMessageToProcess(10000);
				server.MockReceiveBroadcast(GetMockByeByeNotification(publishedDevice.Devices.First().Devices.First()));
				server.WaitForMessageToProcess(10000);
				server.MockReceiveBroadcast(GetMockByeByeNotification(publishedDevice.Devices.First()));
				server.WaitForMessageToProcess(10000);
				eventSignal.WaitOne(10000);
			}

			Assert.IsTrue(discoveredDevices.Any());
			Assert.IsFalse(discoveredDevices.Any((d) => { return !d.Usn.StartsWith(publishedDevice.Devices.First().Udn); }));
		}

		[TestMethod]
		public void DeviceLocator_Notifications_DoesNotRaiseDeviceUnavailableWithUnmatchedNotificationFilter()
		{
			var publishedDevice = CreateDeviceTree();

			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);
			var discoveredDevices = new List<DiscoveredSsdpDevice>();

			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceUnavailable += (sender, args) =>
				{
					discoveredDevices.Add(args.DiscoveredDevice);
					eventSignal.Set();
				};
				deviceLocator.NotificationFilter = "uuid:" + Guid.NewGuid().ToString();
				deviceLocator.StartListeningForNotifications();

				server.MockReceiveBroadcast(GetMockByeByeNotification(publishedDevice));
				eventSignal.WaitOne(1000);
				server.MockReceiveBroadcast(GetMockByeByeNotification(publishedDevice.Devices.First().Devices.First()));
				eventSignal.WaitOne(1000);
				server.MockReceiveBroadcast(GetMockByeByeNotification(publishedDevice.Devices.First()));
				eventSignal.WaitOne(1000);
			}

			Assert.IsFalse(discoveredDevices.Any());
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_SubsequentNotificationsUpdatesSearchResults()
		{
			var publishedDevice = CreateDeviceTree();

			var server = new MockCommsServer();
			using (var deviceLocator = new MockDeviceLocator(server))
			{
				var discoveredDevices = new List<DiscoveredSsdpDevice>();

				using (var signal = new System.Threading.AutoResetEvent(false))
				{
					deviceLocator.DeviceAvailable += (sender, args) =>
					{
						discoveredDevices.Add(args.DiscoveredDevice);
						signal.Set();
					};
					deviceLocator.StartListeningForNotifications();

					server.MockReceiveBroadcast(GetMockAliveNotification(publishedDevice));
					signal.WaitOne(10000);

					var updatedDevice = CreateDeviceTree();
					updatedDevice.Uuid = publishedDevice.Uuid;
					updatedDevice.Location = new Uri("http://somewhereelse:1701");
					updatedDevice.CacheLifetime = TimeSpan.FromDays(365);
					server.MockReceiveBroadcast(GetMockAliveNotification(updatedDevice));
					signal.WaitOne(10000);
				}

				var first = discoveredDevices.First();
				var second = discoveredDevices.Last();

				Assert.IsTrue(discoveredDevices.Any());
				Assert.AreNotEqual(first.DescriptionLocation, second.DescriptionLocation);
				Assert.AreNotEqual(first.CacheLifetime, second.CacheLifetime);

				Assert.AreEqual(second.CacheLifetime, TimeSpan.FromDays(365));
				Assert.AreEqual(second.DescriptionLocation, new Uri("http://somewhereelse:1701"));
			}
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_SubsequentNotificationsUpdatesCachedDescriptionLocation()
		{
			var publishedDevice = CreateDeviceTree();

			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);
			var discoveredDevices = new List<DiscoveredSsdpDevice>();

			using (var signal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceAvailable += (sender, args) =>
				{
					discoveredDevices.Add(args.DiscoveredDevice);
					signal.Set();
				};
				deviceLocator.StartListeningForNotifications();

				server.MockReceiveBroadcast(GetMockAliveNotification(publishedDevice));
				signal.WaitOne(10000);

				var t = deviceLocator.SearchAsync(TimeSpan.FromSeconds(5));

				var updatedDevice = CreateDeviceTree();
				updatedDevice.Uuid = publishedDevice.Uuid;
				updatedDevice.Location = new Uri("http://somewhereelse:1701");
				server.MockReceiveBroadcast(GetMockAliveNotification(updatedDevice));
				signal.WaitOne(10000);

				var results = t.GetAwaiter().GetResult();
				Assert.IsNotNull(results);
				Assert.IsTrue(results.Any());
				Assert.AreEqual(String.Format("{0}::{1}", publishedDevice.Udn, publishedDevice.FullDeviceType), discoveredDevices.Last().Usn);

				var first = discoveredDevices.First();
				var second = discoveredDevices.Last();

				Assert.AreNotEqual(first.DescriptionLocation, second.DescriptionLocation);

				Assert.AreEqual(second.DescriptionLocation, new Uri("http://somewhereelse:1701"));
			}
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_ContainHeaders()
		{
            var publishedDevice = CreateDeviceTree();

            var server = new MockCommsServer();
            using (var deviceLocator = new MockDeviceLocator(server))
            {
                var discoveredDevices = new List<DiscoveredSsdpDevice>();

                using (var signal = new System.Threading.AutoResetEvent(false))
                {
                    deviceLocator.DeviceAvailable += (sender, args) =>
                    {
                        discoveredDevices.Add(args.DiscoveredDevice);
                        signal.Set();
                    };
                    deviceLocator.StartListeningForNotifications();

                    server.MockReceiveBroadcast(GetMockAliveNotification(publishedDevice));
                    signal.WaitOne(10000);
                }

                var first = discoveredDevices.First();

                Assert.IsTrue(discoveredDevices.Any());
                Assert.IsNotNull(first.ResponseHeaders);
                Assert.AreEqual(first.ResponseHeaders.GetValues("NTS").FirstOrDefault(), "ssdp:alive");
            }
        }

		[TestMethod()]
		public void DeviceLocator_Notifications_SubsequentNotificationsUpdatesCachedCacheTime()
		{
			var publishedDevice = CreateDeviceTree();

			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);
			var discoveredDevices = new List<DiscoveredSsdpDevice>();

			using (var signal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceAvailable += (sender, args) =>
				{
					discoveredDevices.Add(args.DiscoveredDevice);
					signal.Set();
				};
				deviceLocator.StartListeningForNotifications();

				server.MockReceiveBroadcast(GetMockAliveNotification(publishedDevice));
				signal.WaitOne(10000);

				var t = deviceLocator.SearchAsync(TimeSpan.FromSeconds(5));

				var updatedDevice = CreateDeviceTree();
				updatedDevice.Uuid = publishedDevice.Uuid;
				updatedDevice.CacheLifetime = TimeSpan.FromDays(365);
				server.MockReceiveBroadcast(GetMockAliveNotification(updatedDevice));
				signal.WaitOne(10000);

				var results = t.GetAwaiter().GetResult();
				Assert.IsNotNull(results);
				Assert.IsTrue(results.Any());
				Assert.AreEqual(String.Format("{0}::{1}", publishedDevice.Udn, publishedDevice.FullDeviceType), discoveredDevices.Last().Usn);

				var first = discoveredDevices.First();
				var second = discoveredDevices.Last();

				Assert.AreNotEqual(first.CacheLifetime, second.CacheLifetime);

				Assert.AreEqual(second.CacheLifetime, TimeSpan.FromDays(365));
			}
		}

		#endregion

		#region Search Tests

		//Test we do throw an object disposed exception while listening
		//if stop listening is called while a search is in progress, and not
		//some other kind of exception such as null reference.
		[TestMethod]
		public void DeviceLocator_SearchAsync_HandlesConcurrentDispose()
		{
			using (var deviceLocator = new Rssdp.SsdpDeviceLocator())
			{
				System.Threading.ThreadPool.QueueUserWorkItem((reserved) =>
					{
						System.Threading.Thread.Sleep(50);
						deviceLocator.Dispose();
					});

				var t = deviceLocator.SearchAsync();

				AggregateException exception = null;
				try
				{
					t.Wait();
				}
				catch (AggregateException aex)
				{
					exception = aex;
				}

				Assert.AreNotEqual(null, exception);
				Assert.AreEqual(1, exception.InnerExceptions.Count);
				Assert.AreEqual(typeof(System.ObjectDisposedException), exception.InnerExceptions.First().GetType());
			}
		}

		[TestMethod]
		public void DeviceLocator_SearchAsync_SearchesForAllDevices()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			var t = deviceLocator.SearchAsync();
			t.Wait();

			Assert.IsTrue(server.SentBroadcasts.Any());
			var searchRequestData = server.SentBroadcasts.First();

			var parser = new HttpRequestParser();
			var searchRequest = parser.Parse(System.Text.UTF8Encoding.UTF8.GetString(searchRequestData.Buffer));

			Assert.AreEqual("ssdp:all", GetFirstHeaderValue(searchRequest, "ST"));
			Assert.AreEqual("3", GetFirstHeaderValue(searchRequest, "MX"));
		}

		[TestMethod]
		public void DeviceLocator_SearchAsync_SearchesForSpecifiedTarget()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			var searchTarget = "uuid:" + Guid.NewGuid().ToString();
			var t = deviceLocator.SearchAsync(searchTarget);
			t.Wait();

			Assert.IsTrue(server.SentBroadcasts.Any());
			var searchRequestData = server.SentBroadcasts.First();

			var parser = new HttpRequestParser();
			var searchRequest = parser.Parse(System.Text.UTF8Encoding.UTF8.GetString(searchRequestData.Buffer));

			Assert.AreEqual(searchTarget, GetFirstHeaderValue(searchRequest, "ST"));
			Assert.AreEqual("3", GetFirstHeaderValue(searchRequest, "MX"));
		}

		[TestMethod]
		public void DeviceLocator_SearchAsync_UsesSpecifiedSearchTimeLess1Second()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			var searchTime = TimeSpan.FromSeconds(2);
			var t = deviceLocator.SearchAsync(searchTime);
			t.Wait();

			Assert.IsTrue(server.SentBroadcasts.Any());
			var searchRequestData = server.SentBroadcasts.First();

			var parser = new HttpRequestParser();
			var searchRequest = parser.Parse(System.Text.UTF8Encoding.UTF8.GetString(searchRequestData.Buffer));

			Assert.AreEqual("ssdp:all", GetFirstHeaderValue(searchRequest, "ST"));
			Assert.AreEqual((searchTime.TotalSeconds - 1).ToString(), GetFirstHeaderValue(searchRequest, "MX"));
		}

		[ExpectedException(typeof(System.ArgumentException))]
		[TestMethod]
		public void DeviceLocator_SearchAsync_ThrowsIfSearchTime1Second()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			var searchTime = TimeSpan.FromSeconds(1);
			var t = deviceLocator.SearchAsync(searchTime);
			t.GetAwaiter().GetResult();
		}

		[ExpectedException(typeof(System.ArgumentException))]
		[TestMethod]
		public void DeviceLocator_SearchAsync_ThrowsIfSearchTimeLessThan1Second()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			var searchTime = TimeSpan.FromMilliseconds(500);
			var t = deviceLocator.SearchAsync(searchTime);
			t.GetAwaiter().GetResult();
		}

		[ExpectedException(typeof(System.ArgumentException))]
		[TestMethod]
		public void DeviceLocator_SearchAsync_ThrowsIfSearchTimeNegative()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			var searchTime = TimeSpan.FromSeconds(-5);
			var t = deviceLocator.SearchAsync(searchTime);
			t.GetAwaiter().GetResult();
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void DeviceLocator_SearchAsync_ThrowsIfSearchTargetNull()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			string searchTarget = null;
			var t = deviceLocator.SearchAsync(searchTarget);
			t.GetAwaiter().GetResult();
		}

		[ExpectedException(typeof(System.ArgumentException))]
		[TestMethod]
		public void DeviceLocator_SearchAsync_ThrowsIfSearchTargetEmpty()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			string searchTarget = String.Empty;
			var t = deviceLocator.SearchAsync(searchTarget);
			t.GetAwaiter().GetResult();
		}

		[TestMethod]
		public void DeviceLocator_SearchAsync_AllowsZeroSearchTime()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			string searchTarget = "ssdp:all";
			var t = deviceLocator.SearchAsync(searchTarget, TimeSpan.Zero);
			t.GetAwaiter().GetResult();
			server.Dispose();
			deviceLocator.Dispose();
		}

		[ExpectedException(typeof(System.InvalidOperationException))]
		[TestMethod]
		public void DeviceLocator_SearchAsync_ThrowsOnDuplicateConcurrentSearch()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			string searchTarget = "ssdp:all";
			var t = deviceLocator.SearchAsync(searchTarget, TimeSpan.FromSeconds(10));
			var t2 = deviceLocator.SearchAsync(searchTarget, TimeSpan.FromSeconds(1.5));
			t2.GetAwaiter().GetResult();
		}

		[TestMethod]
		public void DeviceLocator_SearchAsync_ReturnsCachedDevices()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			DiscoveredSsdpDevice device = null;
			bool newlyDiscovered = false;
			var receivedNotification = false;

			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceAvailable += (sender, args) =>
				{
					device = args.DiscoveredDevice;
					newlyDiscovered = args.IsNewlyDiscovered;
					receivedNotification = true;
					eventSignal.Set();
				};
				deviceLocator.StartListeningForNotifications();

				server.MockReceiveBroadcast(GetMockAliveNotification());
				eventSignal.WaitOne(10000);
				Assert.IsTrue(receivedNotification);

				var results = deviceLocator.SearchAsync(TimeSpan.Zero).GetAwaiter().GetResult();
				Assert.IsNotNull(results);
				Assert.IsTrue(results.Any());
				Assert.IsTrue(results.First().Usn == device.Usn);
			}
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_HandlesByeByeDuringSearch()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			DiscoveredSsdpDevice device = null;
			var receivedNotification = false;

			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceUnavailable += (sender, args) =>
				{
					device = args.DiscoveredDevice;
					receivedNotification = true;
					eventSignal.Set();
				};
				deviceLocator.StartListeningForNotifications();
				server.MockReceiveBroadcast(GetMockAliveNotification());
				server.WaitForMessageToProcess(10000);

				var t = deviceLocator.SearchAsync(TimeSpan.FromSeconds(3));
				System.Threading.Thread.Sleep(500);
				server.MockReceiveBroadcast(GetMockByeByeNotification());
				eventSignal.WaitOne(10000);
				var results = t.GetAwaiter().GetResult();

				Assert.IsNotNull(results);
				Assert.IsFalse(results.Any());
			}

			Assert.IsTrue(receivedNotification);
			Assert.IsNotNull(device);
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_ProcessesSearchResponse()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			var publishedDevice = CreateDeviceTree();
			DiscoveredSsdpDevice device = null;
			var receivedNotification = false;

			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceAvailable += (sender, args) =>
				{
					device = args.DiscoveredDevice;
					receivedNotification = true;
					eventSignal.Set();
				};

				var t = deviceLocator.SearchAsync(TimeSpan.FromSeconds(3));
				System.Threading.Thread.Sleep(500);
				server.MockReceiveMessage(GetMockSearchResponse(publishedDevice, publishedDevice.Udn));
				eventSignal.WaitOne(10000);
				var results = t.GetAwaiter().GetResult();

				Assert.IsNotNull(results);
				Assert.IsTrue(results.Any());
				Assert.IsTrue(receivedNotification);
				Assert.IsNotNull(device);
				Assert.AreEqual(device.Usn, String.Format("{0}:{1}", publishedDevice.Udn, publishedDevice.FullDeviceType));
				Assert.AreEqual(device.NotificationType, publishedDevice.Udn);
			}
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_RetrievesCustomHeader()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			var publishedDevice = CreateDeviceTree(new CustomHttpHeader("machinename", Environment.MachineName));
			DiscoveredSsdpDevice device = null;
			var receivedNotification = false;

			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceAvailable += (sender, args) =>
				{
					device = args.DiscoveredDevice;
					receivedNotification = true;
					eventSignal.Set();
				};

				var t = deviceLocator.SearchAsync(TimeSpan.FromSeconds(3));
				System.Threading.Thread.Sleep(500);
				server.MockReceiveMessage(GetMockSearchResponse(publishedDevice, publishedDevice.Udn));
				eventSignal.WaitOne(10000);
				var results = t.GetAwaiter().GetResult();

				Assert.IsNotNull(results);
				Assert.IsTrue(results.Any());
				Assert.IsTrue(receivedNotification);
				Assert.IsNotNull(device);

				foreach (var h1 in results.First().ResponseHeaders)
				{
					System.Diagnostics.Debug.WriteLine(h1.Key);
				}
				Assert.AreEqual(Environment.MachineName, (from h in results.First().ResponseHeaders where h.Key == "machinename" select h.Value.FirstOrDefault()).FirstOrDefault());
				Assert.AreEqual(device.Usn, String.Format("{0}:{1}", publishedDevice.Udn, publishedDevice.FullDeviceType));
				Assert.AreEqual(device.NotificationType, publishedDevice.Udn);
			}
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_SearchResponseMissingCacheHeaderIsNonCacheable()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			var publishedDevice = CreateDeviceTree();
			DiscoveredSsdpDevice device = null;
			var receivedNotification = false;

			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceAvailable += (sender, args) =>
				{
					device = args.DiscoveredDevice;
					receivedNotification = true;
					eventSignal.Set();
				};

				var t = deviceLocator.SearchAsync(TimeSpan.FromSeconds(3));
				System.Threading.Thread.Sleep(500);
				server.MockReceiveMessage(GetMockSearchResponseWithCustomCacheHeader(publishedDevice, publishedDevice.Udn, null));
				eventSignal.WaitOne(10000);
				var results = t.GetAwaiter().GetResult();

				Assert.IsNotNull(results);
				Assert.IsTrue(results.Any());
				Assert.IsTrue(receivedNotification);
				Assert.IsNotNull(device);
				Assert.AreEqual(device.Usn, String.Format("{0}:{1}", publishedDevice.Udn, publishedDevice.FullDeviceType));
				Assert.AreEqual(device.NotificationType, publishedDevice.Udn);
			}
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_SearchResponseUsesSharedMaxAge()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			var publishedDevice = CreateDeviceTree();
			publishedDevice.CacheLifetime = TimeSpan.FromMinutes(30);

			DiscoveredSsdpDevice device = null;

			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceAvailable += (sender, args) =>
				{
					device = args.DiscoveredDevice;
					eventSignal.Set();
				};

				var t = deviceLocator.SearchAsync(TimeSpan.FromSeconds(3));
				System.Threading.Thread.Sleep(500);
				server.MockReceiveMessage(GetMockSearchResponseWithCustomCacheHeader(publishedDevice, publishedDevice.Udn, String.Format("CACHE-CONTROL: public, s-maxage={0}", publishedDevice.CacheLifetime.TotalSeconds)));
				eventSignal.WaitOne(10000);
				var results = t.GetAwaiter().GetResult();

				Assert.IsNotNull(results);
				Assert.IsTrue(results.Any());
				Assert.AreEqual(device.CacheLifetime, results.First().CacheLifetime);
			}
		}

		[TestMethod()]
		public void DeviceLocator_Notifications_SearchResponseDefaultsToZeroMaxAge()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			var publishedDevice = CreateDeviceTree();
			publishedDevice.CacheLifetime = TimeSpan.FromMinutes(30);

			DiscoveredSsdpDevice device = null;

			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceAvailable += (sender, args) =>
				{
					device = args.DiscoveredDevice;
					eventSignal.Set();
				};

				var t = deviceLocator.SearchAsync(TimeSpan.FromSeconds(3));
				System.Threading.Thread.Sleep(500);
				server.MockReceiveMessage(GetMockSearchResponseWithCustomCacheHeader(publishedDevice, publishedDevice.Udn, String.Format("CACHE-CONTROL: public")));
				eventSignal.WaitOne(10000);
				var results = t.GetAwaiter().GetResult();

				Assert.IsNotNull(results);
				Assert.IsTrue(results.Any());
				Assert.AreEqual(TimeSpan.Zero, results.First().CacheLifetime);
			}
		}

		[TestMethod]
		public void DeviceLocator_SearchAsync_RaisesDeviceAvailableOnResponse()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			var publishedDevice = CreateDeviceTree();
			DiscoveredSsdpDevice device = null;
			bool newlyDiscovered = false;
			var receivedNotification = false;

			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceAvailable += (sender, args) =>
				{
					device = args.DiscoveredDevice;
					newlyDiscovered = args.IsNewlyDiscovered;
					receivedNotification = true;
					eventSignal.Set();
				};


				var task = deviceLocator.SearchAsync(TimeSpan.FromSeconds(2));
				server.MockReceiveMessage(GetMockSearchResponse(publishedDevice, publishedDevice.Udn));
				eventSignal.WaitOne(10000);
				Assert.IsTrue(receivedNotification);
				var results = task.GetAwaiter().GetResult();

				Assert.IsNotNull(results);
				Assert.IsTrue(results.Any());
				Assert.IsTrue(results.First().Usn == device.Usn);
			}
		}

		[TestMethod]
		public void DeviceLocator_SearchAsync_FiltersNotificationsDuringSearch()
		{
			var server = new MockCommsServer();
			var deviceLocator = new MockDeviceLocator(server);

			var publishedDevice = CreateDeviceTree();
			var publishedDevice2 = CreateDeviceTree();

			deviceLocator.NotificationFilter = publishedDevice.Udn;
			deviceLocator.StartListeningForNotifications();

			DiscoveredSsdpDevice device = null;
			bool newlyDiscovered = false;
			var receivedNotification = false;

			using (var eventSignal = new System.Threading.AutoResetEvent(false))
			{
				deviceLocator.DeviceAvailable += (sender, args) =>
				{
					device = args.DiscoveredDevice;

					newlyDiscovered = args.IsNewlyDiscovered;
					receivedNotification = true;
					eventSignal.Set();
				};


				var task = deviceLocator.SearchAsync(publishedDevice.Udn);
				server.MockReceiveBroadcast(GetMockAliveNotification(publishedDevice2));
				server.WaitForMessageToProcess(5000);
				eventSignal.Reset();
				server.MockReceiveBroadcast(GetMockAliveNotification(publishedDevice));
				server.WaitForMessageToProcess(5000);
				eventSignal.WaitOne(10000);
				Assert.IsTrue(receivedNotification);
				var results = task.GetAwaiter().GetResult();

				Assert.IsNotNull(results);
				Assert.AreEqual(1, results.Count());
				Assert.IsTrue(results.First().Usn == device.Usn);
			}
		}

		#region IsSearching Tests

		[TestMethod()]
		public void DeviceLocator_IsSearching_ReturnsFalseWhenNoSearchInProgress()
		{
			var deviceLocator = new MockDeviceLocator();
			Assert.IsFalse(deviceLocator.IsSearching);
			var task = deviceLocator.SearchAsync(TimeSpan.FromSeconds(1.5));
			task.Wait();

			Assert.IsFalse(deviceLocator.IsSearching);
		}

		[TestMethod()]
		public void DeviceLocator_IsSearching_ReturnsTrueWhenSearchInProgress()
		{
			var deviceLocator = new MockDeviceLocator();
			Assert.IsFalse(deviceLocator.IsSearching);
			var task = deviceLocator.SearchAsync(TimeSpan.FromSeconds(1.5));
			Assert.IsTrue(deviceLocator.IsSearching);

			task.Wait();
		}

		#endregion

		#endregion

		#region Event Arguments Tests

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void DeviceAvailableEventArgs_Constructor_ThrowsOnNullDevice()
		{
			var args = new DeviceAvailableEventArgs(null, true);
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void DeviceUnavailableEventArgs_Constructor_ThrowsOnNullDevice()
		{
			var args = new DeviceUnavailableEventArgs(null, false);
		}

		#endregion

		#region Private Methods

		private ReceivedUdpData GetMockSearchResponse(SsdpDevice device, string stHeader)
		{
			return GetMockSearchResponseWithCustomCacheHeader(device, stHeader, String.Format("CACHE-CONTROL: public, max-age={0}", device.ToRootDevice().CacheLifetime.TotalSeconds));
		}

		private ReceivedUdpData GetMockSearchResponseWithCustomCacheHeader(SsdpDevice device, string stHeader, string cacheHeader)
		{
			string otherHeaders = AdditionalHeaders(device);
			var responseText = String.Format(@"HTTP/1.1 200 OK
EXT:
DATE: {4}{0}
ST:{1}
SERVER: TestOS/1.0 UPnP/1.0 RSSDP/1.0
USN:{2}
LOCATION:{3}{5}

", //Blank line at end important, do not remove.
 String.IsNullOrEmpty(cacheHeader) ? String.Empty : Environment.NewLine + cacheHeader,
 stHeader,
 String.Format("{0}:{1}", device.Udn, device.FullDeviceType),
 device.ToRootDevice().Location,
 DateTime.UtcNow.ToString("r"),
 otherHeaders
 );

			var retVal = new ReceivedUdpData()
			{
				Buffer = System.Text.ASCIIEncoding.UTF8.GetBytes(responseText),
				ReceivedFrom = new UdpEndPoint() { IPAddress = SsdpConstants.MulticastLocalAdminAddress, Port = SsdpConstants.MulticastPort }
			};
			retVal.ReceivedBytes = retVal.Buffer.Length;

			return retVal;
		}

		private object GetFirstHeaderValue(System.Net.Http.HttpRequestMessage request, string headerName)
		{
			IEnumerable<string> values = null;
			if (request.Headers.TryGetValues(headerName, out values))
				return values.First();

			return null;
		}

		private ReceivedUdpData GetMockAliveNotification()
		{
			var data = String.Format(@"NOTIFY * HTTP/1.1
HOST: 239.255.255.250:1900
Date: {0}
NT: uuid: 1234
NTS: ssdp:alive
SERVER: TestOS/1.0 UPnP/1.0 RSSDP/1.0
USN: uuid:1234::test-schema:device:TestDeviceType:1
LOCATION: http://192.168.1.100:1701/devicedescription.xml
CACHE-CONTROL: public, max-age=1800

",
	 DateTime.UtcNow.ToString("r")
	 );

			var retVal = new ReceivedUdpData()
			{
				Buffer = System.Text.UTF8Encoding.UTF8.GetBytes(data),
				ReceivedFrom = new UdpEndPoint()
				{
					IPAddress = SsdpConstants.MulticastLocalAdminAddress,
					Port = 1900
				}
			};
			retVal.ReceivedBytes = retVal.Buffer.Length;

			return retVal;
		}

		private ReceivedUdpData GetMockByeByeNotification()
		{
			var data = String.Format(@"NOTIFY * HTTP/1.1
HOST: 239.255.255.250:1900
DATE: {0}
NT: uuid: 1234
NTS: ssdp:byebye
SERVER: TestOS/1.0 UPnP/1.0 RSSDP/1.0
USN: uuid:1234::test-schema:device:TestDeviceType:1

",
	 DateTime.UtcNow.ToString("r")
	 );

			var retVal = new ReceivedUdpData()
			{
				Buffer = System.Text.UTF8Encoding.UTF8.GetBytes(data),
				ReceivedFrom = new UdpEndPoint()
				{
					IPAddress = SsdpConstants.MulticastLocalAdminAddress,
					Port = 1900
				}
			};
			retVal.ReceivedBytes = retVal.Buffer.Length;

			return retVal;
		}

		private ReceivedUdpData GetMockByeByeNotification(SsdpDevice device)
		{
			var data = String.Format(@"NOTIFY * HTTP/1.1
HOST: 239.255.255.250:1900
DATE: {0}
NT: {1}
NTS: ssdp:byebye
SERVER: TestOS/1.0 UPnP/1.0 RSSDP/1.0
USN: {2}

",
	DateTime.UtcNow.ToString("r"),
	device.Udn,
	String.Format("{0}::{1}", device.Udn, device.FullDeviceType)
	 );

			var retVal = new ReceivedUdpData()
			{
				Buffer = System.Text.UTF8Encoding.UTF8.GetBytes(data),
				ReceivedFrom = new UdpEndPoint()
				{
					IPAddress = SsdpConstants.MulticastLocalAdminAddress,
					Port = 1900
				}
			};
			retVal.ReceivedBytes = retVal.Buffer.Length;

			return retVal;
		}

		private ReceivedUdpData GetMockNonNotifyRequest()
		{
			var data = String.Format(@"POST * HTTP/1.1
HOST: 239.255.255.250:1900
Date: {0}
NT: uuid: 1234
NTS: ssdp:alive
SERVER: TestOS/1.0 UPnP/1.0 RSSDP/1.0
USN: uuid:1234::test-schema:device:TestDeviceType:1
LOCATION: http://192.168.1.100:1701/devicedescription.xml
CACHE-CONTROL: public, max-age=1800

",
	 DateTime.UtcNow.ToString("r")
	 );

			var retVal = new ReceivedUdpData()
			{
				Buffer = System.Text.UTF8Encoding.UTF8.GetBytes(data),
				ReceivedFrom = new UdpEndPoint()
				{
					IPAddress = SsdpConstants.MulticastLocalAdminAddress,
					Port = 1900
				}
			};
			retVal.ReceivedBytes = retVal.Buffer.Length;

			return retVal;
		}

		private ReceivedUdpData GetMockAliveNotification(SsdpDevice device)
		{
			var rootDevice = device.ToRootDevice();

			var data = String.Format(@"NOTIFY * HTTP/1.1
HOST: 239.255.255.250:1900
Date: {0}
NT: {1}
NTS: ssdp:alive
SERVER: TestOS/1.0 UPnP/1.0 RSSDP/1.0
USN: {2}
LOCATION: {3} 
CACHE-CONTROL: public, max-age={4}

",
		DateTime.UtcNow.ToString("r"),
		device.Udn,
		String.Format("{0}::{1}", device.Udn, device.FullDeviceType),
		rootDevice.Location,
		rootDevice.CacheLifetime.TotalSeconds
	 );

			var retVal = new ReceivedUdpData()
			{
				Buffer = System.Text.UTF8Encoding.UTF8.GetBytes(data),
				ReceivedFrom = new UdpEndPoint()
				{
					IPAddress = SsdpConstants.MulticastLocalAdminAddress,
					Port = 1900
				}
			};
			retVal.ReceivedBytes = retVal.Buffer.Length;

			return retVal;
		}

		private SsdpRootDevice CreateDeviceTree(CustomHttpHeader testHeader = null)
		{
			var retVal = CreateValidRootDevice();
			if (testHeader != null)
			{
				retVal.CustomResponseHeaders.Add(testHeader);
			}
			retVal.AddDevice(CreateValidEmbeddedDevice(retVal));
			retVal.Devices.First().AddDevice(CreateValidEmbeddedDevice(retVal));
			return retVal;
		}

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
				Location = new Uri("http://testdevice:1700/xml"),
				CacheLifetime = TimeSpan.FromMinutes(30)
			};
			return rootDevice;
		}

		private string AdditionalHeaders(SsdpDevice device)
		{
			if (device.CustomResponseHeaders.Count == 0) return String.Empty;

			StringBuilder returnValue = new StringBuilder();
			foreach (var header in device.CustomResponseHeaders)
			{
				returnValue.Append("\r\n");

				returnValue.Append(header.ToString());
			}
			return returnValue.ToString();
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

		#endregion

	}
}