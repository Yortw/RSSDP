using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp.Infrastructure;
using Rssdp;

namespace Test.RssdpPortable
{
	[TestClass, System.Runtime.InteropServices.GuidAttribute("A330D44B-B20B-4E18-8B2F-86BD77A77549")]
	public class CommServerTests
	{

		#region Constructor Tests

		[TestMethod]
		public void CommsServer_FullConstructor_CompleteSuccessfullyWithValidArguments()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory, 1701, 4);
		}

		[TestMethod]
		public void CommsServer_PartialConstructor_CompleteSuccessfullyWithValidArguments()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory, 1701);
		}

		[TestMethod]
		public void CommsServer_FullConstructor_SpecifiedPortUsedForUnicastSocket()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory, 1702, 4);
			server.SendMessage(System.Text.UTF8Encoding.UTF8.GetBytes("Hello!"), new UdpEndPoint() { IPAddress = "192.168.1.100", Port = 1900 });
			var mockSocket = socketFactory.UnicastSocket as MockSocket;

			Assert.AreEqual(1702, mockSocket.LocalPort);
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void CommsServer_MinimumConstructor_NullSocketFactoryThrowsExeption()
		{
			var server = new SsdpCommunicationsServer(null);
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void CommsServer_FactoryAndSocketConstructor_NullSocketFactoryThrowsExeption()
		{
			var server = new SsdpCommunicationsServer(null, 1701);
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void CommsServer_FullConstructor_NullSocketFactoryThrowsExeption()
		{
			var server = new SsdpCommunicationsServer(null, 1701, 4);
		}

		[ExpectedException(typeof(System.ArgumentOutOfRangeException))]
		[TestMethod]
		public void CommsServer_FullConstructor_NegativeMulticastTtlThrowsException()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory, 1701, -1);
		}

		[ExpectedException(typeof(System.ArgumentOutOfRangeException))]
		[TestMethod]
		public void CommsServer_FullConstructor_ZeroMulticastTtlThrowsException()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory, 1701, 0);
		}

		[TestMethod]
		public void Ctor_WhenIpAddressIsV6_DeviceNetworkTypeEqualsV61()
		{
			var socketFactory = new SocketFactory(IPAddress.IPv6Any.ToString());
			var communicationServer = new SsdpCommunicationsServer(socketFactory);

			Assert.AreEqual(DeviceNetworkType.Ipv6, communicationServer.DeviceNetworkType);
		}

		[TestMethod]
		public void Ctor_WhenIpAddressIsV6_DeviceNetworkTypeEqualsV62()
		{
			var socketFactory = new SocketFactory("::1");
			var communicationServer = new SsdpCommunicationsServer(socketFactory);

			Assert.AreEqual(DeviceNetworkType.Ipv6, communicationServer.DeviceNetworkType);
		}

		[TestMethod]
		public void Ctor_WhenIpAddressIsV4_DeviceNetworkTypeEqualsV41()
		{
			var socketFactory = new SocketFactory(IPAddress.Any.ToString());
			var communicationServer = new SsdpCommunicationsServer(socketFactory);

			Assert.AreEqual(DeviceNetworkType.Ipv4, communicationServer.DeviceNetworkType);
		}

		[TestMethod]
		public void Ctor_WhenIpAddressIsV4_DeviceNetworkTypeEqualsV42()
		{
			var socketFactory = new SocketFactory("127.0.0.1");
			var communicationServer = new SsdpCommunicationsServer(socketFactory);

			Assert.AreEqual(DeviceNetworkType.Ipv4, communicationServer.DeviceNetworkType);
		}

		#endregion

		#region Listen Tests

		[TestMethod]
		public void CommsServer_ReconnectsOnClosedSocket()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);
			server.BeginListeningForBroadcasts();

			var socket1 = socketFactory.MulticastSocket;
			System.Threading.Thread.Sleep(500);

			socketFactory.ThrowSocketClosedException();
			System.Threading.Thread.Sleep(2000);

			var requestReceived = false;
			using (var eventReceivedSignal = new System.Threading.ManualResetEvent(false))
			{
				System.Net.Http.HttpRequestMessage receivedMessage = null;
				server.RequestReceived += (sender, e) =>
				{
					receivedMessage = e.Message;
					requestReceived = true;
					eventReceivedSignal.Set();
				};

				var message = String.Format(@"M-SEARCH * HTTP/1.1
HOST: {0}:{1}
MAN: ""ssdp:discover""
MX: {3}
ST: {2}
CONTENT-ENCODING:UTF8

some content here
",
SsdpConstants.MulticastLocalAdminAddress,
SsdpConstants.MulticastPort,
"test search target",
"1"
);

				socketFactory.MulticastSocket.MockReceive(System.Text.UTF8Encoding.UTF8.GetBytes(message),
					new UdpEndPoint()
					{
						IPAddress = SsdpConstants.MulticastLocalAdminAddress,
						Port = SsdpConstants.MulticastPort
					}
				);

				eventReceivedSignal.WaitOne(120000);
				Assert.AreNotEqual(socket1, socketFactory.MulticastSocket);
				Assert.IsTrue(requestReceived);
			}
		}

		[ExpectedException(typeof(System.ObjectDisposedException))]
		[TestMethod]
		public void CommsServer_BeginListeningForBroadcastsThrowsIfDisposed()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);
			server.Dispose();

			server.BeginListeningForBroadcasts();
		}

		[ExpectedException(typeof(System.ObjectDisposedException))]
		[TestMethod]
		public void CommsServer_StopListeningForBroadcastsThrowsIfDisposed()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);
			server.Dispose();

			server.StopListeningForBroadcasts();
		}

		[ExpectedException(typeof(System.ObjectDisposedException))]
		[TestMethod]
		public void CommsServer_StopListeningForResponsesThrowsIfDisposed()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);
			server.Dispose();

			server.StopListeningForResponses();
		}

		[TestMethod]
		public void CommsServer_ListeningForBroadcastsCreatesMulticastSocket()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			Assert.IsNull(socketFactory.MulticastSocket);
			server.BeginListeningForBroadcasts();
			Assert.IsNotNull(socketFactory.MulticastSocket);
		}

		[TestMethod]
		public void CommsServer_StopListeningForBroadcastsDisposesMulticastSocket()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			server.BeginListeningForBroadcasts();
			server.StopListeningForBroadcasts();

			Assert.IsTrue(((DisposableManagedObjectBase)socketFactory.MulticastSocket).IsDisposed);
		}

		[TestMethod]
		public void CommsServer_StopListeningForResponsesDisposesUnicastSocket()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			string message = "Hello!";
			UdpEndPoint destination = new UdpEndPoint() { IPAddress = "192.168.1.100", Port = 1701 };
			server.SendMessage(System.Text.UTF8Encoding.UTF8.GetBytes(message), destination);

			var mockSocket = socketFactory.UnicastSocket as MockSocket;

			Assert.IsFalse(mockSocket.IsDisposed);

			server.StopListeningForResponses();

			Assert.IsTrue(mockSocket.IsDisposed);
		}

		#endregion

		#region Dispose Tests

		[TestMethod]
		public void CommsServer_DisposeServerDisposesUnicastSocket()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			server.SendMessage(System.Text.UTF8Encoding.UTF8.GetBytes("Hello!"), new UdpEndPoint() { IPAddress = "192.168.1.100", Port = 1701 });
			server.Dispose();

			var mockSocket = socketFactory.UnicastSocket as MockSocket;
			Assert.IsTrue(mockSocket.IsDisposed);
		}

		[TestMethod]
		public void CommsServer_DisposeStopsListeningWithoutError()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			server.BeginListeningForBroadcasts();
			server.Dispose();

			Assert.IsTrue(((DisposableManagedObjectBase)socketFactory.MulticastSocket).IsDisposed);

			var mockSocket = socketFactory.MulticastSocket as MockSocket;
			Assert.IsTrue(mockSocket.IsDisposed);
		}

		#endregion

		#region Send Tests

		[TestMethod]
		public void CommsServer_SendMessageSendsOnUnicastSocket()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			string message = "Hello!";
			UdpEndPoint destination = new UdpEndPoint() { IPAddress = "192.168.1.100", Port = 1701 };
			server.SendMessage(System.Text.UTF8Encoding.UTF8.GetBytes(message), destination);

			Assert.IsNotNull(socketFactory.UnicastSocket);

			var mockSocket = socketFactory.UnicastSocket as MockSocket;
			Assert.AreEqual(message, System.Text.UTF8Encoding.UTF8.GetString(mockSocket.LastMessage));
			Assert.AreEqual(destination.IPAddress, mockSocket.LastSentTo.IPAddress);
			Assert.AreEqual(destination.Port, mockSocket.LastSentTo.Port);
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void CommsServer_SendNullMessageThrowsException()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			UdpEndPoint destination = new UdpEndPoint() { IPAddress = "192.168.1.100", Port = 1701 };
			server.SendMessage(null, destination);
		}

		[TestMethod]
		public void CommsServer_SendMessageV4SendsToSsdpMulticastGroupOnUnicastSoket()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			string message = "Hello Everyone!";
			server.SendMessage(System.Text.UTF8Encoding.UTF8.GetBytes(message), new UdpEndPoint
			{
				IPAddress = SsdpConstants.MulticastLocalAdminAddress,
				Port = SsdpConstants.MulticastPort
			});

			Assert.IsNotNull(socketFactory.UnicastSocket);

			var mockSocket = socketFactory.UnicastSocket as MockSocket;
			Assert.AreEqual(message, System.Text.UTF8Encoding.UTF8.GetString(mockSocket.LastMessage));
			Assert.AreEqual(SsdpConstants.MulticastLocalAdminAddress, mockSocket.LastSentTo.IPAddress);
			Assert.AreEqual(SsdpConstants.MulticastPort, mockSocket.LastSentTo.Port);
		}

		[TestMethod]
		public void CommsServer_SendMessageV6SendsToSsdpMulticastGroupOnUnicastSoket6()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			string message = "Hello Everyone!";
			server.SendMessage(System.Text.UTF8Encoding.UTF8.GetBytes(message), new UdpEndPoint
			{
				IPAddress = SsdpConstants.MulticastAdminLocalAddressV6[0],
				Port = SsdpConstants.MulticastPort
			});

			Assert.IsNotNull(socketFactory.UnicastSocket);

			var mockSocket = socketFactory.UnicastSocket as MockSocket;
			Assert.AreEqual(message, System.Text.UTF8Encoding.UTF8.GetString(mockSocket.LastMessage));
			Assert.AreEqual(SsdpConstants.MulticastAdminLocalAddressV6[0], mockSocket.LastSentTo.IPAddress);
			Assert.AreEqual(SsdpConstants.MulticastPort, mockSocket.LastSentTo.Port);
		}

		[TestMethod]
		public void CommsServer_SendMessageV6SendsToSsdpMulticastGroupOnUnicastSoket2()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			string message = "Hello Everyone!";
			server.SendMessage(System.Text.UTF8Encoding.UTF8.GetBytes(message), new UdpEndPoint
			{
				IPAddress = SsdpConstants.MulticastAdminLocalAddressV6[1],
				Port = SsdpConstants.MulticastPort
			});

			Assert.IsNotNull(socketFactory.UnicastSocket);

			var mockSocket = socketFactory.UnicastSocket as MockSocket;
			Assert.AreEqual(message, System.Text.UTF8Encoding.UTF8.GetString(mockSocket.LastMessage));
			Assert.AreEqual(SsdpConstants.MulticastAdminLocalAddressV6[1], mockSocket.LastSentTo.IPAddress);
			Assert.AreEqual(SsdpConstants.MulticastPort, mockSocket.LastSentTo.Port);
		}

		[TestMethod]
		public void CommsServer_SendMessageV6SendsToSsdpMulticastGroupOnUnicastSoket3()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			string message = "Hello Everyone!";
			server.SendMessage(System.Text.UTF8Encoding.UTF8.GetBytes(message), new UdpEndPoint
			{
				IPAddress = SsdpConstants.MulticastAdminLocalAddressV6[2],
				Port = SsdpConstants.MulticastPort
			});

			Assert.IsNotNull(socketFactory.UnicastSocket);

			var mockSocket = socketFactory.UnicastSocket as MockSocket;
			Assert.AreEqual(message, System.Text.UTF8Encoding.UTF8.GetString(mockSocket.LastMessage));
			Assert.AreEqual(SsdpConstants.MulticastAdminLocalAddressV6[2], mockSocket.LastSentTo.IPAddress);
			Assert.AreEqual(SsdpConstants.MulticastPort, mockSocket.LastSentTo.Port);
		}

		#endregion

		#region Packet Handling Tests

		[TestMethod]
		public void CommsServer_NonHttpPacketsIgnored()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			var requestReceived = false;
			var responseReceived = false;

			server.RequestReceived += (sender, e) =>
				{
					requestReceived = true;
				};

			server.ResponseReceived += (sender, e) =>
				{
					responseReceived = true;
				};

			server.BeginListeningForBroadcasts();

			var mockSocket = socketFactory.MulticastSocket as MockSocket;
			mockSocket.MockReceive(System.Text.UTF8Encoding.UTF8.GetBytes("Not an HTTP message"), new
				UdpEndPoint()
			{ IPAddress = SsdpConstants.MulticastLocalAdminAddress, Port = SsdpConstants.MulticastPort });

			Assert.IsFalse(requestReceived);
			Assert.IsFalse(responseReceived);
		}

		[TestMethod]
		public void CommsServer_HttpPacketsWithInvalidHeaderIgnored()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			var requestReceived = false;
			var responseReceived = false;

			server.RequestReceived += (sender, e) =>
			{
				requestReceived = true;
			};

			server.ResponseReceived += (sender, e) =>
			{
				responseReceived = true;
			};

			server.BeginListeningForBroadcasts();

			var message = String.Format(@"M-SEARCH *
",
"",
SsdpConstants.MulticastLocalAdminAddress,
SsdpConstants.MulticastPort,
"test search target",
"1"
);

			var mockSocket = socketFactory.MulticastSocket as MockSocket;
			mockSocket.MockReceive(System.Text.UTF8Encoding.UTF8.GetBytes(message), new
				UdpEndPoint()
			{ IPAddress = SsdpConstants.MulticastLocalAdminAddress, Port = SsdpConstants.MulticastPort });

			Assert.IsFalse(requestReceived);
			Assert.IsFalse(responseReceived);
		}

		[TestMethod]
		public void CommsServer_HttpRequestInvalidVersionIgnored()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			var requestReceived = false;
			var responseReceived = false;

			server.RequestReceived += (sender, e) =>
			{
				requestReceived = true;
			};

			server.ResponseReceived += (sender, e) =>
			{
				responseReceived = true;
			};

			server.BeginListeningForBroadcasts();

			var message = @"M-SEARCH * HTTP1.1
