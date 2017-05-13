using Rssdp.Infrastructure;

namespace Rssdp
{
    // THIS IS A STUB FILE
    /// <summary>
    /// Allows you to search the network for a particular device, device types, or UPnP service types. Also listenings for broadcast notifications of device availability and raises events to indicate changes in status.
    /// </summary>
    public sealed class SsdpDeviceLocator : SsdpDeviceLocatorBase
    {

        /// <summary>
        /// Default constructor. Constructs a new instance using the default <see cref="ISsdpCommunicationsServer"/> and <see cref="ISocketFactory"/> implementations for this platform.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Can't expose along exception paths here (exceptions should be very rare anyway, and probably fatal too) and we shouldn't dipose the items we pass to base in any other case.")]
        public SsdpDeviceLocator() : base(null)
        {
            throw PCL.StubException;
        }

        /// <summary>
        /// Full constructor. Constructs a new instance using the provided <see cref="ISsdpCommunicationsServer"/> implementation.
        /// </summary>
        public SsdpDeviceLocator(ISsdpCommunicationsServer communicationsServer)
            : base(communicationsServer)
        {
            throw PCL.StubException;
        }

    }
}