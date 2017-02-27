using Rssdp.Infrastructure;

namespace Rssdp
{
	/// <summary>
	/// 
	/// </summary>
	public interface ISsdpDevicePublisherFactory
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ipAddress"></param>
		/// <param name="port"></param>
		/// <returns></returns>
		ISsdpDevicePublisher Create(string ipAddress, int port);
	}
}