";

			var mockSocket = socketFactory.MulticastSocket as MockSocket;
			mockSocket.MockReceive(System.Text.UTF8Encoding.UTF8.GetBytes(message), new
				UdpEndPoint()
			{ IPAddress = SsdpConstants.MulticastLocalAdminAddress, Port = SsdpConstants.MulticastPort });

			Assert.IsFalse(requestReceived);
			Assert.IsFalse(responseReceived);
		}

		[TestMethod]
		public void CommsServer_InvalidHttpRequestIgnored()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			var requestReceived = false;
			var responseReceived = false;

			server.RequestReceived += (sender, e) =>
			{
				requestReceived = true;
			};

			server.ResponseReceived += (sender, e) =>
			{
				responseReceived = true;
			};

			var message = String.Format(@"M-SEARCH * HTTP/1.1
",
		"",
		SsdpConstants.MulticastLocalAdminAddress,
		SsdpConstants.MulticastPort,
		"test search target",
		"1"
 );

			server.BeginListeningForBroadcasts();

			var mockSocket = socketFactory.MulticastSocket as MockSocket;
			mockSocket.MockReceive(System.Text.UTF8Encoding.UTF8.GetBytes(message), new
				UdpEndPoint()
			{ IPAddress = SsdpConstants.MulticastLocalAdminAddress, Port = SsdpConstants.MulticastPort });

			Assert.IsFalse(requestReceived);
			Assert.IsFalse(responseReceived);
		}

		[TestMethod]
		public void CommsServer_InvalidHttpResponseIgnored()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			var requestReceived = false;
			var responseReceived = false;

			server.RequestReceived += (sender, e) =>
			{
				requestReceived = true;
			};

			server.ResponseReceived += (sender, e) =>
			{
				responseReceived = true;
			};

			var message = @"HTTP/1.1 200 OK

