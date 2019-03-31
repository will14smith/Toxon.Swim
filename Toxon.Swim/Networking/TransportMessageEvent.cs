using System.Net;
using Toxon.Swim.Messages;

namespace Toxon.Swim.Networking
{
    public delegate void TransportMessageEvent(object sender, TransportMessageEventArgs args);

    public class TransportMessageEventArgs
    {
        public SwimMessage Message { get; }
        public IPEndPoint RemoteEndpoint { get; }

        public TransportMessageEventArgs(SwimMessage message, IPEndPoint remoteEndpoint)
        {
            Message = message;
            RemoteEndpoint = remoteEndpoint;
        }
    }
}