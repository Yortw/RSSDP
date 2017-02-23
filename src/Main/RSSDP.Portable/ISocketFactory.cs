using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rssdp.Infrastructure
{
	/// <summary>
	/// Implemented by components that can create a platform specific UDP socket implementation, and wrap it in the cross platform <see cref="IUdpSocket"/> interface.
	/// </summary>
	public interface ISocketFactory
	{

		/// <summary>
		/// Creates a new unicast socket using the specified local port number.
		/// </summary>
		/// <param name="localPort">The local port to bind to.</param>
		/// <returns>A <see cref="IUdpSocket"/> implementation.</returns>
		IUdpSocket CreateUdpSocket(int localPort);

		/// <summary>
		/// Creates a new multicast socket using the specified multicast IP address, multicast time to live and local port.
		/// </summary>
		/// <param name="multicastTimeToLive">The multicast time to live value. Actually a maximum number of network hops for UDP packets.</param>
		/// <param name="localPort">The local port to bind to.</param>
		/// <returns>A <see cref="IUdpSocket"/> implementation.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "ip", Justification="IP is a well known and understood abbreviation and the full name is excessive.")]
		IUdpSocket CreateUdpMulticastSocket(int multicastTimeToLive, int localPort);

		/// <summary>
		/// What type of sockets will be created: ipv6 or ipv4
		/// </summary>
		DeviceNetworkType DeviceNetworkType { get; }
	}
}