";

			server.BeginListeningForBroadcasts();

			var mockSocket = socketFactory.MulticastSocket as MockSocket;
			mockSocket.MockReceive(System.Text.UTF8Encoding.UTF8.GetBytes(message), new
				UdpEndPoint()
			{ IPAddress = SsdpConstants.MulticastLocalAdminAddress, Port = SsdpConstants.MulticastPort });

			Assert.IsFalse(requestReceived);
			Assert.IsFalse(responseReceived);
		}

		[TestMethod]
		public void CommsServer_InvalidHttpResponseCodeIgnored()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			var requestReceived = false;
			var responseReceived = false;

			server.RequestReceived += (sender, e) =>
			{
				requestReceived = true;
			};

			server.ResponseReceived += (sender, e) =>
			{
				responseReceived = true;
			};

			var message = @"HTTP/1.1 AAA OK
";

			server.BeginListeningForBroadcasts();

			var mockSocket = socketFactory.MulticastSocket as MockSocket;
			mockSocket.MockReceive(System.Text.UTF8Encoding.UTF8.GetBytes(message), new
				UdpEndPoint()
			{ IPAddress = SsdpConstants.MulticastLocalAdminAddress, Port = SsdpConstants.MulticastPort });

			Assert.IsFalse(requestReceived);
			Assert.IsFalse(responseReceived);
		}
		[TestMethod]
		public void CommsServer_InvalidHttpResponseHeaderIgnored()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			var requestReceived = false;
			var responseReceived = false;

			server.RequestReceived += (sender, e) =>
			{
				requestReceived = true;
			};

			server.ResponseReceived += (sender, e) =>
			{
				responseReceived = true;
			};

			var message = @"HTTP/1.1 200

