using System;
using Rssdp.Infrastructure;

namespace Rssdp
{
	/// <summary>
	/// Used by RSSDP components to create implementations of the <see cref="ISsdpDevicePublisher"/> interface
	/// </summary>
	public sealed class SsdpDevicePublisherFactory : ISsdpDevicePublisherFactory
	{
		/// <summary>
		/// Creates a new <see cref="SsdpDevicePublisher"/> with specified address and port
		/// </summary>
		/// <param name="ipAddress">local ip address</param>
		/// <param name="port">local port</param>
		/// <exception cref="InvalidOperationException">If ip address is null or empty</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ipAddress")]
		public ISsdpDevicePublisher Create(string ipAddress, int port)
		{
			if (string.IsNullOrEmpty(ipAddress)) throw new InvalidOperationException("ipAddress");

			var ssdpDevicePublisher = new SsdpDevicePublisher(ipAddress, port);
			return ssdpDevicePublisher;
		}
	}
}
