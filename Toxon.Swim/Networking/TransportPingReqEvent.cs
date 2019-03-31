using System.Net;
using Toxon.Swim.Messages;

namespace Toxon.Swim.Networking
{
    public delegate void TransportPingReqEvent(object sender, TransportPingReqEventArgs args);

    public class TransportPingReqEventArgs
    {
        public PingReqMessage Message { get; }
        public IPEndPoint RemoteEndpoint { get; }

        public TransportPingReqEventArgs(PingReqMessage message, IPEndPoint remoteEndpoint)
        {
            Message = message;
            RemoteEndpoint = remoteEndpoint;
        }
    }
}