";

			server.BeginListeningForBroadcasts();

			var mockSocket = socketFactory.MulticastSocket as MockSocket;
			mockSocket.MockReceive(System.Text.UTF8Encoding.UTF8.GetBytes(message), new
				UdpEndPoint()
			{ IPAddress = SsdpConstants.MulticastLocalAdminAddress, Port = SsdpConstants.MulticastPort });

			Assert.IsFalse(requestReceived);
			Assert.IsFalse(responseReceived);
		}

		[TestMethod]
		public void CommsServer_HttpResponseInvalidVersionIgnored()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			var requestReceived = false;
			var responseReceived = false;

			server.RequestReceived += (sender, e) =>
			{
				requestReceived = true;
			};

			server.ResponseReceived += (sender, e) =>
			{
				responseReceived = true;
			};

			var message = @"HTTP/A.1 200 OK

";

			server.BeginListeningForBroadcasts();

			var mockSocket = socketFactory.MulticastSocket as MockSocket;
			mockSocket.MockReceive(System.Text.UTF8Encoding.UTF8.GetBytes(message), new
				UdpEndPoint()
			{ IPAddress = SsdpConstants.MulticastLocalAdminAddress, Port = SsdpConstants.MulticastPort });

			Assert.IsFalse(requestReceived);
			Assert.IsFalse(responseReceived);
		}

		[TestMethod]
		public void CommsServer_HandlesRequestContentHeadersOk()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			var requestReceived = false;
			var responseReceived = false;

			using (var eventReceivedSignal = new System.Threading.ManualResetEvent(false))
			{

				System.Net.Http.HttpRequestMessage receivedMessage = null;
				server.RequestReceived += (sender, e) =>
				{
					receivedMessage = e.Message;
					requestReceived = true;
					eventReceivedSignal.Set();
				};

				server.ResponseReceived += (sender, e) =>
				{
					responseReceived = true;
				};

				var message = String.Format(@"M-SEARCH * HTTP/1.1
HOST: {0}:{1}
MAN: ""ssdp:discover""
MX: {3}
ST: {2}
CONTENT-ENCODING:UTF8

some content here
",
			SsdpConstants.MulticastLocalAdminAddress,
			SsdpConstants.MulticastPort,
			"test search target",
			"1"
	 );

				server.BeginListeningForBroadcasts();

				var mockSocket = socketFactory.MulticastSocket as MockSocket;
				mockSocket.MockReceive(System.Text.UTF8Encoding.UTF8.GetBytes(message), new
					UdpEndPoint()
				{ IPAddress = SsdpConstants.MulticastLocalAdminAddress, Port = SsdpConstants.MulticastPort });

				eventReceivedSignal.WaitOne(10000);
				Assert.IsTrue(requestReceived);
				Assert.IsFalse(responseReceived);
				Assert.AreEqual("UTF8", receivedMessage.Content.Headers.ContentEncoding.FirstOrDefault());
			}
		}

		[TestMethod]
		public void CommsServer_HandlesResponseContentHeadersOk()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			var requestReceived = false;
			var responseReceived = false;

			var eventReceivedSignal = new System.Threading.ManualResetEvent(false);
			System.Net.Http.HttpResponseMessage receivedMessage = null;
			server.RequestReceived += (sender, e) =>
			{
				requestReceived = true;
			};

			server.ResponseReceived += (sender, e) =>
			{
				receivedMessage = e.Message;
				responseReceived = true;
				eventReceivedSignal.Set();
			};

			var message = String.Format(@"HTTP/1.1 200 OK
EXT:
DATE: {0}
CACHE-CONTROL: public, max-age=1800
ST:test search target
SERVER: TestOS/1.0 UPnP/1.0 RSSDP/1.0
USN:testusn
LOCATION:http://somedevice:1700
CONTENT-ENCODING:UTF8

some content here
",
			DateTime.UtcNow.ToString("r")
			);

			server.BeginListeningForBroadcasts();

			var mockSocket = socketFactory.MulticastSocket as MockSocket;
			mockSocket.MockReceive(System.Text.UTF8Encoding.UTF8.GetBytes(message), new
				UdpEndPoint()
			{ IPAddress = SsdpConstants.MulticastLocalAdminAddress, Port = SsdpConstants.MulticastPort });

			eventReceivedSignal.WaitOne(10000);

			Assert.IsFalse(requestReceived);
			Assert.IsTrue(responseReceived);
			Assert.AreEqual("UTF8", receivedMessage.Content.Headers.ContentEncoding.FirstOrDefault());
		}

		[TestMethod]
		public void CommsServer_HttpRequestWithNonAsterixUriIgnored()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			var requestReceived = false;
			var responseReceived = false;

			var eventReceivedSignal = new System.Threading.ManualResetEvent(false);
			server.RequestReceived += (sender, e) =>
			{
				requestReceived = true;
				eventReceivedSignal.Set();
			};

			server.ResponseReceived += (sender, e) =>
			{
				responseReceived = true;
				eventReceivedSignal.Set();
			};

			server.BeginListeningForBroadcasts();

			var message = String.Format(@"M-SEARCH http://someuri HTTP/1.1
HOST: {0}:{1}
MAN: ""ssdp:discover""
MX: {3}
ST: {2}

",
		"",
		SsdpConstants.MulticastLocalAdminAddress,
		SsdpConstants.MulticastPort,
		"test search target",
		"1"
 );


			var mockSocket = socketFactory.MulticastSocket as MockSocket;
			mockSocket.MockReceive(System.Text.UTF8Encoding.UTF8.GetBytes(message), new
				UdpEndPoint()
			{ IPAddress = SsdpConstants.MulticastLocalAdminAddress, Port = SsdpConstants.MulticastPort });

			eventReceivedSignal.WaitOne(1000);

			Assert.IsFalse(requestReceived);
			Assert.IsFalse(responseReceived);
		}

		#endregion

		#region Event Raising Tests

		[TestMethod]
		public void CommsServer_HttpRequestRaisesRequestReceived()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			var requestReceived = false;
			var responseReceived = false;

			var eventReceivedSignal = new System.Threading.ManualResetEvent(false);

			System.Net.Http.HttpRequestMessage receivedMessage = null;
			UdpEndPoint receivedFrom = null;
			server.RequestReceived += (sender, e) =>
			{
				receivedMessage = e.Message;
				receivedFrom = e.ReceivedFrom;
				requestReceived = true;
				eventReceivedSignal.Set();
			};

			server.ResponseReceived += (sender, e) =>
			{
				responseReceived = true;
			};

			server.BeginListeningForBroadcasts();

			var message = String.Format(@"M-SEARCH * HTTP/1.1
HOST: {0}:{1}
MAN: ""ssdp:discover""
MX: {3}
ST: {2}

",
		"",
		SsdpConstants.MulticastLocalAdminAddress,
		SsdpConstants.MulticastPort,
		"test search target",
		"1"
 );


			var mockSocket = socketFactory.MulticastSocket as MockSocket;
			mockSocket.MockReceive(System.Text.UTF8Encoding.UTF8.GetBytes(message), new
				UdpEndPoint()
			{ IPAddress = SsdpConstants.MulticastLocalAdminAddress, Port = SsdpConstants.MulticastPort });

			eventReceivedSignal.WaitOne(10000);

			Assert.IsTrue(requestReceived);
			Assert.IsFalse(responseReceived);
			Assert.AreEqual("M-SEARCH", receivedMessage.Method.Method);
			Assert.AreEqual("*", receivedMessage.RequestUri.ToString());
			Assert.AreEqual(SsdpConstants.MulticastLocalAdminAddress, receivedFrom.IPAddress);
			Assert.AreEqual(SsdpConstants.MulticastPort, receivedFrom.Port);
		}

		[TestMethod]
		public void CommsServer_HttpResponseRaisesResponseReceived()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory);

			var requestReceived = false;
			var responseReceived = false;

			var eventReceivedSignal = new System.Threading.ManualResetEvent(false);

			System.Net.Http.HttpResponseMessage receivedMessage = null;
			UdpEndPoint receivedFrom = null;
			server.RequestReceived += (sender, e) =>
			{
				requestReceived = true;
			};

			server.ResponseReceived += (sender, e) =>
			{
				receivedMessage = e.Message;
				receivedFrom = e.ReceivedFrom;
				responseReceived = true;
				eventReceivedSignal.Set();
			};

			server.BeginListeningForBroadcasts();

			var message = String.Format(@"HTTP/1.1 200 OK
EXT:
DATE: {0}
CACHE-CONTROL: public, max-age=1800
ST:test search target
SERVER: TestOS/1.0 UPnP/1.0 RSSDP/1.0
USN:testusn
LOCATION:http://somedevice:1700

",
	DateTime.UtcNow.ToString("r")
 );


			var mockSocket = socketFactory.MulticastSocket as MockSocket;
			mockSocket.MockReceive(System.Text.UTF8Encoding.UTF8.GetBytes(message), new
				UdpEndPoint()
			{ IPAddress = SsdpConstants.MulticastLocalAdminAddress, Port = SsdpConstants.MulticastPort });

			eventReceivedSignal.WaitOne(10000);

			Assert.IsFalse(requestReceived);
			Assert.IsTrue(responseReceived);
			Assert.IsTrue(receivedMessage.IsSuccessStatusCode);
			Assert.AreEqual(System.Net.HttpStatusCode.OK, receivedMessage.StatusCode);
			Assert.AreEqual(SsdpConstants.MulticastLocalAdminAddress, receivedFrom.IPAddress);
			Assert.AreEqual(SsdpConstants.MulticastPort, receivedFrom.Port);
		}

		#endregion

		[TestMethod]
		public void CommsServer_IsSharedPropertyReturnsSetValue()
		{
			var socketFactory = new MockSocketFactory();
			var server = new SsdpCommunicationsServer(socketFactory, 1701, 4);
			server.IsShared = true;
			Assert.IsTrue(server.IsShared);
			server.IsShared = false;
			Assert.IsFalse(server.IsShared);
		}

		#region Private Class

		private class MockSocketFactory : ISocketFactory
		{
			private MockSocket _UnicastSocket;
			private MockSocket _MulticastSocket;

			#region ISocketFactory Members

			public IUdpSocket CreateUdpSocket(int localPort)
			{
				return (_UnicastSocket = new MockSocket(localPort));
			}

			public IUdpSocket CreateUdpMulticastSocket(int multicastTimeToLive, int localPort)
			{
				return (_MulticastSocket = new MockSocket(null, multicastTimeToLive, localPort));
			}

			public DeviceNetworkType DeviceNetworkType
			{
				get { throw new NotImplementedException(); }
			}

			#endregion

			public void ThrowSocketClosedException()
			{
				_MulticastSocket.ThrowSocketClosedException();
			}

			public MockSocket UnicastSocket
			{
				get { return _UnicastSocket; }
			}

			public MockSocket MulticastSocket
			{
				get { return _MulticastSocket; }
			}
		}

		private class MockSocket : DisposableManagedObjectBase, IUdpSocket
		{

			private int _LocalPort;
			private string _IpAddress;
			private int _MulticastTimeToLive;

			private byte[] _LastMessage;
			private UdpEndPoint _LastSentTo;

			private System.Collections.Generic.Queue<ReceivedUdpData> _ReceiveQueue = new Queue<ReceivedUdpData>();
			private System.Threading.ManualResetEvent _DataAvailableSignal = new System.Threading.ManualResetEvent(false);

			public MockSocket(int localPort)
			{
				_LocalPort = localPort;
			}

			public MockSocket(string ipAddress, int multicastTimeToLive, int localPort)
			{
				_IpAddress = ipAddress;
				_MulticastTimeToLive = multicastTimeToLive;
				_LocalPort = localPort;
			}

			public int LocalPort
			{
				get { return _LocalPort; }
			}

			public UdpEndPoint LastSentTo
			{
				get { return _LastSentTo; }
			}

			public byte[] LastMessage
			{
				get { return _LastMessage; }
			}

			public void MockReceive(byte[] data, UdpEndPoint fromEndPoint)
			{
				_ReceiveQueue.Enqueue(new ReceivedUdpData() { Buffer = data, ReceivedFrom = fromEndPoint, ReceivedBytes = data.Length });
				_DataAvailableSignal.Set();
			}

			#region IUdpSocket Members

			public Task<ReceivedUdpData> ReceiveAsync()
			{
				var tcs = new TaskCompletionSource<ReceivedUdpData>();

				Task.Run
				(
					() =>
					{
						try
						{
							var signal = _DataAvailableSignal;
							if (!_ReceiveQueue.Any() && signal != null)
								signal.WaitOne();

							if (this.IsDisposed)
								tcs.SetCanceled();
							else
							{
								var message = _ReceiveQueue.Dequeue();
								if (message.Buffer == null && message.ReceivedBytes == 0 && message.ReceivedFrom == null)
									tcs.SetException(new SocketClosedException());
								else
									tcs.SetResult(message);

								signal.Reset();
							}
						}
						catch (ObjectDisposedException)
						{
							if (!tcs.Task.IsCompleted)
								tcs.SetCanceled();
						}
					}
				);

				return tcs.Task;
			}

			public void SendTo(byte[] messageData, UdpEndPoint endPoint)
			{
				_LastSentTo = endPoint;
				_LastMessage = messageData;
			}

			#endregion

			protected override void Dispose(bool disposing)
			{
				_ReceiveQueue.Clear();
				var data = _DataAvailableSignal;
				_DataAvailableSignal = null;
				data.Dispose();
			}

			public void ThrowSocketClosedException()
			{
				_ReceiveQueue.Enqueue(new ReceivedUdpData() { Buffer = null, ReceivedFrom = null, ReceivedBytes = 0 });
				_DataAvailableSignal.Set();
			}
		}

		#endregion

	}
}