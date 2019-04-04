using Toxon.Swim.Membership;

namespace Toxon.Swim.Models
{
    public class SwimMember
    {
        public SwimMember(SwimHost host, SwimMeta meta, SwimMemberState state, int incarnation)
        {
            Host = host;
            Meta = meta;
            State = state;
            Incarnation = incarnation;
        }

        public SwimHost Host { get; }
        public SwimMeta Meta { get; }
        public SwimMemberState State { get; }
        public int Incarnation { get; }

        public SwimMember WithState(SwimMemberState newState)
        {
            return new SwimMember(Host, Meta, newState, Incarnation);
        }
    }
}
