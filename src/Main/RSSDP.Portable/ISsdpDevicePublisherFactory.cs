using System;
using Rssdp.Infrastructure;

namespace Rssdp
{
	/// <summary>
	/// Used by RSSDP components to create implementations of the <see cref="ISsdpDevicePublisher"/> interface
	/// </summary>
	public interface ISsdpDevicePublisherFactory
	{
		/// <summary>
		/// Creates a new <see cref="ISsdpDevicePublisher"/> with specified address and port
		/// </summary>
		/// <param name="ipAddress">local ip address</param>
		/// <param name="port">local port</param>
		/// <exception cref="InvalidOperationException">If ip address is null or empty</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "ip")]
		ISsdpDevicePublisher Create(string ipAddress, int port);
	}
}
