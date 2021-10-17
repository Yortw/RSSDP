using Rssdp.Infrastructure;

namespace Rssdp
{
  // THIS IS A LINKED FILE - SHARED AMONGST MULTIPLE PLATFORMS	
  // Be careful to check any changes compile and work for all platform projects it is shared in.

  /// <summary>
  /// Allows you to search the network for a particular device, device types, or UPnP service types. Also listenings for broadcast notifications of device availability and raises events to indicate changes in status.
  /// </summary>
  public sealed class SsdpDeviceLocator : SsdpDeviceLocatorBase
  {

    /// <summary>
    /// Default constructor. Constructs a new instance using the default <see cref="ISsdpCommunicationsServer"/> and <see cref="ISocketFactory"/> implementations for this platform.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Can't expose along exception paths here (exceptions should be very rare anyway, and probably fatal too) and we shouldn't dipose the items we pass to base in any other case.")]
    public SsdpDeviceLocator() : base(new SsdpCommunicationsServer(new SocketFactory(null)))
    {
      // This is not the problem you are looking for.
      // Yes, this is poor man's dependency injection which some call an anti-pattern.
      // However, it makes the library really simple to get started with or to use if the calling code isn't using IoC/DI.
      // The fact we have injected dependencies is really an internal architectural implementation detail to allow for the
      // cross platform and testing concerns of this library. It shouldn't be something calling code worries about and is 
      // not a deliberate extension point, except where adding new platform support in which case...
      // There is a constructor that takes a manually injected dependency anyway, so proper DI using
      // a container or whatever can be done anyway.
    }

    /// <summary>
    /// Partial constructor. 
    /// </summary>
    /// <param name="ipAddress">The IP address of the local network adapter to bind sockets to. 
    /// Null or empty string will use an IP address selected by the OS or runtime.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Can't expose along exception paths here (exceptions should be very rare anyway, and probably fatal too) and we shouldn't dipose the items we pass to base in any other case.")]
    public SsdpDeviceLocator(string ipAddress) : base(new SsdpCommunicationsServer(new SocketFactory(ipAddress)))
    {
      // This is not the problem you are looking for.
      // Yes, this is poor man's dependency injection which some call an anti-pattern.
      // However, it makes the library really simple to get started with or to use if the calling code isn't using IoC/DI.
      // The fact we have injected dependencies is really an internal architectural implementation detail to allow for the
      // cross platform and testing concerns of this library. It shouldn't be something calling code worries about and is 
      // not a deliberate extension point, except where adding new platform support in which case...
      // There is a constructor that takes a manually injected dependency anyway, so proper DI using
      // a container or whatever can be done anyway.
    }

    /// <summary>
    /// Full constructor. Constructs a new instance using the provided <see cref="ISsdpCommunicationsServer"/> implementation.
    /// </summary>
    public SsdpDeviceLocator(ISsdpCommunicationsServer communicationsServer)
      : base(communicationsServer)
    {
    }
  }
}