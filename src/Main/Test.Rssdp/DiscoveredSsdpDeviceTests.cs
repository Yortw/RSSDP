using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp;

namespace TestRssdp
{
	[TestClass]
	public class DiscoveredSsdpDeviceTests
	{

		#region IsExpired Tests

		[TestMethod]
		public void DiscoveredDevice_IsExpired_DoesNotImmediatelyReportTrue()
		{
			using (var requestMessage = new HttpRequestMessage())
			{
				var discoveredDevice = new DiscoveredSsdpDevice("upnp:rootdevice", requestMessage.Headers)
				{
					AsAt = DateTimeOffset.Now,
					CacheLifetime = TimeSpan.FromSeconds(1)
				};

				Assert.IsFalse(discoveredDevice.IsExpired());
			}
		}

		[TestMethod]
		public void DiscoveredDevice_IsExpired_ImmediatelyReportsTrueIfCacheLifetimeIsZero()
		{
			using (var requestMessage = new HttpRequestMessage())
			{
				var discoveredDevice = new DiscoveredSsdpDevice("upnp:rootdevice", requestMessage.Headers)
				{
					AsAt = DateTimeOffset.Now,
					CacheLifetime = TimeSpan.Zero
				};

				Assert.IsTrue(discoveredDevice.IsExpired());
			}
		}

		[TestMethod]
		public void DiscoveredDevice_IsExpired_ReportsTrueAfterCacheLifetimeExpires()
		{
			using (var requestMessage = new HttpRequestMessage())
			{
				var discoveredDevice = new DiscoveredSsdpDevice("upnp:rootdevice", requestMessage.Headers)
				{
					AsAt = DateTimeOffset.Now,
					CacheLifetime = TimeSpan.FromMilliseconds(100)
				};
				System.Threading.Thread.Sleep(500);

				Assert.IsTrue(discoveredDevice.IsExpired());
			}
		}

		#endregion

		#region GetDeviceInfo Tests

		[TestMethod]
		public void DiscoveredDevice_GetDeviceInfo_MakesHttpRequest()
		{
			var publishedDevice = new SsdpRootDevice()
			{
				Location = new Uri("http://192.168.1.100:1702/description"),
				CacheLifetime = TimeSpan.FromMinutes(1),
				DeviceType = "TestDeviceType",
				Uuid = System.Guid.NewGuid().ToString()
			};

			using (var requestMessage = new HttpRequestMessage())
			{
				var discoveredDevice = new DiscoveredSsdpDevice("upnp:rootdevice", requestMessage.Headers)
				{
					Usn = "test usn",
					AsAt = DateTimeOffset.Now,
					CacheLifetime = TimeSpan.FromSeconds(1),
					DescriptionLocation = publishedDevice.Location
				};

				var client = new MockHttpClient(publishedDevice.ToDescriptionDocument());
				var device = discoveredDevice.GetDeviceInfo(client).GetAwaiter().GetResult();

				Assert.IsNotNull(client.LastRequest);
				Assert.AreEqual(device.Uuid, publishedDevice.Uuid);
				Assert.AreEqual(device.DeviceType, publishedDevice.DeviceType);
			}
		}

		[TestMethod]
		public void DiscoveredDevice_GetDeviceInfo_DoesNotMakeHttpRequestIfDataCached()
		{
			var publishedDevice = new SsdpRootDevice()
			{
				Location = new Uri("http://192.168.1.100:1702/description"),
				CacheLifetime = TimeSpan.FromMinutes(1),
				DeviceType = "TestDeviceType",
				Uuid = System.Guid.NewGuid().ToString()
			};

			using (var requestMessage = new HttpRequestMessage())
			{
				var discoveredDevice = new DiscoveredSsdpDevice("upnp:rootdevice", requestMessage.Headers)
				{
					Usn = "test usn",
					AsAt = DateTimeOffset.Now,
					CacheLifetime = publishedDevice.CacheLifetime,
					DescriptionLocation = publishedDevice.Location
				};

				var client = new MockHttpClient(publishedDevice.ToDescriptionDocument());
				_ = discoveredDevice.GetDeviceInfo(client).GetAwaiter().GetResult();

				client = new MockHttpClient(publishedDevice.ToDescriptionDocument());
				var device = discoveredDevice.GetDeviceInfo(client).GetAwaiter().GetResult();

				Assert.IsNull(client.LastRequest);
				Assert.AreEqual(device.Uuid, publishedDevice.Uuid);
				Assert.AreEqual(device.DeviceType, publishedDevice.DeviceType);
			}
		}

