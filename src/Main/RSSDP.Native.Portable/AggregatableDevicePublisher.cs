using System;
using System.Collections.Generic;
using Rssdp.Infrastructure;
using Rssdp.Network;

namespace Rssdp.Aggregatable
{
	/// <summary>
	/// Creates a <see cref="ISsdpDevicePublisher"/> for each available network interface
	/// </summary>
	public sealed class AggregatableDevicePublisher : IAggregatableDevicePublisher
	{
		private readonly IList<ISsdpDevicePublisher> _ssdpDevicePublisherses = new List<ISsdpDevicePublisher>();

		/// <param name="networkInfoProvider"></param>
		/// <param name="ssdpDevicePublisherFactory"></param>
		/// <param name="port"></param>
		public AggregatableDevicePublisher(INetworkInfoProvider networkInfoProvider,
			ISsdpDevicePublisherFactory ssdpDevicePublisherFactory,
			int port)
		{
			throw PCL.StubException;
		}

		/// <param name="unicastAddresses"></param>
		/// <param name="ssdpDevicePublisherFactory"></param>
		/// <param name="port"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public AggregatableDevicePublisher(IEnumerable<string> unicastAddresses,
			ISsdpDevicePublisherFactory ssdpDevicePublisherFactory,
			int port)
		{
			throw PCL.StubException;
		}

		/// <summary>
		/// Dispose all created <see cref="ISsdpDevicePublisher"/>
		/// </summary>
		public void Dispose()
		{
			throw PCL.StubException;
		}

		/// <summary>
		/// Provides all instances of created <see cref="ISsdpDevicePublisher"/>
		/// </summary>
		public IEnumerable<ISsdpDevicePublisher> Publishers { get { throw PCL.StubException; } }

		/// <summary>
		/// Provides all added <see cref="SsdpRootDevice"/> for each <see cref="ISsdpDevicePublisher"/>
		/// </summary>
		public IEnumerable<SsdpRootDevice> Devices
		{
			get
			{
				throw PCL.StubException;
			}
		}

		/// <summary>
		/// Add <see cref="SsdpRootDevice"/> to each <see cref="ISsdpDevicePublisher"/>
		/// </summary>
		/// <param name="ssdpRootDevice"></param>
		public void AddDevice(SsdpRootDevice ssdpRootDevice)
		{
			throw PCL.StubException;
		}

		/// <summary>
		/// Remove <see cref="SsdpRootDevice"/> from each <see cref="ISsdpDevicePublisher"/>
		/// </summary>
		/// <param name="ssdpRootDevice"></param>
		public void RemoveDevice(SsdpRootDevice ssdpRootDevice)
		{
			throw PCL.StubException;
		}
	}
}
