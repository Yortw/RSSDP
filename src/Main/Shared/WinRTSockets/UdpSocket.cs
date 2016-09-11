using System;
using System.Threading.Tasks;
using Rssdp.Infrastructure;
using System.IO;
using System.Linq;

namespace Rssdp
{
	internal sealed class UwaUdpSocket : IUdpSocket
	{
		private string _LocalIPAddress;
		private int _LocalPort;
		private int _MulticastTimeToLive;

		private System.Threading.ManualResetEvent _DataAvailableSignal;
		private System.Collections.Concurrent.ConcurrentQueue<ReceivedUdpData> _ReceivedData;

		private Windows.Networking.Sockets.DatagramSocket _Socket;

		public UwaUdpSocket(string ipAddress, int multicastTimeToLive, int localPort, string localIPAddress)
		{
			_LocalIPAddress = localIPAddress;
			_DataAvailableSignal = new System.Threading.ManualResetEvent(false);
			_ReceivedData = new System.Collections.Concurrent.ConcurrentQueue<ReceivedUdpData>();

			_MulticastTimeToLive = multicastTimeToLive;
			_LocalPort = localPort;

			_Socket = new Windows.Networking.Sockets.DatagramSocket();
#if !WINRT
			_Socket.Control.MulticastOnly = true;
#endif
			_Socket.MessageReceived += _Socket_MessageReceived;

			BindSocket();
			_Socket.JoinMulticastGroup(new Windows.Networking.HostName(ipAddress));
		}

		public UwaUdpSocket(int localPort, string localIPAddress)
		{
			_DataAvailableSignal = new System.Threading.ManualResetEvent(false);
			_ReceivedData = new System.Collections.Concurrent.ConcurrentQueue<ReceivedUdpData>();

			this._LocalPort = localPort;

			_Socket = new Windows.Networking.Sockets.DatagramSocket();
			_Socket.MessageReceived += _Socket_MessageReceived;

			BindSocket();
		}

		public Task<ReceivedUdpData> ReceiveAsync()
		{
			return Task.Run<ReceivedUdpData>(() =>
			{
				ReceivedUdpData data = null;

				while (!_ReceivedData.TryDequeue(out data))
				{
					_DataAvailableSignal.WaitOne();
				}
				_DataAvailableSignal.Reset();

				return data;
			});
		}

		public void SendTo(byte[] messageData, UdpEndPoint endPoint)
		{
			using (var stream = (_Socket.GetOutputStreamAsync(new Windows.Networking.HostName(endPoint.IPAddress), endPoint.Port.ToString()).AsTask().Result))
			{
				using (var outStream = stream.AsStreamForWrite())
				{
					outStream.Write(messageData, 0, messageData.Length);
					outStream.Flush();
				}
			}
		}

		public void Dispose()
		{
			try
			{
				var socket = _Socket;
				if (socket != null)
				{
					_Socket = null;
					socket.Dispose();
				}
			}
			finally
			{
				GC.SuppressFinalize(this);
			}
		}

		private void _Socket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender, Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
		{
			using (var reader = args.GetDataReader())
			{
				var data = new ReceivedUdpData()
				{
					ReceivedBytes = Convert.ToInt32(reader.UnconsumedBufferLength),
					ReceivedFrom = new UdpEndPoint()
					{
						IPAddress = args.RemoteAddress.RawName,
						Port = Convert.ToInt32(args.RemotePort)
					}
				};

				data.Buffer = new byte[data.ReceivedBytes];
				reader.ReadBytes(data.Buffer);

				_ReceivedData.Enqueue(data);
				_DataAvailableSignal.Set();
			}
		}

		private Windows.Networking.HostName GetLocalIPInfo()
		{
			var localIpInfo = (
													from n
													in Windows.Networking.Connectivity.NetworkInformation.GetHostNames()
													where n.DisplayName == _LocalIPAddress
														&& n.IPInformation != null
														&& n.IPInformation.NetworkAdapter != null
													select n
												).FirstOrDefault();

			if (localIpInfo == null) throw new InvalidOperationException("Could not find adapter from local IP address");
			return localIpInfo;
		}

		private void BindSocket()
		{
			Task t;
			if (!String.IsNullOrEmpty(_LocalIPAddress))
			{
				Windows.Networking.HostName localIpInfo = GetLocalIPInfo();

				t = _Socket.BindServiceNameAsync(this._LocalPort.ToString(), localIpInfo.IPInformation.NetworkAdapter).AsTask();
			}
			else
				t = _Socket.BindServiceNameAsync(this._LocalPort.ToString()).AsTask();

			t.Wait();
		}

	}
}