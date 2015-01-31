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

	internal sealed class UdpSocket : IUdpSocket
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

			_Socket.Bind(new IPEndPoint(IPAddress.Any, _LocalPort));
			if (_LocalPort == 0)
				_LocalPort = (_Socket.LocalEndPoint as IPEndPoint).Port;
		}

		#endregion

		#region IUdpSocket Members

		public System.Threading.Tasks.Task<ReceivedUdpData> ReceiveAsync()
		{
			var tcs = new TaskCompletionSource<ReceivedUdpData>();

			System.Net.EndPoint receivedFromEndPoint = new IPEndPoint(IPAddress.Any, 0);
			var state = new AsyncReceiveState(_Socket, receivedFromEndPoint);
			state.TaskCompletionSource = tcs;
			_Socket.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, System.Net.Sockets.SocketFlags.None, ref state.EndPoint, new AsyncCallback(this.ProcessResponse), state);

			return tcs.Task;
		}

		public void SendTo(byte[] messageData, UdpEndPoint endPoint)
		{
			if (messageData == null) throw new ArgumentNullException("messageData");
			if (endPoint == null) throw new ArgumentNullException("endPoint");

			_Socket.SendTo(messageData, new System.Net.IPEndPoint(IPAddress.Parse(endPoint.IPAddress), endPoint.Port));
		}

		#endregion

		#region Private Methods

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification="Exceptions via task methods should be reported by task completion source, so this should be ok.")]
		private void ProcessResponse(IAsyncResult asyncResult)
		{
			var state = asyncResult.AsyncState as AsyncReceiveState;
			try
			{
				var bytesRead = state.Socket.EndReceiveFrom(asyncResult, ref state.EndPoint);

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
			catch (StackOverflowException) // Handle this manually as we may not be able to call a sub method to check exception type etc.
			{
				throw;
			}
			catch (Exception ex)
			{
				if (ex.IsCritical()) //Critical exceptions that indicate memory corruption etc. shouldn't be caught.
					throw;

				state.TaskCompletionSource.SetException(ex);
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			try
			{
				var socket = _Socket;
				if (socket != null)
					socket.Dispose();
			}
			finally
			{
				GC.SuppressFinalize(this);
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

		#endregion

	}
}