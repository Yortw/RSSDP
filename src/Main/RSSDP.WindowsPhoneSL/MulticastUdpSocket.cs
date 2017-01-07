using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Rssdp.Infrastructure;

namespace Rssdp
{
	internal sealed class MulticastUdpSocket : IUdpSocket
	{

		#region Fields

		private UdpAnySourceMulticastClient _UdpClient;

		private bool _IsDisposed;

		#endregion

		#region Constructors

		public MulticastUdpSocket(int localPort)
		{
			_UdpClient = CreateMulticastClientAndJoinGroup(SsdpConstants.MulticastLocalAdminAddress, localPort);
		}

		#endregion

		#region IUdpSocket Members

		public Task<ReceivedUdpData> ReceiveAsync()
		{
			ThrowIfDisposed();

			var taskCompletionSource = new System.Threading.Tasks.TaskCompletionSource<ReceivedUdpData>();

			byte[] buffer = new byte[SsdpConstants.DefaultUdpSocketBufferSize];
			try
			{
				_UdpClient.BeginReceiveFromGroup(buffer, 0, buffer.Length,
					(asyncResult) =>
					{
						IPEndPoint receivedFromEndPoint;

						try
						{
							_UdpClient.EndReceiveFromGroup(asyncResult, out receivedFromEndPoint);

							var tcs = asyncResult.AsyncState as System.Threading.Tasks.TaskCompletionSource<ReceivedUdpData>;

							var result = new ReceivedUdpData()
							{
								ReceivedFrom = new UdpEndPoint()
								{
									IPAddress = receivedFromEndPoint.Address.ToString(),
									Port = receivedFromEndPoint.Port
								},
								Buffer = buffer,
								ReceivedBytes = buffer.Length
							};

							tcs.SetResult(result);
						}
						catch (SocketException se)
						{
							if (se.SocketErrorCode == SocketError.Shutdown || se.SocketErrorCode == SocketError.OperationAborted || se.SocketErrorCode == SocketError.NotConnected)
								throw new SocketClosedException(se.Message, se);

							throw;
						}
					},
					taskCompletionSource
				);
			}
			catch (SocketException se)
			{
				if (se.SocketErrorCode == SocketError.Shutdown || se.SocketErrorCode == SocketError.OperationAborted || se.SocketErrorCode == SocketError.NotConnected)
					throw new SocketClosedException(se.Message, se);

				throw;
			}

			return taskCompletionSource.Task;
		}

		public void SendTo(byte[] messageData, UdpEndPoint endPoint)
		{
			if (messageData == null) throw new ArgumentNullException("messageData");
			if (endPoint == null) throw new ArgumentNullException("endPoint");

			ThrowIfDisposed();

			var signal = new System.Threading.ManualResetEvent(false);
			try
			{
				_UdpClient.BeginSendToGroup(messageData, 0, messageData.Length,
					(asyncResult) =>
					{
						_UdpClient.EndSendToGroup(asyncResult);
						signal.Set();
					}
					, null
				);
				signal.WaitOne();
			}
			finally
			{
				signal.Dispose();
			}
		}

		#endregion

		#region Private Methods

		private void ThrowIfDisposed()
		{
			if (_IsDisposed) throw new ObjectDisposedException("Socket is disposed.");
		}

		private static UdpAnySourceMulticastClient CreateMulticastClientAndJoinGroup(string ipAddress, int localPort)
		{
			var retVal = new UdpAnySourceMulticastClient(IPAddress.Parse(ipAddress), localPort);

			var signal = new System.Threading.ManualResetEvent(false);
			try
			{
				retVal.BeginJoinGroup(
					(joinResult) =>
					{
						retVal.EndJoinGroup(joinResult);
						retVal.MulticastLoopback = true;
						retVal.SendBufferSize = retVal.ReceiveBufferSize = SsdpConstants.DefaultUdpSocketBufferSize;

						signal.Set();
					}, null);

				signal.WaitOne();
			}
			finally
			{
				signal.Dispose();
			}
			return retVal;
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			try
			{
				_IsDisposed = true;

				if (_UdpClient != null)
					_UdpClient.Dispose();
			}
			finally
			{
				GC.SuppressFinalize(this);
			}
		}

		#endregion
	}
}