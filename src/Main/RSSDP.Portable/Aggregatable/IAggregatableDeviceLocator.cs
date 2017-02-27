using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rssdp.Infrastructure;

namespace Rssdp.Aggregatable
{
	/// <summary>
	/// 
	/// </summary>
	public interface IAggregatableDeviceLocator : IDisposable
	{
		/// <summary>
		/// 
		/// </summary>
		event EventHandler<DeviceAvailableEventArgs> DeviceAvailable;

		/// <summary>
		/// 
		/// </summary>
		event EventHandler<DeviceUnavailableEventArgs> DeviceUnavailable;

		/// <summary>
		/// 
		/// </summary>
		IEnumerable<ISsdpDeviceLocator> Locators { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		Task<IEnumerable<DiscoveredSsdpDevice>> SearchAsync();

		/// <summary>
		/// 
		/// </summary>
		void StartListening();

		/// <summary>
		/// 
		/// </summary>
		void StopListening();
	}
}