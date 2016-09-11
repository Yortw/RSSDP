using Rssdp.Infrastructure;

namespace Rssdp
{
    // THIS IS A STUB FILE

    public sealed class UdpSocket : DisposableManagedObjectBase, IUdpSocket
    {
        public System.Threading.Tasks.Task<ReceivedUdpData> ReceiveAsync()
        {
            throw PCL.StubException;
        }

        public void SendTo(byte[] messageData, UdpEndPoint endPoint)
        {
            throw PCL.StubException;
        }

        protected override void Dispose(bool disposing)
        {
            throw PCL.StubException;
        }
    }
}