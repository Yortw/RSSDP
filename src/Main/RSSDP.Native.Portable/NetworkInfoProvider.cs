using System.Collections.Generic;
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
			throw PCL.StubException;
		}
	}
}
