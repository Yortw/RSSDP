using System.Collections.Generic;

namespace Rssdp.Network
{
	/// <summary>
	/// 
	/// </summary>
	public interface INetworkInfoProvider
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		IEnumerable<string> GetIpAddressesFromAdapters();
	}
}
