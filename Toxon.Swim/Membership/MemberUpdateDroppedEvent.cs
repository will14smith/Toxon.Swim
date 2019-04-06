using Toxon.Swim.Models;

namespace Toxon.Swim.Membership
{
    public delegate void MemberUpdateDroppedEvent(object sender, MemberDroppedEventArgs args);

    public class MemberDroppedEventArgs
    {
        public SwimMember Member { get; }

        public MemberDroppedEventArgs(SwimMember member)
        {
            Member = member;
        }
    }
}