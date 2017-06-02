using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security;
using System.Text;
using Rssdp.Infrastructure;

namespace Rssdp
{
	// THIS IS A LINKED FILE - SHARED AMONGST MULTIPLE PLATFORMS	
	// Be careful to check any changes compile and work for all platform projects it is shared in.

	// Not entirely happy with this. Would have liked to have done something more generic/reusable,
	// but that wasn't really the point so kept to YAGNI principal for now, even if the 
	// interfaces are a bit ugly, specific and make assumptions.

	/// <summary>
	/// Used by RSSDP components to create implementations of the <see cref="IUdpSocket"/> interface, to perform platform agnostic socket communications.
	/// </summary>
	public sealed class SocketFactory : ISocketFactory
	{
		private readonly DeviceNetworkType _DeviceNetworkType;
		private IPAddress _LocalIP;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="ipAddress">The IP address of the local network adapter to bind sockets to. 
		/// Null or empty string will use <see cref="IPAddress.Any"/>.</param>
		public SocketFactory(string ipAddress)
		{
			if (String.IsNullOrEmpty(ipAddress))
				_LocalIP = IPAddress.Any;
			else
				_LocalIP = IPAddress.Parse(ipAddress);

			_DeviceNetworkType = GetDeviceNetworkType(_LocalIP.AddressFamily);
		}

		#region ISocketFactory Members

		/// <summary>
		/// Creates a new UDP socket that is a member of the SSDP multicast local admin group and binds it to the specified local port.
		/// </summary>
		/// <param name="localPort">An integer specifying the local port to bind the socket to.</param>
		/// <returns>An implementation of the <see cref="IUdpSocket"/> interface used by RSSDP components to perform socket operations.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The purpose of this method is to create and returns a disposable result, it is up to the caller to dispose it when they are done with it.")]
		public IUdpSocket CreateUdpSocket(int localPort)
		{
			if (localPort < 0) throw new ArgumentException("localPort cannot be less than zero.", "localPort");

			var retVal = new Socket(_LocalIP.AddressFamily, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
			try
			{
				retVal.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

				SetMulticastSocketOptions(retVal, SsdpConstants.SsdpDefaultMulticastTimeToLive);

				return new UdpSocket(retVal, _LocalIP.ToString(), localPort);
			}
			catch
			{
				if (retVal != null)
					retVal.Dispose();

				throw;
			}
		}

		/// <summary>
		/// Creates a new UDP socket that is a member of the specified multicast IP address, and binds it to the specified local port.
		/// </summary>
		/// <param name="multicastTimeToLive">The multicast time to live value for the socket.</param>
		/// <param name="localPort">The number of the local port to bind to.</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "ip"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The purpose of this method is to create and returns a disposable result, it is up to the caller to dispose it when they are done with it.")]
		public IUdpSocket CreateUdpMulticastSocket(int multicastTimeToLive, int localPort)
		{
			if (multicastTimeToLive <= 0) throw new ArgumentException("multicastTimeToLive cannot be zero or less.", "multicastTimeToLive");
			if (localPort < 0) throw new ArgumentException("localPort cannot be less than zero.", "localPort");

			var retVal = new Socket(_LocalIP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

			try
			{
#if NETSTANDARD1_3
	// The ExclusiveAddressUse socket option is a Windows-specific option that, when set to "true," tells Windows not to allow another socket to use the same local address as this socket
	// See https://github.com/dotnet/corefx/pull/11509 for more details
				if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
				{
					retVal.ExclusiveAddressUse = false;
				}
#else
				retVal.ExclusiveAddressUse = false;
#endif
				retVal.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

				SetMulticastSocketOptions(retVal, multicastTimeToLive);

				retVal.MulticastLoopback = true;

				return new UdpSocket(retVal, _LocalIP.ToString(), localPort);
			}
			catch
			{
				if (retVal != null)
					retVal.Dispose();

				throw;
			}
		}

		/// <summary>
		/// What type of sockets will be created: ipv6 or ipv4
		/// </summary>
		public DeviceNetworkType DeviceNetworkType
		{
			get
			{
				return _DeviceNetworkType;
			}
		}

		#endregion

		/// <summary>
		/// Set options for multicast depending on the type of the local address
		/// </summary>
		/// <param name="retVal">Socket for setting options</param>
		/// <param name="multicastTimeToLive">Multicast Time to live for multicast options</param>
		/// <returns></returns>
		private void SetMulticastSocketOptions(Socket retVal, int multicastTimeToLive)
		{
			string multicastIpAddress = _DeviceNetworkType.GetMulticastIPAddress();
			IPAddress ipAddress = IPAddress.Parse(multicastIpAddress);

			switch (_DeviceNetworkType)
			{
				case DeviceNetworkType.IPv4:
					retVal.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, multicastTimeToLive);
					retVal.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ipAddress, _LocalIP));
					break;

				case DeviceNetworkType.IPv6:
					retVal.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastTimeToLive, multicastTimeToLive);
					long interfaceIndex = -1;

#if !NETSTANDARD
					if (_LocalIP != null & _LocalIP != IPAddress.IPv6Any)
						interfaceIndex = GetInterfaceIndexFromIPAddress(_LocalIP);
#endif

					if (interfaceIndex >= 0)
						retVal.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, new IPv6MulticastOption(ipAddress, interfaceIndex));
					else
						retVal.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, new IPv6MulticastOption(ipAddress));
					break;

				default:
					throw new InvalidOperationException($"{nameof(_DeviceNetworkType)} is not equal to Ipv4 or Ipv6");
			}
		}

#if !NETSTANDARD
		private static long GetInterfaceIndexFromIPAddress(IPAddress ipAddress)
		{
			foreach (var networkInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
			{
				if (networkInterface.Supports(NetworkInterfaceComponent.IPv6))
				{
					var ipProperties = networkInterface.GetIPProperties();
					if (ipProperties != null)
					{
						foreach (var address in ipProperties.UnicastAddresses)
						{
							if (address.Address?.ToString() == ipAddress.ToString())
							{
								var ipv6Properties = ipProperties?.GetIPv6Properties();
								return ipv6Properties?.Index ?? -1;
							}
						}
					}
				}
			}

			return -1;
		}
#endif

		private static DeviceNetworkType GetDeviceNetworkType(AddressFamily addressFamily)
		{
			switch (addressFamily)
			{
				case AddressFamily.InterNetwork:
					return DeviceNetworkType.IPv4;
				case AddressFamily.InterNetworkV6:
					return DeviceNetworkType.IPv6;
				default:
					throw new ArgumentOutOfRangeException(nameof(addressFamily), addressFamily, null);
			}
		}
	}
}