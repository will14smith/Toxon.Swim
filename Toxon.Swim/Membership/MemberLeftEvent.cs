using Toxon.Swim.Models;

namespace Toxon.Swim.Membership
{
    public delegate void MemberLeftEvent(object sender, MemberLeftEventArgs args);

    public class MemberLeftEventArgs
    {
        public SwimMember Member { get; }

        public MemberLeftEventArgs(SwimMember member)
        {
            Member = member;
        }
    }
}