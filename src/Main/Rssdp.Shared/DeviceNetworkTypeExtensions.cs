using System;
using Rssdp.Infrastructure;

namespace Rssdp
{
	/// <summary>
	/// Provides extensions to the <see cref="DeviceNetworkType"/> enum.
	/// </summary>
	public static class DeviceNetworkTypeExtensions
	{
		/// <summary>
		/// Get multicast ip address for ipv4 or ipv6 network by <see cref="DeviceNetworkType"/>
		/// </summary>
		/// <param name="deviceNetworkType"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static string GetMulticastIPAddress(this DeviceNetworkType deviceNetworkType)
		{
			string multicastIpAddress;

			switch (deviceNetworkType)
			{
				case DeviceNetworkType.IPv4:
					multicastIpAddress = SsdpConstants.MulticastLocalAdminAddress;
					break;

				case DeviceNetworkType.IPv6:
					multicastIpAddress = SsdpConstants.MulticastLinkLocalAddressV6;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(deviceNetworkType));
			}

			return multicastIpAddress;
		}
	}
}
