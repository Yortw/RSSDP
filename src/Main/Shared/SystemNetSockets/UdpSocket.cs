using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rssdp.Infrastructure;

namespace Rssdp
{
	// THIS IS A LINKED FILE - SHARED AMONGST MULTIPLE PLATFORMS	
	// Be careful to check any changes compile and work for all platform projects it is shared in.
	internal sealed class UdpSocket : DisposableManagedObjectBase, IUdpSocket
	{

		#region Fields

		private System.Net.Sockets.Socket _Socket;
		private int _LocalPort;

		#endregion

		#region Constructors

		public UdpSocket(Socket socket, string ipAddress, int localPort)
		{
			if (socket == null) throw new ArgumentNullException("socket");

			_Socket = socket;
			_LocalPort = localPort;

			var ip = String.IsNullOrEmpty(ipAddress) ? GetDefaultIpAddress(socket) : IPAddress.Parse(ipAddress);

			_Socket.Bind(new IPEndPoint(ip, _LocalPort));
			if (_LocalPort == 0)
				_LocalPort = ((IPEndPoint) _Socket.LocalEndPoint).Port;
		}

		#endregion

		#region IUdpSocket Members

		public System.Threading.Tasks.Task<ReceivedUdpData> ReceiveAsync()
		{
			ThrowIfDisposed();

			var tcs = new TaskCompletionSource<ReceivedUdpData>();

			System.Net.EndPoint receivedFromEndPoint = new IPEndPoint(GetDefaultIpAddress(_Socket), 0);
			var state = new AsyncReceiveState(_Socket, receivedFromEndPoint);
			state.TaskCompletionSource = tcs;
#if NETSTANDARD1_3
			_Socket.ReceiveFromAsync(new System.ArraySegment<Byte>(state.Buffer), System.Net.Sockets.SocketFlags.None, state.EndPoint)
				.ContinueWith((task, asyncState) =>
				{
					if (this.IsDisposed) return;

					try
					{
						if (task.Status != TaskStatus.Faulted)
						{
							var receiveState = asyncState as AsyncReceiveState;
							receiveState.EndPoint = task.Result.RemoteEndPoint;
							ProcessResponse(receiveState, () => task.Result.ReceivedBytes);
						}
					}
					catch (ObjectDisposedException) { if (!this.IsDisposed) throw; } //Only rethrow disposed exceptions if we're NOT disposed, because then they are unexpected.
				}, state);
#else
			_Socket.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, System.Net.Sockets.SocketFlags.None, ref state.EndPoint,
				new AsyncCallback((result) => ProcessResponse(state, () => state.Socket.EndReceiveFrom(result, ref state.EndPoint))), state);
#endif

			return tcs.Task;
		}

		public void SendTo(byte[] messageData, UdpEndPoint endPoint)
		{
			ThrowIfDisposed();

			if (messageData == null) throw new ArgumentNullException("messageData");
			if (endPoint == null) throw new ArgumentNullException("endPoint");

			_Socket.SendTo(messageData, new System.Net.IPEndPoint(IPAddress.Parse(endPoint.IPAddress), endPoint.Port));
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

		#region Private Methods

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions via task methods should be reported by task completion source, so this should be ok.")]
		private static void ProcessResponse(AsyncReceiveState state, Func<int> receiveData)
		{
			try
			{
				var bytesRead = receiveData();

				var ipEndPoint = state.EndPoint as IPEndPoint;
				state.TaskCompletionSource.SetResult(
					new ReceivedUdpData()
					{
						Buffer = state.Buffer,
						ReceivedBytes = bytesRead,
						ReceivedFrom = new UdpEndPoint()
						{
							IPAddress = ipEndPoint.Address.ToString(),
							Port = ipEndPoint.Port
						}
					}
				);
			}
			catch (ObjectDisposedException)
			{
				state.TaskCompletionSource.SetCanceled();
			}
			catch (SocketException se)
			{
				if (se.SocketErrorCode != SocketError.Interrupted && se.SocketErrorCode != SocketError.OperationAborted && se.SocketErrorCode != SocketError.Shutdown)
					state.TaskCompletionSource.SetException(se);
				else
					state.TaskCompletionSource.SetCanceled();
			}
#if NETSTANDARD
			// Unrecoverable exceptions should not be getting caught and will be dealt with on a broad level by a high-level catch-all handler
			// https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/breaking-change-rules.md#exceptions
#else
			catch (StackOverflowException) // Handle this manually as we may not be able to call a sub method to check exception type etc.
			{
				throw;
			}
#endif
			catch (Exception ex)
			{
				if (ex.IsCritical()) //Critical exceptions that indicate memory corruption etc. shouldn't be caught.
					throw;

				state.TaskCompletionSource.SetException(ex);
			}
		}

		#endregion

		#region Private Classes

		private class AsyncReceiveState
		{
			public AsyncReceiveState(System.Net.Sockets.Socket socket, EndPoint endPoint)
			{
				this.Socket = socket;
				this.EndPoint = endPoint;
			}

			public EndPoint EndPoint;
			public byte[] Buffer = new byte[SsdpConstants.DefaultUdpSocketBufferSize];

			public System.Net.Sockets.Socket Socket { get; private set; }

			public TaskCompletionSource<ReceivedUdpData> TaskCompletionSource { get; set; }

		}

		private static IPAddress GetDefaultIpAddress(Socket socket)
		{
			switch (socket.AddressFamily)
			{
				case AddressFamily.InterNetwork:
					return IPAddress.Any;
				case AddressFamily.InterNetworkV6:
					return IPAddress.IPv6Any;
			}

			return IPAddress.None;
		}
		#endregion

	}
}