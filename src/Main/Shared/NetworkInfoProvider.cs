using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Rssdp.Network;

namespace Rssdp
{
	// THIS IS A LINKED FILE - SHARED AMONGST MULTIPLE PLATFORMS	
	// Be careful to check any changes compile and work for all platform projects it is shared in.

	/// <summary>
	/// Provides a list of addresses for creating publishers and locators
	/// </summary>
	public sealed class NetworkInfoProvider : INetworkInfoProvider
	{
		/// <summary>
		/// Provides a list of addresses
		/// </summary>
		/// <remarks> Only if the adapter supports multicast and for the following types: Ethernet, Wireless80211</remarks>
		public IEnumerable<string> GetIpAddressesFromAdapters()
		{
			var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

			foreach (var networkInterface in networkInterfaces)
			{
				if (networkInterface.OperationalStatus != OperationalStatus.Up)
					continue;

				if (!networkInterface.SupportsMulticast)
					continue;

				if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
					networkInterface.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
					continue;

				var unciastAddress = GetAnyUnicastAddress(networkInterface, AddressFamily.InterNetwork);
				if (!unciastAddress.Equals(default(IPAddress)))
					yield return unciastAddress.ToString();

				unciastAddress = GetAnyUnicastAddress(networkInterface, AddressFamily.InterNetworkV6);
				if (!unciastAddress.Equals(default(IPAddress)))
					yield return unciastAddress.ToString();
			}
		}

		private static IPAddress GetAnyUnicastAddress(NetworkInterface networkInterface, AddressFamily addressFamily)
		{
			var interfaceProperties = networkInterface.GetIPProperties();
			var unicastAddresses = interfaceProperties.UnicastAddresses;

			return unicastAddresses.Where(ipAddressInfo => ipAddressInfo.Address.AddressFamily == addressFamily &&
			!IPAddress.IsLoopback(ipAddressInfo.Address))
				.Select(addressInfo => addressInfo.Address)
				.FirstOrDefault();
		}
	}
}
