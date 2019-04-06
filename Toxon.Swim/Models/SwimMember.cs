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
        public SwimMember WithMeta(SwimMeta newMeta)
        {
            return new SwimMember(Host, newMeta, State, Incarnation);
        }
        public SwimMember IncrementIncarnation()
        {
            return new SwimMember(Host, Meta, State, Incarnation + 1);
        }

        public bool TryIncarnate(SwimMember member, bool force, out SwimMember target)
        {
            if (member == null)
            {
                target = IncrementIncarnation();
                return true;
            }

            if (member.Incarnation > Incarnation)
            {
                target = WithMeta(member.Meta).IncrementIncarnation();
                return true;
            }

            if (member.Incarnation == Incarnation && force)
            {
                target = IncrementIncarnation();
                return true;
            }

            target = this;
            return false;
        }
    }
}