		[TestMethod]
		public void DiscoveredDevice_GetDeviceInfo_MakesRequestWhenCachedDataExpired()
		{
			var publishedDevice = new SsdpRootDevice()
			{
				Location = new Uri("http://192.168.1.100:1702/description"),
				CacheLifetime = TimeSpan.FromSeconds(2),
				DeviceType = "TestDeviceType",
				Uuid = System.Guid.NewGuid().ToString()
			};

			using (var requestMessage = new HttpRequestMessage())
			{
				var discoveredDevice = new DiscoveredSsdpDevice("upnp:rootdevice", requestMessage.Headers)
				{
					Usn = "test usn",
					AsAt = DateTimeOffset.Now,
					CacheLifetime = publishedDevice.CacheLifetime,
					DescriptionLocation = publishedDevice.Location
				};

				var client = new MockHttpClient(publishedDevice.ToDescriptionDocument());
				_ = discoveredDevice.GetDeviceInfo(client).GetAwaiter().GetResult();

				System.Threading.Thread.Sleep(3000);
				client = new MockHttpClient(publishedDevice.ToDescriptionDocument());
				var device = discoveredDevice.GetDeviceInfo(client).GetAwaiter().GetResult();

				Assert.IsNotNull(client.LastRequest);
				Assert.AreEqual(device.Uuid, publishedDevice.Uuid);
				Assert.AreEqual(device.DeviceType, publishedDevice.DeviceType);
			}
		}

		[TestMethod]
		public void DiscoveredDevice_GetDeviceInfo_CreatesDefaultClient()
		{
			var publishedDevice = new SsdpRootDevice()
			{
				Location = new Uri("http://192.168.1.100:1702/description"),
				CacheLifetime = TimeSpan.FromMinutes(1),
				DeviceType = "TestDeviceType",
				Uuid = System.Guid.NewGuid().ToString()
			};

			using (var requestMessage = new HttpRequestMessage())
			{
				var discoveredDevice = new DiscoveredSsdpDevice("upnp:rootdevice", requestMessage.Headers)
				{
					Usn = "test usn",
					AsAt = DateTimeOffset.Now,
					CacheLifetime = TimeSpan.FromSeconds(1),
					DescriptionLocation = publishedDevice.Location
				};

				Assert.Throws<HttpRequestException>(() =>
				{
					_ = discoveredDevice.GetDeviceInfo().GetAwaiter().GetResult();
				});	
			}
		}

		#endregion

		[TestMethod]
		public void DiscoveredDevice_ToStringReturnsUsn()
		{
			using (var requestMessage = new HttpRequestMessage())
			{
				var discoveredDevice = new DiscoveredSsdpDevice("upnp:rootdevice", requestMessage.Headers)
				{
					Usn = "test usn",
					AsAt = DateTimeOffset.Now,
					CacheLifetime = TimeSpan.FromSeconds(1)
				};
				System.Threading.Thread.Sleep(1000);

				Assert.AreEqual(discoveredDevice.Usn, discoveredDevice.ToString());
			}
		}

		private class MockHttpClient : HttpClient
		{
			private readonly MockHttpHandler _InnerHandler;

			public MockHttpClient(string responseData) : this(new MockHttpHandler(responseData))
			{
			}

			public MockHttpClient(MockHttpHandler handler)
				: base(handler)
			{
				_InnerHandler = handler;
			}

			public HttpRequestMessage LastRequest
			{
				get { return _InnerHandler.LastRequest; }
			}
		}

		private class MockHttpHandler : HttpMessageHandler
		{

			private HttpRequestMessage _LastRequest;
			private readonly string _ResponseData;

			public MockHttpHandler(string responseData)
			{
				_ResponseData = responseData;
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
			{
				_LastRequest = request;

				HttpResponseMessage result;
				if (request.RequestUri.ToString() == "http://192.168.1.100:1702/description")
				{
					result = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
					{
						RequestMessage = request,
						Content = new ByteArrayContent(System.Text.UTF8Encoding.UTF8.GetBytes(_ResponseData))
					};
				}
				else
				{
					result = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound) { RequestMessage = request };
				}

				var tcs = new TaskCompletionSource<HttpResponseMessage>();
				tcs.TrySetResult(result);
				return tcs.Task;
			}

			public HttpRequestMessage LastRequest
			{
				get { return _LastRequest; }
			}
		}


	}
}