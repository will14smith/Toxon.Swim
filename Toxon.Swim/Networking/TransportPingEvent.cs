using System.Net;
using Toxon.Swim.Messages;

namespace Toxon.Swim.Networking
{
    public delegate void TransportPingEvent(object sender, TransportPingEventArgs args);

    public class TransportPingEventArgs
    {
        public PingMessage Message { get; }
        public IPEndPoint RemoteEndpoint { get; }

        public TransportPingEventArgs(PingMessage message, IPEndPoint remoteEndpoint)
        {
            Message = message;
            RemoteEndpoint = remoteEndpoint;
        }
    }
}