using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rssdp.Infrastructure;
using Rssdp.Network;

namespace Rssdp.Aggregatable
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class AggregatableDeviceLocator : IAggregatableDeviceLocator
	{
		private readonly IList<ISsdpDeviceLocator> _ssdpDeviceLocators = new List<ISsdpDeviceLocator>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="networkInfoProvider"></param>
		/// <param name="ssdpDeviceLocatorFactory"></param>
		/// <param name="port"></param>
		public AggregatableDeviceLocator(INetworkInfoProvider networkInfoProvider, ISsdpDeviceLocatorFactory ssdpDeviceLocatorFactory, int port=0)
		{
			if (networkInfoProvider == null) throw new ArgumentNullException(nameof(networkInfoProvider));
			if (ssdpDeviceLocatorFactory == null) throw new ArgumentNullException(nameof(ssdpDeviceLocatorFactory));

			var unicastAddresses = networkInfoProvider.GetIpAddressesFromAdapters();
			AddLocator(ssdpDeviceLocatorFactory, unicastAddresses, port);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="unicastAddresses"></param>
		/// <param name="ssdpDeviceLocatorFactory"></param>
		/// <param name="port"></param>
		public AggregatableDeviceLocator(IEnumerable<string> unicastAddresses, ISsdpDeviceLocatorFactory ssdpDeviceLocatorFactory, int port = 0)
		{
			if (unicastAddresses == null) throw new ArgumentNullException(nameof(unicastAddresses));
			if (ssdpDeviceLocatorFactory == null) throw new ArgumentNullException(nameof(ssdpDeviceLocatorFactory));

			AddLocator(ssdpDeviceLocatorFactory, unicastAddresses, port);
		}

		void IDisposable.Dispose()
		{
			foreach (var ssdpDeviceLocator in _ssdpDeviceLocators)
			{
				ssdpDeviceLocator.DeviceAvailable -= OnDeviceAvailable;
				ssdpDeviceLocator.DeviceUnavailable -= OnDeviceUnavailable;
				ssdpDeviceLocator.StopListeningForNotifications();
				ssdpDeviceLocator.Dispose();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public event EventHandler<DeviceAvailableEventArgs> DeviceAvailable;

		/// <summary>
		/// 
		/// </summary>
		public event EventHandler<DeviceUnavailableEventArgs> DeviceUnavailable;

		/// <summary>
		/// 
		/// </summary>
		public IEnumerable<ISsdpDeviceLocator> Locators
		{
			get { return _ssdpDeviceLocators; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<DiscoveredSsdpDevice>> SearchAsync()
		{
			var allDevices = new List<DiscoveredSsdpDevice>();
			foreach (var ssdpDeviceLocator in _ssdpDeviceLocators)
			{
				var devices = await ssdpDeviceLocator.SearchAsync();
				allDevices.AddRange(devices);
			}
			return allDevices;
		}

		/// <summary>
		/// 
		/// </summary>
		public void StartListening()
		{
			foreach (var ssdpDeviceLocator in _ssdpDeviceLocators)
				ssdpDeviceLocator.StartListeningForNotifications();
		}

		/// <summary>
		/// 
		/// </summary>
		public void StopListening()
		{
			foreach (var ssdpDeviceLocator in _ssdpDeviceLocators)
				ssdpDeviceLocator.StopListeningForNotifications();
		}

		private void AddLocator(ISsdpDeviceLocatorFactory ssdpDeviceLocatorFactory, IEnumerable<string> availableUnicastAddresses, int port)
		{
			foreach (var availableUnicastAddress in availableUnicastAddresses)
			{
				var ssdpDeviceLocator = ssdpDeviceLocatorFactory.Create(availableUnicastAddress, port);

				ssdpDeviceLocator.DeviceAvailable += OnDeviceAvailable;
				ssdpDeviceLocator.DeviceUnavailable += OnDeviceUnavailable;
				_ssdpDeviceLocators.Add(ssdpDeviceLocator);
			}
		}

		private void OnDeviceAvailable(object sender, DeviceAvailableEventArgs deviceAvailableEventArgs)
		{
			DeviceAvailable?.Invoke(this, deviceAvailableEventArgs);
		}

		private void OnDeviceUnavailable(object sender, DeviceUnavailableEventArgs deviceUnavailableEventArgs)
		{
			DeviceUnavailable?.Invoke(this, deviceUnavailableEventArgs);
		}
	}
}
