using System;
using Rssdp.Infrastructure;

namespace Rssdp
{
	/// <summary>
	/// Used by RSSDP components to create implementations of the <see cref="ISsdpDeviceLocator"/> interface
	/// </summary>
	public sealed class SsdpDeviceLocatorFactory : ISsdpDeviceLocatorFactory
	{
		/// <summary>
		/// Creates a new <see cref="SsdpDeviceLocator"/> with specified address and port
		/// </summary>
		/// <param name="ipAddress">local ip address</param>
		/// <param name="port">local port</param>
		/// <exception cref="InvalidOperationException">If ip address is null or empty</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ipAddress")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public ISsdpDeviceLocator Create(string ipAddress, int port)
		{
			if (string.IsNullOrEmpty(ipAddress)) throw new InvalidOperationException("ipAddress");

			var socketFactory = new SocketFactory(ipAddress);
			var ssdpCommunicationsServer = new SsdpCommunicationsServer(socketFactory, port);
			var deviceLocator = new SsdpDeviceLocator(ssdpCommunicationsServer);
			return deviceLocator;
		}
	}
}