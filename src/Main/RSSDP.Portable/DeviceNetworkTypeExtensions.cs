using System;
using Rssdp.Infrastructure;

namespace Rssdp
{
	public static class DeviceNetworkTypeExtensions
	{
		/// <summary>
		/// Get multicast ip address for ipv4 or ipv6 network by <see cref="DeviceNetworkType"/>
		/// </summary>
		/// <param name="deviceNetworkType"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static string GetMulticastIpAddress(this DeviceNetworkType deviceNetworkType)
		{
			string multicastIpAddress;

			switch (deviceNetworkType)
			{
				case DeviceNetworkType.Ipv4:
					multicastIpAddress = SsdpConstants.MulticastLocalAdminAddress;
					break;

				case DeviceNetworkType.Ipv6:
					multicastIpAddress = SsdpConstants.MulticastLinkLocalAddressV6;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(deviceNetworkType));
			}

			return multicastIpAddress;
		}
	}
}
