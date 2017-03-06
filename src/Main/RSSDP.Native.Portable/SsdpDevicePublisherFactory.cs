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
		public ISsdpDevicePublisher Create(string ipAddress, int port)
		{
			throw PCL.StubException;
		}
	}
}
