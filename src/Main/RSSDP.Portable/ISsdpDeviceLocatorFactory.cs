using Rssdp.Infrastructure;

namespace Rssdp
{
	/// <summary>
	/// 
	/// </summary>
	public interface ISsdpDeviceLocatorFactory
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ipAddress"></param>
		/// <param name="port"></param>
		/// <returns></returns>
		ISsdpDeviceLocator Create(string ipAddress, int port);
	}
}