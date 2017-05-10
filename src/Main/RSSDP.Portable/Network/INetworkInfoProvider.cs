using System.Collections.Generic;

namespace Rssdp.Network
{
	/// <summary>
	/// Provides a list of addresses for creating publishers and locators
	/// </summary>
	public interface INetworkInfoProvider
	{
		/// <summary>
		/// Provides a list of addresses
		/// </summary>
		/// <remarks> Only if the adapter supports multicast and for the following types: Ethernet, Wireless80211</remarks>
		IEnumerable<string> GetIpAddressesFromAdapters();
	}
}
