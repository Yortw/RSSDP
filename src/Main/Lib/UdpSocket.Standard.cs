#if !UAP && !WINDOWS_UWP

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Rssdp.Infrastructure;

namespace Rssdp
{
	internal sealed class UdpSocket : DisposableManagedObjectBase, IUdpSocket
	{

		#region Fields

		private readonly System.Net.Sockets.Socket _Socket;
		private readonly int _LocalPort;

		#endregion

		#region Constructors

		public UdpSocket(Socket socket, string ipAddress, int localPort)
		{
			if (socket == null) throw new ArgumentNullException(nameof(socket));

			_Socket = socket;
			_LocalPort = localPort;

			var ip = String.IsNullOrEmpty(ipAddress) ? GetDefaultIpAddress(socket) : IPAddress.Parse(ipAddress);

			_Socket.Bind(new IPEndPoint(ip, _LocalPort));
			if (_LocalPort == 0)
			{
				if (_Socket.LocalEndPoint is not IPEndPoint endPoint)
					throw new ArgumentException("No localPort specified and unable to determine port after socket bound. Please specify a port.", nameof(localPort));

				_LocalPort = endPoint.Port;
			}
		}

		#endregion

		#region IUdpSocket Members

		public System.Threading.Tasks.Task<ReceivedUdpData> ReceiveAsync()
		{
			ThrowIfDisposed();

			var tcs = new TaskCompletionSource<ReceivedUdpData>();

			System.Net.EndPoint receivedFromEndPoint = new IPEndPoint(GetDefaultIpAddress(_Socket), 0);
			var state = new AsyncReceiveState(_Socket, receivedFromEndPoint, tcs);

			_Socket.ReceiveFromAsync(new System.ArraySegment<Byte>(state.Buffer), System.Net.Sockets.SocketFlags.None, state.EndPoint)
				.ContinueWith((task, asyncState) =>
				{
					if (this.IsDisposed)
					{
						tcs.TrySetCanceled(); // Dont' leave clients waiting on tcs.Task hanging if task not completed yet.
						return;
					}

					try
					{
						if (task.Status != TaskStatus.Faulted)
						{
							if (asyncState is AsyncReceiveState receiveState)
							{
								receiveState.EndPoint = task.Result.RemoteEndPoint;
								ProcessResponse(receiveState, () => task.Result.ReceivedBytes);
								return;
							}

							tcs.TrySetException(new InvalidOperationException("asyncState was not an AsyncReceiveState instance.")); // Dont' leave clients waiting on tcs.Task hanging if task not completed yet.
							return;
						}

						tcs.TrySetException(task.Exception?.InnerExceptions.FirstOrDefault() ?? new InvalidOperationException("An unknown error occurred during socket receive operation.")); // Dont' leave clients waiting on tcs.Task hanging if task not completed yet.
					}
					catch (ObjectDisposedException odex) 
					{
						if (!this.IsDisposed) //Only rethrow disposed exceptions if we're NOT disposed, because then they are unexpected.
						{
							tcs.TrySetException(odex);
							throw;
						}

						tcs.TrySetCanceled(); // If we are disposed and anyone is still waiting on tcs.task, tell them we cancelled.
					} 
				}, state);

			return tcs.Task;
		}

		public void SendTo(byte[] messageData, UdpEndPoint endPoint)
		{
			ThrowIfDisposed();

			if (messageData == null) throw new ArgumentNullException(nameof(messageData));
			if (endPoint == null) throw new ArgumentNullException(nameof(endPoint));

			_Socket.SendTo(messageData, new System.Net.IPEndPoint(IPAddress.Parse(endPoint.IPAddress), endPoint.Port));
		}

		#endregion

		#region Overrides

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var socket = _Socket;
				socket?.Dispose();
			}
		}

		#endregion

		#region Private Methods

		private static void ProcessResponse(AsyncReceiveState state, Func<int> receiveData)
		{
			try
			{
				var bytesRead = receiveData();

				var ipEndPoint = (IPEndPoint)state.EndPoint;
				state.TaskCompletionSource.SetResult
				(
					new ReceivedUdpData()
					{
						Buffer = state.Buffer,
						ReceivedBytes = bytesRead,
						ReceivedFrom = new UdpEndPoint(ipEndPoint.Address.ToString(), ipEndPoint.Port)
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
			catch (Exception ex)
			{
				if (ex.IsCritical()) //Critical exceptions that indicate memory corruption etc. shouldn't be caught.
					throw;

				state.TaskCompletionSource.SetException(ex);
			}
		}

		#endregion

		#region Private Classes

		private sealed class AsyncReceiveState
		{
			public AsyncReceiveState(System.Net.Sockets.Socket socket, EndPoint endPoint, TaskCompletionSource<ReceivedUdpData> taskCompletionSource)
			{
				this.Socket = socket;
				this.EndPoint = endPoint;
				this.TaskCompletionSource = taskCompletionSource;
			}

			public EndPoint EndPoint;
			public byte[] Buffer = new byte[SsdpConstants.DefaultUdpSocketBufferSize];

			public System.Net.Sockets.Socket Socket { get; }

			public TaskCompletionSource<ReceivedUdpData> TaskCompletionSource { get; private set; }

		}

		private static IPAddress GetDefaultIpAddress(Socket socket)
		{
			return socket.AddressFamily switch
			{
				AddressFamily.InterNetwork => IPAddress.Any,
				AddressFamily.InterNetworkV6 => IPAddress.IPv6Any,
				_ => IPAddress.None,
			};
		}
		#endregion

	}
}

#endif