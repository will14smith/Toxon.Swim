using Toxon.Swim.Messages;
using Toxon.Swim.Models;

namespace Toxon.Swim.Networking
{
    public delegate void TransportAckEvent(object sender, TransportAckEventArgs args);

    public class TransportAckEventArgs
    {
        public AckMessage Message { get; }
        public SwimHost Remote { get; }

        public TransportAckEventArgs(AckMessage message, SwimHost remote)
        {
            Message = message;
            Remote = remote;
        }
    }
}