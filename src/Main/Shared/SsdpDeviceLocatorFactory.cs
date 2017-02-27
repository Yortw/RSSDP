using System;
using Rssdp.Aggregatable;
using Rssdp.Infrastructure;

namespace Rssdp
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class SsdpDeviceLocatorFactory : ISsdpDeviceLocatorFactory
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ipAddress"></param>
		/// <param name="port"></param>
		/// <exception cref="InvalidOperationException"></exception>
		/// <returns></returns>
		public ISsdpDeviceLocator Create(string ipAddress, int port)
		{
			if (string.IsNullOrEmpty(ipAddress)) throw new InvalidOperationException("ipAddress");

			var deviceLocator = new SsdpDeviceLocator(new SsdpCommunicationsServer(new SocketFactory(ipAddress), port));
			return deviceLocator;
		}
	}
}