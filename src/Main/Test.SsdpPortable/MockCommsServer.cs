using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Rssdp;
using Rssdp.Infrastructure;

namespace Test.RssdpPortable
{
	public class MockCommsServer : DisposableManagedObjectBase, ISsdpCommunicationsServer
	{

		private System.Threading.ManualResetEvent _BroadcastAvailableSignal = new System.Threading.ManualResetEvent(false);
		private System.Collections.Generic.Queue<ReceivedUdpData> _ReceivedBroadcastsQueue = new Queue<ReceivedUdpData>();

		private System.Threading.ManualResetEvent _MessageAvailableSignal = new System.Threading.ManualResetEvent(false);
		private System.Collections.Generic.Queue<ReceivedUdpData> _ReceivedMessageQueue = new Queue<ReceivedUdpData>();

		private System.Threading.AutoResetEvent _MessageProcessedSignal = new System.Threading.AutoResetEvent(false);

		private HttpRequestParser _RequestParser = new HttpRequestParser();
		private HttpResponseParser _ResponseParser = new HttpResponseParser();

		public System.Collections.Generic.Queue<ReceivedUdpData> SentMessages = new Queue<ReceivedUdpData>();
		public System.Collections.Generic.Queue<ReceivedUdpData> SentBroadcasts = new Queue<ReceivedUdpData>();

		private System.Threading.ManualResetEvent _SentBroadcastSignal = new System.Threading.ManualResetEvent(false);
		private System.Threading.ManualResetEvent _SentMessageSignal = new System.Threading.ManualResetEvent(false);

		private System.Threading.Timer _MessageSentSignalTimer;
		private System.Threading.Timer _BroadcastSentSignalTimer;

		private System.Threading.Tasks.Task _ListenTask;

