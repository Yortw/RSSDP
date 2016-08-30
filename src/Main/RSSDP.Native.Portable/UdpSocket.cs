using System;
using Rssdp.Infrastructure;

namespace Rssdp
{
    // THIS IS A STUB FILE

    internal sealed class UdpSocket : DisposableManagedObjectBase, IUdpSocket
    {
        public System.Threading.Tasks.Task<ReceivedUdpData> ReceiveAsync()
        {
            throw new NotImplementedException();
        }

        public void SendTo(byte[] messageData, UdpEndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }
    }
}