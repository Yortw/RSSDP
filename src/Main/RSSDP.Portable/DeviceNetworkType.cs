namespace Rssdp
{
	/// <summary>
	/// What type of sockets will be created: ipv6 or ipv4
	/// </summary>
	public enum DeviceNetworkType
	{
		/// <summary>
		/// Equals to AddressFamily.InternetNetwork
		/// </summary>
		Ipv4,
		/// <summary>
		/// Equals to AddressFamily.InternetNetworkV6
		/// </summary>
		Ipv6
	}
}
