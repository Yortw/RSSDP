using System;
using Rssdp.Infrastructure;

namespace Rssdp
{
	/// <summary>
	/// Used by RSSDP components to create implementations of the <see cref="ISsdpDeviceLocator"/> interface
	/// </summary>
	public interface ISsdpDeviceLocatorFactory
	{
		/// <summary>
		/// Creates a new <see cref="ISsdpDeviceLocator"/> with specified address and port
		/// </summary>
		/// <param name="ipAddress">local ip address</param>
		/// <param name="port">local port</param>
		/// <exception cref="InvalidOperationException">If ip address is null or empty</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "ip")]
		ISsdpDeviceLocator Create(string ipAddress, int port);
	}
}