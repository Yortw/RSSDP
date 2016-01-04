using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Rssdp.Infrastructure;

namespace Rssdp
{
	// Not entirely happy with this. Would have liked to have done something more generic/reusable,
	// but that wasn't really the point so kept to YAGNI principal for now, even if the 
	// interfaces are a bit ugly, specific and make assumptions.

	/// <summary>
	/// Used by RSSDP components to create implementations of the <see cref="IUdpSocket"/> interface, to perform platform agnostic socket communications.
	/// </summary>
	public sealed class SocketFactory : ISocketFactory
	{

		private string _LocalIPAddress;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="localIP">A string containing the IP address of the local network adapter to bind sockets to. Null or empty string will use <see cref="IPAddress.Any"/>.</param>
		public SocketFactory(string localIP)
		{
			_LocalIPAddress = localIP;
		}

		#region ISocketFactory Members

		/// <summary>
		/// Creates a new UDP socket that is a member of the SSDP multicast local admin group and binds it to the specified local port.
		/// </summary>
		/// <param name="localPort">An integer specifying the local port to bind the socket to.</param>
		/// <returns>An implementation of the <see cref="IUdpSocket"/> interface used by RSSDP components to perform socket operations.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification="The purpose of this method is to create and return a value that happens to be disposable, calling code is responsible for lifetime of result, not us.")]
		public IUdpSocket CreateUdpSocket(int localPort)
		{
			var netSocket = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
			try
			{
				return new UdpSocket(netSocket, localPort, _LocalIPAddress);
			}
			catch
			{
				if (netSocket != null)
					netSocket.Dispose();

				throw;
			}
		}

		/// <summary>
		/// Creates a new UDP socket that is a member of the specified multicast IP address, and binds it to the specified local port.
		/// </summary>
		/// <param name="ipAddress">The multicast IP address to make the socket a member of.</param>
		/// <param name="multicastTimeToLive">The multicase time to live value for the socket.</param>
		/// <param name="localPort">The number of the local port to bind to.</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "ip", Justification="Well understood and known abbreviation, it's ok, really it is."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The purpose of this method is to create and return a value that happens to be disposable, calling code is responsible for lifetime of result, not us.")]
		public IUdpSocket CreateUdpMulticastSocket(string ipAddress, int multicastTimeToLive, int localPort)
		{ 
			//Can't set multicast timeout on WP AnySourceUdpClient, so don't bother passing it (still required as argument to method by interface though).
			return new MulticastUdpSocket(localPort);
		}

		#endregion
	}
}