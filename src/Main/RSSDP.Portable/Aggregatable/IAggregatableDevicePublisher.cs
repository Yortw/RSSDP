using System;
using System.Collections.Generic;
using Rssdp.Infrastructure;

namespace Rssdp.Aggregatable
{
	/// <summary>
	/// 
	/// </summary>
	public interface IAggregatableDevicePublisher : IDisposable
	{
		/// <summary>
		/// 
		/// </summary>
		IEnumerable<ISsdpDevicePublisher> Publishers { get; }

		/// <summary>
		/// 
		/// </summary>
		IEnumerable<SsdpRootDevice> Devices { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ssdpRootDevice"></param>
		void AddDevice(SsdpRootDevice ssdpRootDevice);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ssdpRootDevice"></param>
		void RemoveDevice(SsdpRootDevice ssdpRootDevice);
	}
}