using Rssdp.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rssdp
{
	// THIS IS A LINKED FILE - SHARED AMONGST MULTIPLE PLATFORMS	
	// Be careful to check any changes compile and work for all platform projects it is shared in.

	/// <summary>
	/// Used by RSSDP components to create implementations of the <see cref="IUdpSocket"/> interface, to perform platform agnostic socket communications.
	/// </summary>
	public class SocketFactory : ISocketFactory
	{
		private string _LocalIP;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="localIP">A string containing the IP address of the local network adapter to bind sockets to. Null or empty string will use IPAddress.Any.</param>
		public SocketFactory(string localIP)
		{
			_LocalIP = localIP;
		}

		/// <summary>
		/// Creates a new UDP socket that is a member of the specified multicast IP address, and binds it to the specified local port.
		/// </summary>
		/// <param name="ipAddress">The multicast IP address to make the socket a member of.</param>
		/// <param name="multicastTimeToLive">The multicast time to live value for the socket.</param>
		/// <param name="localPort">The number of the local port to bind to.</param>
		/// <returns></returns>
		public IUdpSocket CreateUdpMulticastSocket(string ipAddress, int multicastTimeToLive, int localPort)
		{
			return new UwaUdpSocket(ipAddress, multicastTimeToLive, localPort, _LocalIP);
		}

		/// <summary>
		/// Creates a new UDP socket that is a member of the SSDP multicast local admin group and binds it to the specified local port.
		/// </summary>
		/// <param name="localPort">An integer specifying the local port to bind the socket to.</param>
		/// <returns>An implementation of the <see cref="IUdpSocket"/> interface used by RSSDP components to perform socket operations.</returns>
		public IUdpSocket CreateUdpSocket(int localPort)
		{
			return new UwaUdpSocket(localPort, _LocalIP);
		}
	}
}