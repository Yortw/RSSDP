using System;
using System.Diagnostics;
using System.Net;

namespace Rssdp.Infrastructure
{
	/// <summary>
	/// Cross platform representation of a UDP end point, being an IP address (either IPv4 or IPv6) and a port.
	/// </summary>
	[DebuggerDisplay("{IPAddress}:{Port}")]
	public sealed class UdpEndPoint
	{

		/// <summary>
		/// Full constructor.
		/// </summary>
		/// <param name="ipAddress">A string representation of the IP address.</param>
		/// <param name="port">The port number.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ipAddress"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="port"/> is not between 0 and 65535.</exception>
		public UdpEndPoint(string ipAddress, int port)
		{
			if (ipAddress == null) throw new ArgumentNullException(nameof(ipAddress));
			if (port < 0 || port > 65535) throw new ArgumentOutOfRangeException(nameof(port), "port must be between 0 and 65535.");

			this.IPAddress = ipAddress;
			this.Port = port;
		}

		/// <summary>
		/// The IP Address of the end point.
		/// </summary>
		/// <remarks>
		/// <para>Can be either IPv4 or IPv6, up to the code using this instance to determine which was provided.</para>
		/// </remarks>
		public string IPAddress { get; private set; }

		/// <summary>
		/// The port of the end point.
		/// </summary>
		public int Port { get; private set; }

		/// <summary>
		/// Returns the <see cref="IPAddress"/> and <see cref="Port"/> values separated by a colon.
		/// </summary>
		/// <returns>A string containing <see cref="IPAddress"/>:<see cref="Port"/>.</returns>
		public override string ToString()
		{
			return (this.IPAddress ?? String.Empty) + ":" + this.Port.ToString(System.Globalization.CultureInfo.InvariantCulture);
		}
	}
}