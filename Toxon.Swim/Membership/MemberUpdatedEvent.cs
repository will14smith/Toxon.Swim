using Toxon.Swim.Models;

namespace Toxon.Swim.Membership
{
    public delegate void MemberUpdatedEvent(object sender, MembershipUpdatedEventArgs args);

    public class MembershipUpdatedEventArgs
    {
        public SwimMember Member { get; }

        public MembershipUpdatedEventArgs(SwimMember member)
        {
            Member = member;
        }
    }
}