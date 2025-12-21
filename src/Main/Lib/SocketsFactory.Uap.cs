#if UAP || WINDOWS_UWP

using Rssdp.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rssdp
{
	/// <summary>
	/// Used by RSSDP components to create implementations of the <see cref="IUdpSocket"/> interface, to perform platform agnostic socket communications.
	/// </summary>
	public class SocketFactory : ISocketFactory
	{
		private readonly DeviceNetworkType _DeviceNetworkType;
		private string _LocalIP;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="localIP">A string containing the IP address of the local network adapter to bind sockets to. Null or empty string will use IPAddress.Any.</param>
		public SocketFactory(string localIP)
		{
			_DeviceNetworkType = DeviceNetworkType.IPv4;
			_LocalIP = localIP;
			if (!String.IsNullOrEmpty(localIP))
			{
				var hostName = new Windows.Networking.HostName(localIP);
				if (hostName.Type == Windows.Networking.HostNameType.Ipv6) _DeviceNetworkType = DeviceNetworkType.IPv6;
			}
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

		/// <summary>
		/// Creates a new UDP socket that is a member of the SSDP multicast local admin group and binds it to the specified local port.
		/// </summary>
		/// <param name="multicastTimeToLive"></param>
		/// <param name="localPort">An integer specifying the local port to bind the socket to.</param>
		/// <returns>An implementation of the <see cref="IUdpSocket"/> interface used by RSSDP components to perform socket operations.</returns>
		public IUdpSocket CreateUdpMulticastSocket(int multicastTimeToLive, int localPort)
		{
			return new UwaUdpSocket(SsdpConstants.MulticastLocalAdminAddress, multicastTimeToLive, localPort, _LocalIP);
		}

		/// <summary>
		/// What type of sockets will be created: ipv6 or ipv4
		/// For WinRT it will be IPv4
		/// </summary>
		public DeviceNetworkType DeviceNetworkType
		{
			get
			{
				return _DeviceNetworkType;
			}
		}
	}
}

#endif