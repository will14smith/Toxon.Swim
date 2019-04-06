using Toxon.Swim.Models;

namespace Toxon.Swim.Membership
{
    public delegate void MemberChangedEvent(object sender, MembershipChangedEventArgs args);

    public class MembershipChangedEventArgs
    {
        public SwimMember Member { get; }

        public MembershipChangedEventArgs(SwimMember member)
        {
            Member = member;
        }
    }
}