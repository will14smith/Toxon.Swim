using Toxon.Swim.Models;

namespace Toxon.Swim.Messages
{
    /// <summary>
    /// Send local member and request all members from remote
    /// </summary>
    public class SyncMessage : SwimMessage
    {
        public SyncMessage(SwimMember member)
        {
            Member = member;
        }

        public override SwimMessageType Type => SwimMessageType.Sync;

        public SwimMember Member { get; }
    }
}
