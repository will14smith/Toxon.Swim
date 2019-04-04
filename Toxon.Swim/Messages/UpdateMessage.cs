using Toxon.Swim.Models;

namespace Toxon.Swim.Messages
{
    /// <summary>
    /// Send our copy of a member to the remote
    /// </summary>
    public class UpdateMessage : SwimMessage
    {
        public UpdateMessage(SwimMember member)
        {
            Member = member;
        }

        public override SwimMessageType Type => SwimMessageType.Update;

        public SwimMember Member { get; }
    }
}
