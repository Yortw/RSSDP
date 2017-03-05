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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ip")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ip")]
		IEnumerable<string> GetIpAddressesFromAdapters();
	}
}
