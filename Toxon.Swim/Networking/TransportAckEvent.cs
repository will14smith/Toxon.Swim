using System.Net;
using Toxon.Swim.Messages;

namespace Toxon.Swim.Networking
{
    public delegate void TransportAckEvent(object sender, TransportAckEventArgs args);

    public class TransportAckEventArgs
    {
        public AckMessage Message { get; }
        public IPEndPoint RemoteEndpoint { get; }

        public TransportAckEventArgs(AckMessage message, IPEndPoint remoteEndpoint)
        {
            Message = message;
            RemoteEndpoint = remoteEndpoint;
        }
    }
}