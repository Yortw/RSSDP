using System;

namespace Rssdp.Infrastructure
{
	/// <summary>
	/// Used by the sockets wrapper to hold raw data received from a UDP socket.
	/// </summary>
	public sealed class ReceivedUdpData
	{
		/// <summary>
		/// Full constructor.
		/// </summary>
		/// <param name="receivedFrom">A <see cref="UdpEndPoint"/> representing the address and port the data was received from.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="receivedFrom"/> is null.</exception>
		public ReceivedUdpData(UdpEndPoint receivedFrom)
		{
			if (receivedFrom == null) throw new ArgumentNullException(nameof(receivedFrom));

			this.ReceivedFrom = receivedFrom;
		}

		/// <summary>
		/// The buffer to place received data into.
		/// </summary>
		public byte[] Buffer { get; set; } = [];

		/// <summary>
		/// The number of bytes received.
		/// </summary>
		public int ReceivedBytes { get; set; }

		/// <summary>
		/// The <see cref="UdpEndPoint"/> the data was received from.
		/// </summary>
		public UdpEndPoint ReceivedFrom { get; private set; }
	}
}
