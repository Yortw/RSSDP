using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using Rssdp.Infrastructure;

namespace Rssdp
{
	// THIS IS A LINKED FILE - SHARED AMONGST MULTIPLE PLATFORMS	
	// Be careful to check any changes compile and work for all platform projects it is shared in.

	// Not entirely happy with this. Would have liked to have done something more generic/reusable,
	// but that wasn't really the point so kept to YAGNI principal for now, even if the 
	// interfaces are a bit ugly, specific and make assumptions.
	internal sealed class SocketFactory : ISocketFactory
	{
		#region ISocketFactory Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification="The purpose of this method is to create and returns a disposable result, it is up to the caller to dispose it when they are done with it.")]
		public IUdpSocket CreateUdpSocket(int localPort)
		{
			if (localPort < 0) throw new ArgumentException("localPort cannot be less than zero.", "localPort");

			var retVal = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
			try
			{
				retVal.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				retVal.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, SsdpConstants.SsdpDefaultMulticastTimeToLive);
				retVal.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(SsdpConstants.MulticastLocalAdminAddress), IPAddress.Any));
				return new UdpSocket(retVal, localPort);
			}
			catch
			{
				if (retVal != null)
					retVal.Dispose();

				throw;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "ip"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The purpose of this method is to create and returns a disposable result, it is up to the caller to dispose it when they are done with it.")]
		public IUdpSocket CreateUdpMulticastSocket(string ipAddress, int multicastTimeout, int localPort)
		{
			if (ipAddress == null) throw new ArgumentNullException("ipAddress");
			if (ipAddress.Length == 0) throw new ArgumentException("ipAddress cannot be an empty string.", "ipAddress");
			if (localPort <= 0) throw new ArgumentException("multicastTimeout cannot be zero or less.", "localPort");
			if (localPort < 0) throw new ArgumentException("localPort cannot be less than zero.", "localPort");

			var retVal = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
	
			try
			{ 
			retVal.ExclusiveAddressUse = false;
			retVal.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			retVal.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, multicastTimeout);
			retVal.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(ipAddress), IPAddress.Any));
			retVal.MulticastLoopback = true;

			return new UdpSocket(retVal, localPort);
			}
			catch
			{
				if (retVal != null)
					retVal.Dispose();

				throw;
			}
		}

		#endregion
	}
}