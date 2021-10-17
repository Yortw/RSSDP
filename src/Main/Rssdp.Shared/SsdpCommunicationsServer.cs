﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Rssdp.Infrastructure
{
  /// <summary>
  /// Provides the platform independent logic for publishing device existence and responding to search requests.
  /// </summary>
  public sealed class SsdpCommunicationsServer : DisposableManagedObjectBase, ISsdpCommunicationsServer
  {

    #region Fields

    /* 		  
		 We could technically use one socket listening on port 1900 for everything.
		 This should get both multicast (notifications) and unicast (search response) messages, however 
		 this often doesn't work under Windows because the MS SSDP service is running. If that service 
		 is running then it will steal the unicast messages and we will never see search responses. 
		 Since stopping the service would be a bad idea (might not be allowed security wise and might 
		 break other apps running on the system) the only other work around is to use two sockets.
		
		 We use one socket to listen for/receive notifications and search requests (_BroadcastListenSocket).
		 We use a second socket, bound to a different local port, to send search requests and listen for 
		 responses (_SendSocket). The responses  are sent to the local port this socket is bound to, 
		 which isn't port 1900 so the MS service doesn't steal them. While the caller can specify a  local 
		 port to use, we will default to 0 which allows the underlying system to auto-assign a free port.
		  
		*/

    private readonly object _BroadcastListenSocketSynchroniser = new object();
    private IUdpSocket _BroadcastListenSocket;

    private readonly object _SendSocketSynchroniser = new object();
    private IUdpSocket _SendSocket;

    private readonly HttpRequestParser _RequestParser;
    private readonly HttpResponseParser _ResponseParser;

    private readonly ISocketFactory _SocketFactory;

    private readonly int _LocalPort;
    private readonly int _MulticastTtl;

    #endregion

    #region Events

    /// <summary>
    /// Raised when a HTTPU request message is received by a socket (unicast or multicast).
    /// </summary>
    public event EventHandler<RequestReceivedEventArgs> RequestReceived;

    /// <summary>
    /// Raised when an HTTPU response message is received by a socket (unicast or multicast).
    /// </summary>
    public event EventHandler<ResponseReceivedEventArgs> ResponseReceived;

    #endregion

    #region Public Properties

    /// <summary>
    /// The number of times the Udp message is sent. Any value less than 2 will result in one message being sent. SSDP spec recommends sending messages multiple times (not more than 3) to account for possible packet loss over UDP.
    /// </summary>
    /// <seealso cref="UdpSendDelay"/>
    public int UdpSendCount { get; set; } = SsdpConstants.DefaultUdpResendCount;

    /// <summary>
    /// The delay between repeating messages (as specified in UdpSendCount).
    /// </summary>
    /// <seealso cref="UdpSendCount"/>
    public TimeSpan UdpSendDelay { get; set; } = SsdpConstants.DefaultUdpResendDelay;

    #endregion

    #region Constructors

    /// <summary>
    /// Minimum constructor.
    /// </summary>
    /// <param name="socketFactory">An implementation of the <see cref="ISocketFactory"/> interface that can be used to make new unicast and multicast sockets. Cannot be null.</param>
    /// <exception cref="System.ArgumentNullException">The <paramref name="socketFactory"/> argument is null.</exception>
    public SsdpCommunicationsServer(ISocketFactory socketFactory)
      : this(socketFactory, 0, SsdpConstants.SsdpDefaultMulticastTimeToLive)
    {
    }

    /// <summary>
    /// Partial constructor.
    /// </summary>
    /// <param name="socketFactory">An implementation of the <see cref="ISocketFactory"/> interface that can be used to make new unicast and multicast sockets. Cannot be null.</param>
    /// <param name="localPort">The specific local port to use for all sockets created by this instance. Specify zero to indicate the system should choose a free port itself.</param>
    /// <exception cref="System.ArgumentNullException">The <paramref name="socketFactory"/> argument is null.</exception>
    public SsdpCommunicationsServer(ISocketFactory socketFactory, int localPort)
      : this(socketFactory, localPort, SsdpConstants.SsdpDefaultMulticastTimeToLive)
    {
    }

    /// <summary>
    /// Full constructor.
    /// </summary>
    /// <param name="socketFactory">An implementation of the <see cref="ISocketFactory"/> interface that can be used to make new unicast and multicast sockets. Cannot be null.</param>
    /// <param name="localPort">The specific local port to use for all sockets created by this instance. Specify zero to indicate the system should choose a free port itself.</param>
    /// <param name="multicastTimeToLive">The multicast time to live value for multicast sockets. Technically this is a number of router hops, not a 'Time'. Must be greater than zero.</param>
    /// <exception cref="System.ArgumentNullException">The <paramref name="socketFactory"/> argument is null.</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">The <paramref name="multicastTimeToLive"/> argument is less than or equal to zero.</exception>
    public SsdpCommunicationsServer(ISocketFactory socketFactory, int localPort, int multicastTimeToLive)
    {
      if (socketFactory == null) throw new ArgumentNullException("socketFactory");
      if (multicastTimeToLive <= 0) throw new ArgumentOutOfRangeException("multicastTimeToLive", "multicastTimeToLive must be greater than zero.");

      _BroadcastListenSocketSynchroniser = new object();
      _SendSocketSynchroniser = new object();

      _LocalPort = localPort;
      _SocketFactory = socketFactory;

      _RequestParser = new HttpRequestParser();
      _ResponseParser = new HttpResponseParser();

      _MulticastTtl = multicastTimeToLive;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Causes the server to begin listening for multicast messages, being SSDP search requests and notifications.
    /// </summary>
    /// <exception cref="System.ObjectDisposedException">Thrown if the <see cref="DisposableManagedObjectBase.IsDisposed"/> property is true (because <seealso cref="DisposableManagedObjectBase.Dispose()" /> has been called previously).</exception>
    public void BeginListeningForBroadcasts()
    {
      ThrowIfDisposed();

      if (_BroadcastListenSocket == null)
      {
        lock (_BroadcastListenSocketSynchroniser)
        {
          if (_BroadcastListenSocket == null)
            _BroadcastListenSocket = ListenForBroadcastsAsync();
        }
      }
    }

    /// <summary>
    /// Causes the server to stop listening for multicast messages, being SSDP search requests and notifications.
    /// </summary>
    /// <exception cref="System.ObjectDisposedException">Thrown if the <see cref="DisposableManagedObjectBase.IsDisposed"/> property is true (because <seealso cref="DisposableManagedObjectBase.Dispose()" /> has been called previously).</exception>
    public void StopListeningForBroadcasts()
    {
      ThrowIfDisposed();

      lock (_BroadcastListenSocketSynchroniser)
      {
        if (_BroadcastListenSocket != null)
        {
          _BroadcastListenSocket.Dispose();
          _BroadcastListenSocket = null;
        }
      }
    }

    /// <summary>
    /// Sends a message to a particular address (uni or multicast) and port.
    /// </summary>
    /// <param name="messageData">A byte array containing the data to send.</param>
    /// <param name="destination">A <see cref="UdpEndPoint"/> representing the destination address for the data. Can be either a multicast or unicast destination.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="messageData"/> argument is null.</exception>
    /// <exception cref="System.ObjectDisposedException">Thrown if the <see cref="DisposableManagedObjectBase.IsDisposed"/> property is true (because <seealso cref="DisposableManagedObjectBase.Dispose()" /> has been called previously).</exception>
    public void SendMessage(byte[] messageData, UdpEndPoint destination)
    {
      if (messageData == null) throw new ArgumentNullException("messageData");

      ThrowIfDisposed();

      EnsureSendSocketCreated();

      // SSDP spec recommends sending messages multiple times (not more than 3) to account for possible packet loss over UDP.
      Repeat(UdpSendCount, UdpSendDelay, () =>
        {
          SendMessageIfSocketNotDisposed(messageData, destination);
        });
    }

    /// <summary>
    /// Stops listening for search responses on the local, unicast socket.
    /// </summary>
    /// <exception cref="System.ObjectDisposedException">Thrown if the <see cref="DisposableManagedObjectBase.IsDisposed"/> property is true (because <seealso cref="DisposableManagedObjectBase.Dispose()" /> has been called previously).</exception>
    public void StopListeningForResponses()
    {
      ThrowIfDisposed();

      lock (_SendSocketSynchroniser)
      {
        var socket = _SendSocket;
        _SendSocket = null;
        if (socket != null)
          socket.Dispose();
      }
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets or sets a boolean value indicating whether or not this instance is shared amongst multiple <see cref="SsdpDeviceLocatorBase"/> and/or <see cref="ISsdpDevicePublisher"/> instances.
    /// </summary>
    /// <remarks>
    /// <para>If true, disposing an instance of a <see cref="SsdpDeviceLocatorBase"/>or a <see cref="ISsdpDevicePublisher"/> will not dispose this comms server instance. The calling code is responsible for managing the lifetime of the server.</para>
    /// </remarks>
    public bool IsShared { get; set; }

    /// <summary>
    /// What type of sockets will be created: ipv6 or ipv4
    /// </summary>
    public DeviceNetworkType DeviceNetworkType { get { return _SocketFactory.DeviceNetworkType; } }

    #endregion

    #region Overrides

    /// <summary>
    /// Stops listening for requests, disposes this instance and all internal resources.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);

      if (disposing)
      {
        lock (_BroadcastListenSocketSynchroniser)
        {
          if (_BroadcastListenSocket != null)
            _BroadcastListenSocket.Dispose();
        }

        lock (_SendSocketSynchroniser)
        {
          if (_SendSocket != null)
            _SendSocket.Dispose();
        }
      }
    }

    #endregion

    #region Private Methods

    private void SendMessageIfSocketNotDisposed(byte[] messageData, UdpEndPoint destination)
    {
      var socket = _SendSocket;
      if (socket != null)
      {
        _SendSocket.SendTo(messageData, destination);
      }
      else
      {
        ThrowIfDisposed();
      }
    }

    private static void Repeat(int repetitions, TimeSpan delay, Action work)
    {
      if (repetitions < 2)
        repetitions = 1;

      for (int cnt = 0; cnt < repetitions; cnt++)
      {
        work();

        if (delay != TimeSpan.Zero)
          TaskEx.Delay(delay).Wait();
      }
    }

    private IUdpSocket ListenForBroadcastsAsync()
    {
      var socket = _SocketFactory.CreateUdpMulticastSocket(_MulticastTtl, SsdpConstants.MulticastPort);

      _ = ListenToSocket(socket);

      return socket;
    }

    private IUdpSocket CreateSocketAndListenForResponsesAsync()
    {
      _SendSocket = _SocketFactory.CreateUdpSocket(_LocalPort);

      _ = ListenToSocket(_SendSocket);

      return _SendSocket;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "t", Justification = "Capturing task to local variable removes compiler warning, task is not otherwise required.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    private async Task ListenToSocket(IUdpSocket socket)
    {
      // Tasks are captured to local variables even if we don't use them just to avoid compiler warnings.
      try
      {
        await TaskEx.Run(async () =>
        {

          var cancelled = false;
          while (!cancelled)
          {
            try
            {
              var result = await socket.ReceiveAsync().ConfigureAwait(false);

              if (result.ReceivedBytes > 0)
              {
                // Strange cannot convert compiler error here if I don't explicitly
                // assign or cast to Action first. Assignment is easier to read,
                // so went with that.
                Action processWork = () => ProcessMessage(Encoding.UTF8.GetString(result.Buffer, 0, result.ReceivedBytes), result.ReceivedFrom);
                var processTask = TaskEx.Run(processWork);
              }
            }
            catch (SocketClosedException)
            {
              if (this.IsDisposed) return; //No error or reconnect if we're shutdown.

              await ReconnectBroadcastListeningSocket();
              cancelled = true;
              break;
            }
            catch (Exception)
            {
              cancelled = true;
            }
          }
        });
      }
      catch
      {
        if (this.IsDisposed) return;

        await ReconnectBroadcastListeningSocket();
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private async Task ReconnectBroadcastListeningSocket()
    {
      var success = false;
      while (!success)
      {
        try
        {
          var oldSocket = _BroadcastListenSocket;

          _BroadcastListenSocket = null;
          BeginListeningForBroadcasts();

          try
          {
            oldSocket?.Dispose();
          }
          catch
          {
            // Empty
          }

          success = true;
          break;
        }
        catch
        {
          await TaskEx.Delay(30000);
        }
      }
    }

    private void EnsureSendSocketCreated()
    {
      if (_SendSocket == null)
      {
        lock (_SendSocketSynchroniser)
        {
          if (_SendSocket == null)
            _SendSocket = CreateSocketAndListenForResponsesAsync();
        }
      }
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
        catch (FormatException) { } // Ignore invalid packets.

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
        catch (FormatException) { } // Ignore invalid packets.

        if (requestMessage != null)
          OnRequestReceived(requestMessage, endPoint);
      }
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

    #endregion

  }
}