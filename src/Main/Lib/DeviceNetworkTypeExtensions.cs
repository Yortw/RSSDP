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
			var multicastIpAddress = deviceNetworkType switch
			{
				DeviceNetworkType.IPv4 => SsdpConstants.MulticastLocalAdminAddress,
				DeviceNetworkType.IPv6 => SsdpConstants.MulticastLinkLocalAddressV6,
				_ => throw new ArgumentOutOfRangeException(nameof(deviceNetworkType)),
			};
			return multicastIpAddress;
		}
	}
}
