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
		private string _LocalIP;

		public SocketFactory(string localIP)
		{
			_LocalIP = localIP;
		}

		public IUdpSocket CreateUdpMulticastSocket(string ipAddress, int multicastTimeToLive, int localPort)
		{
			return new UwaUdpSocket(ipAddress, multicastTimeToLive, localPort, _LocalIP);
		}

		public IUdpSocket CreateUdpSocket(int localPort)
		{
			return new UwaUdpSocket(localPort, _LocalIP);
		}
	}
}