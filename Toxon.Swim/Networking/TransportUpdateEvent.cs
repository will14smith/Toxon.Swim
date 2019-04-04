using Toxon.Swim.Messages;
using Toxon.Swim.Models;

namespace Toxon.Swim.Networking
{
    public delegate void TransportUpdateEvent(object sender, TransportUpdateEventArgs args);

    public class TransportUpdateEventArgs
    {
        public UpdateMessage Message { get; }
        public SwimHost Remote { get; }

        public TransportUpdateEventArgs(UpdateMessage message, SwimHost remote)
        {
            Message = message;
            Remote = remote;
        }
    }
}