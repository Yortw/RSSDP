using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp;

namespace Test.RssdpPortable
{
	[TestClass]
	public class DiscoveredSsdpDeviceTests
	{

		#region IsExpired Tests

		[TestMethod]
		public void DiscoveredDevice_IsExpired_DoesNotImmediatelyReportTrue()
		{
			var discoveredDevice = new DiscoveredSsdpDevice();
			discoveredDevice.AsAt = DateTimeOffset.Now;
			discoveredDevice.CacheLifetime = TimeSpan.FromSeconds(1);

			Assert.IsFalse(discoveredDevice.IsExpired());
		}

		[TestMethod]
		public void DiscoveredDevice_IsExpired_ImmediatelyReportsTrueIfCacheLifetimeIsZero()
		{
			var discoveredDevice = new DiscoveredSsdpDevice();
			discoveredDevice.AsAt = DateTimeOffset.Now;
			discoveredDevice.CacheLifetime = TimeSpan.Zero;

			Assert.IsTrue(discoveredDevice.IsExpired());
		}

		[TestMethod]
		public void DiscoveredDevice_IsExpired_ReportsTrueAfterCacheLifetimeExpires()
		{
			var discoveredDevice = new DiscoveredSsdpDevice();
			discoveredDevice.AsAt = DateTimeOffset.Now;
			discoveredDevice.CacheLifetime = TimeSpan.FromMilliseconds(100);
			System.Threading.Thread.Sleep(500);

			Assert.IsTrue(discoveredDevice.IsExpired());
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

			var discoveredDevice = new DiscoveredSsdpDevice();
			discoveredDevice.Usn = "test usn";
			discoveredDevice.AsAt = DateTimeOffset.Now;
			discoveredDevice.CacheLifetime = TimeSpan.FromSeconds(1);
			discoveredDevice.DescriptionLocation = publishedDevice.Location;

			var client = new MockHttpClient(publishedDevice.ToDescriptionDocument());
			var device = discoveredDevice.GetDeviceInfo(client).GetAwaiter().GetResult();

			Assert.IsNotNull(client.LastRequest);
			Assert.AreEqual(device.Uuid, publishedDevice.Uuid);
			Assert.AreEqual(device.DeviceType, publishedDevice.DeviceType);
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

			var discoveredDevice = new DiscoveredSsdpDevice();
			discoveredDevice.Usn = "test usn";
			discoveredDevice.AsAt = DateTimeOffset.Now;
			discoveredDevice.CacheLifetime = publishedDevice.CacheLifetime;
			discoveredDevice.DescriptionLocation = publishedDevice.Location;

			var client = new MockHttpClient(publishedDevice.ToDescriptionDocument());
			var device = discoveredDevice.GetDeviceInfo(client).GetAwaiter().GetResult();

			client = new MockHttpClient(publishedDevice.ToDescriptionDocument());
			device = discoveredDevice.GetDeviceInfo(client).GetAwaiter().GetResult();

			Assert.IsNull(client.LastRequest);
			Assert.AreEqual(device.Uuid, publishedDevice.Uuid);
			Assert.AreEqual(device.DeviceType, publishedDevice.DeviceType);
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

			var discoveredDevice = new DiscoveredSsdpDevice();
			discoveredDevice.Usn = "test usn";
			discoveredDevice.AsAt = DateTimeOffset.Now;
			discoveredDevice.CacheLifetime = publishedDevice.CacheLifetime;
			discoveredDevice.DescriptionLocation = publishedDevice.Location;

			var client = new MockHttpClient(publishedDevice.ToDescriptionDocument());
			var device = discoveredDevice.GetDeviceInfo(client).GetAwaiter().GetResult();

			System.Threading.Thread.Sleep(3000);
			client = new MockHttpClient(publishedDevice.ToDescriptionDocument());
			device = discoveredDevice.GetDeviceInfo(client).GetAwaiter().GetResult();

			Assert.IsNotNull(client.LastRequest);
			Assert.AreEqual(device.Uuid, publishedDevice.Uuid);
			Assert.AreEqual(device.DeviceType, publishedDevice.DeviceType);
		}

		[ExpectedException(typeof(HttpRequestException))]
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

			var discoveredDevice = new DiscoveredSsdpDevice();
			discoveredDevice.Usn = "test usn";
			discoveredDevice.AsAt = DateTimeOffset.Now;
			discoveredDevice.CacheLifetime = TimeSpan.FromSeconds(1);
			discoveredDevice.DescriptionLocation = publishedDevice.Location;

			var device = discoveredDevice.GetDeviceInfo().GetAwaiter().GetResult();
		}

		#endregion

		[TestMethod]
		public void DiscoveredDevice_ToStringReturnsUsn()
		{
			var discoveredDevice = new DiscoveredSsdpDevice();
			discoveredDevice.Usn = "test usn";
			discoveredDevice.AsAt = DateTimeOffset.Now;
			discoveredDevice.CacheLifetime = TimeSpan.FromSeconds(1);
			System.Threading.Thread.Sleep(1000);

			Assert.AreEqual(discoveredDevice.Usn, discoveredDevice.ToString());
		}

		private class MockHttpClient : HttpClient
		{
			private MockHttpHandler _InnerHandler;

			public MockHttpClient(string responseData) : this(responseData, new MockHttpHandler(responseData))
			{
			}

			public MockHttpClient(string responseData, MockHttpHandler handler)
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
			private string _ResponseData;

			public MockHttpHandler(string responseData)
			{
				_ResponseData = responseData;
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
			{
				_LastRequest = request;

				HttpResponseMessage result = null;
				if (request.RequestUri.ToString() == "http://192.168.1.100:1702/description")
				{
					result = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
					{
						RequestMessage = request,
						Content = new ByteArrayContent(System.Text.UTF8Encoding.UTF8.GetBytes(_ResponseData))
					};
				}
				else
					result = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound) { RequestMessage = request };

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