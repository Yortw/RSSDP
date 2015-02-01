using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rssdp.Infrastructure;

namespace Rssdp
{
	internal sealed class UdpSocket : DisposableManagedObjectBase, IUdpSocket
	{

		#region Fields

		private System.Net.Sockets.Socket _Socket;
		private int _LocalPort;

		#endregion

		#region Constructors

		public UdpSocket(System.Net.Sockets.Socket socket, int localPort)
		{
			if (socket == null) throw new ArgumentNullException("socket");

			_Socket = socket;
			_LocalPort = localPort;

			_Socket.Bind(new IPEndPoint(IPAddress.Any, localPort));
			if (_LocalPort == 0)
				_LocalPort = (_Socket.LocalEndPoint as IPEndPoint).Port;
		}

		#endregion

		#region IUdpSocket Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "This is the worlds ugliest Microsoft API anyway, but we can't dispose the event args object or else it will cause the async call to fail. The object should be disposed of when we are done with it, in the event handler.")]
		public System.Threading.Tasks.Task<ReceivedUdpData> ReceiveAsync()
		{
			ThrowIfDisposed();

			var tcs = new TaskCompletionSource<ReceivedUdpData>();

			var socketEventArg = new SocketAsyncEventArgs();
			try
			{
				socketEventArg.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
				socketEventArg.UserToken = tcs;

				socketEventArg.SetBuffer(new Byte[SsdpConstants.DefaultUdpSocketBufferSize], 0, SsdpConstants.DefaultUdpSocketBufferSize);

				socketEventArg.Completed += socketEventArg_ReceiveCompleted;

				_Socket.ReceiveAsync(socketEventArg);
			}
			catch
			{
				socketEventArg.Dispose();

				throw;
			}

			return tcs.Task;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "This is the worlds ugliest Microsoft API anyway, but we can't dispose the event args object or else it will cause the async call to fail. The object should be disposed of when we are done with it, in the event handler.")]
		public void SendTo(byte[] messageData, UdpEndPoint endPoint)
		{
			if (messageData == null) throw new ArgumentNullException("messageData");
			if (endPoint == null) throw new ArgumentNullException("endPoint");

			ThrowIfDisposed();

			var args = new SocketAsyncEventArgs();
			try
			{
				args.SetBuffer(messageData, 0, messageData.Length);
				args.RemoteEndPoint = new System.Net.IPEndPoint(IPAddress.Parse(endPoint.IPAddress), endPoint.Port);

				var signal = new ManualResetEvent(false);
				try
				{
					args.Completed += (sender, e) =>
					{
						signal.Set();
					};

					_Socket.SendToAsync(args);

					signal.WaitOne();
				}
				finally
				{
					signal.Dispose();
				}
			}
			catch
			{
				if (args != null)
					args.Dispose();

				throw;
			}
		}

		#endregion

		#region Overrides

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var socket = _Socket;
				if (socket != null)
					socket.Dispose();
			}
		}

		#endregion

		#region Event Handlers

		private void socketEventArg_ReceiveCompleted(object sender, SocketAsyncEventArgs e)
		{
			try
			{
				var ttcs = e.UserToken as TaskCompletionSource<ReceivedUdpData>;

				if (e.SocketError == SocketError.Success)
				{
					var result = new ReceivedUdpData();
					result.Buffer = new byte[e.BytesTransferred];
					result.ReceivedBytes = e.BytesTransferred;

					var endPoint = e.RemoteEndPoint as IPEndPoint;
					result.ReceivedFrom = new UdpEndPoint()
					{
						IPAddress = endPoint.Address.ToString(),
						Port = endPoint.Port
					};

					Array.Copy(e.Buffer, e.Offset, result.Buffer, 0, e.BytesTransferred);

					ttcs.SetResult(result);
				}
				else
					ttcs.SetException(new System.Net.Sockets.SocketException((int)e.SocketError));
			}
			finally
			{
				e.Dispose();
			}
		}

		#endregion

	}
}