		public MockCommsServer()
		{
			_MessageSentSignalTimer = new System.Threading.Timer((reserved) => _SentMessageSignal.Set(), null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
			_BroadcastSentSignalTimer = new System.Threading.Timer((reserved) => _SentBroadcastSignal.Set(), null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
		}

		protected override void Dispose(bool disposing)
		{
			var signal = _BroadcastAvailableSignal;
			_BroadcastAvailableSignal = null;
			if (signal != null)
			{
				signal.Set();
				signal.Dispose();
			}

			signal = _MessageAvailableSignal;
			_MessageAvailableSignal = null; 
			if (signal != null)
			{
				signal.Set();
				signal.Dispose();
			}
		}

		#region ISsdpCommunicationsServer Members

		public event EventHandler<RequestReceivedEventArgs> RequestReceived;

		public event EventHandler<ResponseReceivedEventArgs> ResponseReceived;

		public void BeginListeningForBroadcasts()
		{
			var t = Task.Run(() =>
				{
					try
					{
						while (!this.IsDisposed)
						{
							_BroadcastAvailableSignal.WaitOne();

							if (this.IsDisposed) break;

							while (_ReceivedBroadcastsQueue.Any())
							{
								if (this.IsDisposed) break;

								var data = _ReceivedBroadcastsQueue.Dequeue();
								Action processWork = () => ProcessMessage(System.Text.UTF8Encoding.UTF8.GetString(data.Buffer, 0, data.ReceivedBytes), data.ReceivedFrom);
								var processTask = TaskEx.Run(processWork);

							}
							_BroadcastAvailableSignal.Reset();
						}
					}
					catch (ObjectDisposedException) { }
				});
		}

		public void StopListeningForBroadcasts()
		{
			_ReceivedBroadcastsQueue.Clear();
			_BroadcastAvailableSignal.Dispose();
			_BroadcastAvailableSignal = new System.Threading.ManualResetEvent(false);
		}

		public void StopListeningForResponses()
		{
			_ReceivedMessageQueue.Clear();
			_MessageAvailableSignal.Dispose();
			_MessageAvailableSignal = new System.Threading.ManualResetEvent(false);
		}

		public void SendMessage(byte[] messageData, UdpEndPoint destination)
		{
			SentMessages.Enqueue(new ReceivedUdpData() { Buffer = messageData, ReceivedBytes = messageData.Length, ReceivedFrom = destination });
			SetMessageSentSignal();
		}

		private void SetMessageSentSignal()
		{
			_MessageSentSignalTimer.Change(40, System.Threading.Timeout.Infinite);
		}

		public void SendMulticastMessage(byte[] messageData)
		{
			SentBroadcasts.Enqueue(new ReceivedUdpData()
			{
				Buffer = messageData,
				ReceivedBytes = messageData.Length,
				ReceivedFrom = new UdpEndPoint()
				{
					IPAddress = SsdpConstants.MulticastLocalAdminAddress,
					Port = SsdpConstants.MulticastPort
				}
			});

			if (_ListenTask == null)
			{
				_ListenTask = Task.Run(() =>
				{
					try
					{
						while (!this.IsDisposed)
						{
							_MessageAvailableSignal.WaitOne();

							if (this.IsDisposed) break;

							while (_ReceivedMessageQueue.Any())
							{
								if (this.IsDisposed) break;

								var data = _ReceivedMessageQueue.Dequeue();
								Action processWork = () => ProcessMessage(System.Text.UTF8Encoding.UTF8.GetString(data.Buffer, 0, data.ReceivedBytes), data.ReceivedFrom);
								var processTask = TaskEx.Run(processWork);
							}
							_MessageAvailableSignal.Reset();
						}
					}
					catch (ObjectDisposedException) { }
					finally
					{
						_ListenTask = null;
					}
				});
			}

			_BroadcastSentSignalTimer.Change(50, System.Threading.Timeout.Infinite);
			//_SentBroadcastSignal.Set();
		}

		public bool IsShared
		{
			get;
			set;
		}

		#endregion

		public void MockReceiveBroadcast(ReceivedUdpData broadcastMessage)
		{
			if (!_ReceivedBroadcastsQueue.Any())
				_BroadcastAvailableSignal.Reset();

			_ReceivedBroadcastsQueue.Enqueue(broadcastMessage);
			_BroadcastAvailableSignal.Set();
		}

		public void MockReceiveMessage(ReceivedUdpData message)
		{
			if (!_ReceivedMessageQueue.Any())
				_MessageAvailableSignal.Reset();

			_ReceivedMessageQueue.Enqueue(message);
			_MessageAvailableSignal.Set();
		}

		public void WaitForMockBroadcast(int timeoutMilliseconds)
		{
			if (!SentBroadcasts.Any())
				_SentBroadcastSignal.Reset();

			_SentBroadcastSignal.WaitOne(timeoutMilliseconds);
			_SentBroadcastSignal.Reset();
		}

		public bool WaitForMockMessage(int timeoutMilliseconds)
		{
			if (!SentMessages.Any())
				_SentMessageSignal.Reset();

			var retVal = _SentMessageSignal.WaitOne(timeoutMilliseconds);
			_SentMessageSignal.Reset();
			return retVal;
		}

		public bool WaitForMessageToProcess(int timeoutMillseconds)
		{
			return _MessageProcessedSignal.WaitOne(timeoutMillseconds);
		}

		private void ProcessMessage(string data, UdpEndPoint endPoint)
		{
			//Responses start with the HTTP version, prefixed with HTTP/ while
			//requests start with a method which can vary and might be one we haven't 
			//seen/don't know. We'll check if this message is a request or a response
			//by checking for the static HTTP/ prefix on the start of the message.
			if (data.StartsWith("HTTP/", StringComparison.OrdinalIgnoreCase))
			{
				HttpResponseMessage responseMessage = null;
				try
				{
					responseMessage = _ResponseParser.Parse(data);
				}
				catch (ArgumentException) { } // Ignore invalid packets.

				if (responseMessage != null)
					OnResponseReceived(responseMessage, endPoint);
			}
			else
			{
				HttpRequestMessage requestMessage = null;
				try
				{
					requestMessage = _RequestParser.Parse(data);
				}
				catch (ArgumentException) { } // Ignore invalid packets.

				if (requestMessage != null)
					OnRequestReceived(requestMessage, endPoint);
			}
			_MessageProcessedSignal.Set();
		}

		private void OnRequestReceived(HttpRequestMessage data, UdpEndPoint endPoint)
		{
			//SSDP specification says only * is currently used but other uri's might
			//be implemented in the future and should be ignored unless understood.
			//Section 4.2 - http://tools.ietf.org/html/draft-cai-ssdp-v1-03#page-11
			if (data.RequestUri.ToString() != "*") return;

			var handlers = this.RequestReceived;
			if (handlers != null)
				handlers(this, new RequestReceivedEventArgs(data, endPoint));
		}

		private void OnResponseReceived(HttpResponseMessage data, UdpEndPoint endPoint)
		{
			var handlers = this.ResponseReceived;
			if (handlers != null)
				handlers(this, new ResponseReceivedEventArgs(data, endPoint));
		}

	}
}