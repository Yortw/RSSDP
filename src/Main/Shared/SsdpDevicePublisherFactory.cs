using System;
using Rssdp.Aggregatable;
using Rssdp.Infrastructure;

namespace Rssdp
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class SsdpDevicePublisherFactory : ISsdpDevicePublisherFactory
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ipAddress"></param>
		/// <param name="port"></param>
		/// <exception cref="InvalidOperationException"></exception>
		/// <returns></returns>
		public ISsdpDevicePublisher Create(string ipAddress, int port)
		{
			if (string.IsNullOrEmpty(ipAddress)) throw new InvalidOperationException("ipAddress");

			var ssdpDevicePublisher = new SsdpDevicePublisher(ipAddress, port);
			return ssdpDevicePublisher;
		}
	}
}
