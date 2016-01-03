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
	internal sealed class SocketFactory : ISocketFactory
	{

		private string _LocalIPAddress;

		public SocketFactory(string localIPAddress)
		{
			_LocalIPAddress = localIPAddress;
		}

		#region ISocketFactory Members

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "ip", Justification="Well understood and known abbreviation, it's ok, really it is."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The purpose of this method is to create and return a value that happens to be disposable, calling code is responsible for lifetime of result, not us.")]
		public IUdpSocket CreateUdpMulticastSocket(string ipAddress, int multicastTimeout, int localPort)
		{ 
			//Can't set multicast timeout on WP AnySourceUdpClient, so don't bother passing it (still required as argument to method by interface though).
			return new MulticastUdpSocket(localPort);
		}

		#endregion
	}
}