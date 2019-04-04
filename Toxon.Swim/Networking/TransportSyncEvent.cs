using Toxon.Swim.Messages;
using Toxon.Swim.Models;

namespace Toxon.Swim.Networking
{
    public delegate void TransportSyncEvent(object sender, TransportSyncEventArgs args);

    public class TransportSyncEventArgs
    {
        public SyncMessage Message { get; }
        public SwimHost Remote { get; }

        public TransportSyncEventArgs(SyncMessage message, SwimHost remote)
        {
            Message = message;
            Remote = remote;
        }
    }
}