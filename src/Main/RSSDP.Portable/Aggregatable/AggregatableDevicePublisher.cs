using System;
using System.Collections.Generic;
using Rssdp.Infrastructure;
using Rssdp.Network;

namespace Rssdp.Aggregatable
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class AggregatableDevicePublisher : IAggregatableDevicePublisher
	{
		private readonly IList<ISsdpDevicePublisher> _ssdpDevicePublisherses = new List<ISsdpDevicePublisher>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="networkInfoProvider"></param>
		/// <param name="ssdpDevicePublisherFactory"></param>
		/// <param name="port"></param>
		public AggregatableDevicePublisher(INetworkInfoProvider networkInfoProvider,
			ISsdpDevicePublisherFactory ssdpDevicePublisherFactory,
			int port = 0)
		{
			if (networkInfoProvider == null) throw new ArgumentNullException(nameof(networkInfoProvider));
			if (ssdpDevicePublisherFactory == null) throw new ArgumentNullException(nameof(ssdpDevicePublisherFactory));

			var unicastAddresses = networkInfoProvider.GetIpAddressesFromAdapters();

			AddPublisher(ssdpDevicePublisherFactory, unicastAddresses, port);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="unicastAddresses"></param>
		/// <param name="ssdpDevicePublisherFactory"></param>
		/// <param name="port"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public AggregatableDevicePublisher(IEnumerable<string> unicastAddresses,
			ISsdpDevicePublisherFactory ssdpDevicePublisherFactory,
			int port = 0)
		{
			if (unicastAddresses == null) throw new ArgumentNullException(nameof(unicastAddresses));
			if (ssdpDevicePublisherFactory == null) throw new ArgumentNullException(nameof(ssdpDevicePublisherFactory));

			AddPublisher(ssdpDevicePublisherFactory, unicastAddresses, port);
		}

		void IDisposable.Dispose()
		{
			foreach (var ssdpDevicePublisher in _ssdpDevicePublisherses)
			{
				//todo maybe 'byebye notification' should be sent atomatically for each device 
				//in other words need remove all published devices, when Dispose has been called
				ssdpDevicePublisher.Dispose();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public IEnumerable<ISsdpDevicePublisher> Publishers { get { return _ssdpDevicePublisherses; } }

		/// <summary>
		/// 
		/// </summary>
		public IEnumerable<SsdpRootDevice> Devices
		{
			get
			{
				var allPublishedDevices = new List<SsdpRootDevice>();

				foreach (var ssdpDevicePublisher in Publishers)
					allPublishedDevices.AddRange(ssdpDevicePublisher.Devices);

				return allPublishedDevices;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ssdpRootDevice"></param>
		public void AddDevice(SsdpRootDevice ssdpRootDevice)
		{
			foreach (var ssdpDevicePublisher in _ssdpDevicePublisherses)
				ssdpDevicePublisher.AddDevice(ssdpRootDevice);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ssdpRootDevice"></param>
		public void RemoveDevice(SsdpRootDevice ssdpRootDevice)
		{
			foreach (var ssdpDevicePublisher in _ssdpDevicePublisherses)
				ssdpDevicePublisher.RemoveDevice(ssdpRootDevice);
		}

		private void AddPublisher(ISsdpDevicePublisherFactory ssdpDevicePublisherFactory, IEnumerable<string> availableUnicastAddresses, int port)
		{
			foreach (var availableUnicastAddress in availableUnicastAddresses)
			{
				var ssdpDevicePublisher = ssdpDevicePublisherFactory.Create(availableUnicastAddress, port);
				_ssdpDevicePublisherses.Add(ssdpDevicePublisher);
			}
		}
	}
}
