using Rssdp.Infrastructure;

namespace Rssdp
{
	// THIS IS A STUB FILE
	/// <summary>
	/// Used by RSSDP components to create implementations of the <see cref="IUdpSocket"/> interface, to perform platform agnostic socket communications.
	/// </summary>
	public sealed class SocketFactory : ISocketFactory
	{

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="localIP">A string containing the IP address of the local network adapter to bind sockets to. Null or empty string will use IPAddress.Any.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "localIP")]
		public SocketFactory(string localIP)
		{
			throw PCL.StubException;
		}


		/// <summary>
		/// Creates a new UDP socket that is a member of the SSDP multicast local admin group and binds it to the specified local port.
		/// </summary>
		/// <param name="localPort">An integer specifying the local port to bind the socket to.</param>
		/// <returns>An implementation of the <see cref="IUdpSocket"/> interface used by RSSDP components to perform socket operations.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The purpose of this method is to create and returns a disposable result, it is up to the caller to dispose it when they are done with it.")]
		public IUdpSocket CreateUdpSocket(int localPort)
		{
			throw PCL.StubException;
		}

		/// <summary>
		/// Creates a new UDP socket that is a member of the specified multicast IP address, and binds it to the specified local port.
		/// </summary>
		/// <param name="ipAddress">The multicast IP address to make the socket a member of.</param>
		/// <param name="multicastTimeToLive">The multicase time to live value for the socket.</param>
		/// <param name="localPort">The number of the local port to bind to.</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "ip"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The purpose of this method is to create and returns a disposable result, it is up to the caller to dispose it when they are done with it.")]
		public IUdpSocket CreateUdpMulticastSocket(string ipAddress, int multicastTimeToLive, int localPort)
		{
			throw PCL.StubException;
		}
	}
}