using Rssdp.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rssdp
{
	public class SocketFactory : ISocketFactory
	{
		public IUdpSocket CreateUdpMulticastSocket(string ipAddress, int multicastTimeToLive, int localPort)
		{
			return new UwaUdpSocket(ipAddress, multicastTimeToLive, localPort);
		}

		public IUdpSocket CreateUdpSocket(int localPort)
		{
			return new UwaUdpSocket(localPort);
		}
	}
}