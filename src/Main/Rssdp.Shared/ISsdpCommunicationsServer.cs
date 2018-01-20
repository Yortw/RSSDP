using System;

namespace Rssdp.Infrastructure
{
	/// <summary>
	/// Interface for a component that manages network communication (sending and receiving HTTPU messages) for the SSDP protocol.
	/// </summary>
	public interface ISsdpCommunicationsServer : IDisposable
	{

		#region Events

		/// <summary>
		/// Raised when a HTTPU request message is received by a socket (unicast or multicast).
		/// </summary>
		event EventHandler<RequestReceivedEventArgs> RequestReceived;

		/// <summary>
		/// Raised when an HTTPU response message is received by a socket (unicast or multicast).
		/// </summary>
		event EventHandler<ResponseReceivedEventArgs> ResponseReceived;

		#endregion

		#region Methods

		/// <summary>
		/// Causes the server to begin listening for multicast messages, being SSDP search requests and notifications.
		/// </summary>
		void BeginListeningForBroadcasts();

		/// <summary>
		/// Causes the server to stop listening for multicast messages, being SSDP search requests and notifications.
		/// </summary>
		void StopListeningForBroadcasts();

		/// <summary>
		/// Stops listening for search responses on the local, unicast socket.
		/// </summary>
		void StopListeningForResponses();

		/// <summary>
		/// Sends a message to a particular address (uni or multicast) and port.
		/// </summary>
		/// <param name="messageData">A byte array containing the data to send.</param>
		/// <param name="destination">A <see cref="UdpEndPoint"/> representing the destination address for the data. Can be either a multicast or unicast destination.</param>
		void SendMessage(byte[] messageData, UdpEndPoint destination);

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets a boolean value indicating whether or not this instance is shared amongst multiple <see cref="SsdpDeviceLocatorBase"/> and/or <see cref="ISsdpDevicePublisher"/> instances.
		/// </summary>
		/// <remarks>
		/// <para>If true, disposing an instance of a <see cref="SsdpDeviceLocatorBase"/>or a <see cref="ISsdpDevicePublisher"/> will not dispose this comms server instance. The calling code is responsible for managing the lifetime of the server.</para>
		/// </remarks>
		bool IsShared { get; set; }

		/// <summary>
		/// Determines whether IPv4 or IPv5 sockets are used by this communications server.
		/// </summary>
		DeviceNetworkType DeviceNetworkType { get; }

		/// <summary>
		/// The number of times the Udp message is sent. Any value less than 2 will result in one message being sent. SSDP spec recommends sending messages multiple times (not more than 3) to account for possible packet loss over UDP.
		/// </summary>
		/// <seealso cref="UdpSendDelay"/>
		int UdpSendCount { get; set; }

		/// <summary>
		/// The delay between repeating messages (as specified in UdpSendCount).
		/// </summary>
		/// <seealso cref="UdpSendCount"/>
		TimeSpan UdpSendDelay { get; set; }

		#endregion

	}
}