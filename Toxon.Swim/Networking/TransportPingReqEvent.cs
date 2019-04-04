using Toxon.Swim.Messages;
using Toxon.Swim.Models;

namespace Toxon.Swim.Networking
{
    public delegate void TransportPingReqEvent(object sender, TransportPingReqEventArgs args);

    public class TransportPingReqEventArgs
    {
        public PingReqMessage Message { get; }
        public SwimHost Remote { get; }

        public TransportPingReqEventArgs(PingReqMessage message, SwimHost remote)
        {
            Message = message;
            Remote = remote;
        }
    }
}