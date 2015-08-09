using System;
using System.Threading.Tasks;
using Rssdp.Infrastructure;
using System.IO;

namespace Rssdp
{
	public sealed class UwaUdpSocket : IUdpSocket
	{
		private string ipAddress;
		private int localPort;
		private int multicastTimeToLive;

		private System.Threading.ManualResetEvent _DataAvailableSignal;
		private System.Collections.Concurrent.ConcurrentQueue<ReceivedUdpData> _ReceivedData;

		private Windows.Networking.Sockets.DatagramSocket _Socket;


		public UwaUdpSocket(string ipAddress, int multicastTimeToLive, int localPort)
		{
			_DataAvailableSignal = new System.Threading.ManualResetEvent(false);
			_ReceivedData = new System.Collections.Concurrent.ConcurrentQueue<ReceivedUdpData>();

			this.ipAddress = ipAddress;
			this.multicastTimeToLive = multicastTimeToLive;
			this.localPort = localPort;

			_Socket = new Windows.Networking.Sockets.DatagramSocket();
			//_Socket.Control.MulticastOnly = true;
			_Socket.MessageReceived += _Socket_MessageReceived;
			var t = _Socket.BindServiceNameAsync(this.localPort.ToString()).AsTask();
			t.Wait();
			_Socket.JoinMulticastGroup(new Windows.Networking.HostName(Rssdp.Infrastructure.SsdpConstants.MulticastLocalAdminAddress));
		}

		public UwaUdpSocket(int localPort)
		{
			_DataAvailableSignal = new System.Threading.ManualResetEvent(false);
			_ReceivedData = new System.Collections.Concurrent.ConcurrentQueue<ReceivedUdpData>();

			this.localPort = localPort;

			_Socket = new Windows.Networking.Sockets.DatagramSocket();
			_Socket.MessageReceived += _Socket_MessageReceived;

			var t = _Socket.BindServiceNameAsync(this.localPort.ToString()).AsTask();
			t.Wait();
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
	}
}