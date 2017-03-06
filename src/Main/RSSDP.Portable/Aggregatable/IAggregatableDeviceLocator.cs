using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rssdp.Infrastructure;

namespace Rssdp.Aggregatable
{
	/// <summary>
	/// Creates a <see cref="ISsdpDeviceLocator"/> for each available network interface
	/// </summary>
	public interface IAggregatableDeviceLocator : IDisposable
	{
		/// <summary>
		/// Event raised when a device becomes available
		/// </summary>
		event EventHandler<DeviceAvailableEventArgs> DeviceAvailable;

		/// <summary>
		/// Event raised when a device explicitly notifies of shutdown or a device expires from the cache.
		/// </summary>
		event EventHandler<DeviceUnavailableEventArgs> DeviceUnavailable;

		/// <summary>
		/// Provides all instances of created <see cref="ISsdpDeviceLocator"/>
		/// </summary>
		IEnumerable<ISsdpDeviceLocator> Locators { get; }

		/// <summary>
		/// Aynchronously performs a search for all devices using the default search timeout, and returns an awaitable task that can be used to retrieve the results.
		/// <remarks>All locators doing search</remarks>
		/// </summary>
		Task<IEnumerable<DiscoveredSsdpDevice>> SearchAsync();

		/// <summary>
		/// Starts listening for broadcast notifications of service availability on all locators
		/// </summary>
		/// <remarks>
		/// <para>When called the system will listen for 'alive' and 'byebye' notifications. This can speed up searching, as well as provide dynamic notification of new devices appearing on the network, and previously discovered devices disappearing.</para>
		/// </remarks>
		void StartListeningForNotifications();

		/// <summary>
		/// Stops listening for broadcast notifications of service availability on all locators
		/// </summary>
		/// <remarks>
		/// <para>Does nothing if this instance is not already listening for notifications.</para>
		/// </remarks>
		/// <exception cref="System.ObjectDisposedException">Throw if the <see cref="DisposableManagedObjectBase.IsDisposed"/> property is true.</exception>
		void StopListening();
	}
}