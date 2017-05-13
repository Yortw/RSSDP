using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rssdp
{
	/// <summary>
	/// Event arguments for the <see cref="SsdpDevice.ServiceAdded"/> and <see cref="SsdpDevice.ServiceRemoved"/> events.
	/// </summary>
	public sealed class ServiceEventArgs : EventArgs
	{

		#region Fields

		private readonly SsdpService _Service;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs a new instance for the specified <see cref="SsdpService"/>.
		/// </summary>
		/// <param name="service">The <see cref="SsdpService"/> associated with the event this argument class is being used for.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="service"/> argument is null.</exception>
		public ServiceEventArgs(SsdpService service)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));

			_Service = service;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Returns the <see cref="SsdpService"/> instance the event is being raised for.
		/// </summary>
		public SsdpService Service
		{
			get { return _Service; }
		}

		#endregion

	}
}