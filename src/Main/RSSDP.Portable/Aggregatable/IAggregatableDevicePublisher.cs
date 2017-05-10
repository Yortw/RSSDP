using System;
using System.Collections.Generic;
using Rssdp.Infrastructure;

namespace Rssdp.Aggregatable
{
	/// <summary>
	/// Creates a <see cref="ISsdpDevicePublisher"/> for each available network interface
	/// </summary>
	public interface IAggregatableDevicePublisher : IDisposable
	{
		/// <summary>
		/// Provides all instances of created <see cref="ISsdpDevicePublisher"/>
		/// </summary>
		IEnumerable<ISsdpDevicePublisher> Publishers { get; }

		/// <summary>
		/// Provides all added <see cref="SsdpRootDevice"/> for each <see cref="ISsdpDevicePublisher"/>
		/// </summary>
		IEnumerable<SsdpRootDevice> Devices { get; }

		/// <summary>
		/// Add <see cref="SsdpRootDevice"/> to each <see cref="ISsdpDevicePublisher"/>
		/// </summary>
		/// <param name="ssdpRootDevice"></param>
		void AddDevice(SsdpRootDevice ssdpRootDevice);

		/// <summary>
		/// Remove <see cref="SsdpRootDevice"/> from each <see cref="ISsdpDevicePublisher"/>
		/// </summary>
		/// <param name="ssdpRootDevice"></param>
		void RemoveDevice(SsdpRootDevice ssdpRootDevice);
	}
}