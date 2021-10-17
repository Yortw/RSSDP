using Rssdp.Infrastructure;

namespace Test.RssdpPortable
{
  public class TestDevicePublisher : SsdpDevicePublisherBase
  {

    public TestDevicePublisher(ISsdpCommunicationsServer commsServer)
      : base(commsServer, "TestOS", "1.1")
    {
    }

    public TestDevicePublisher(ISsdpCommunicationsServer commsServer, string osName, string osVersion)
      : base(commsServer, osName, osVersion)
    {
    }

  }
}