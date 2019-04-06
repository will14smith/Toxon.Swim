using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Toxon.Swim.Models;

namespace Toxon.Swim.Membership
{
    public class MembershipList
    {
        private SwimMember _local;
        public SwimMember Local
        {
            get => _local;
            private set => _local = value;
        }

        private readonly ConcurrentDictionary<SwimHost, SwimMember> _members = new ConcurrentDictionary<SwimHost, SwimMember>();
        private readonly ConcurrentQueue<SwimMember> _queue = new ConcurrentQueue<SwimMember>();

        public event MemberChangedEvent OnJoined;
        public event MemberUpdatedEvent OnUpdated;
        public event MemberUpdateDroppedEvent OnUpdateDropped;
        public event MemberLeftEvent OnLeft;

        public MembershipList(SwimHost local, SwimMeta initialLocalMeta)
        {
            Local = new SwimMember(local, initialLocalMeta, SwimMemberState.Alive, 0);
        }

        public void Update(SwimMember member)
        {
            switch (member.State)
            {
                case SwimMemberState.Alive: UpdateAlive(member); break;
                case SwimMemberState.Suspect: throw new NotImplementedException();
                case SwimMemberState.Faulty: throw new NotImplementedException();

                default: throw new ArgumentOutOfRangeException();
            }
        }
        public void UpdateState(SwimHost host, SwimMemberState state)
        {
            Update(_members[host].WithState(state));
        }

        public IReadOnlyCollection<SwimMember> GetAll(bool includeLocal = false, bool includeFaulty = false)
        {
            var members = _members.Values.ToList();

            if (includeLocal)
            {
                members.Add(Local);
            }

            if (!includeFaulty)
            {
                return members.Where(x => x.State != SwimMemberState.Faulty).ToList();
            }

            return members;
        }

        public SwimMember GetFromHost(SwimHost host)
        {
            return _members.TryGetValue(host, out var member) ? member : null;
        }

        public SwimMember Next()
        {
            if (_queue.IsEmpty)
            {
                RequeueAll();
            }

            if (_queue.TryDequeue(out var member))
            {
                // get the latest version (queue member might have been updated)
                return _members[member.Host];
            }

            return null;
        }

        private void RequeueAll()
        {
            // TODO randomize order
            foreach (var member in _members.Values)
            {
                _queue.Enqueue(member);
            }
        }

        public IReadOnlyCollection<SwimMember> GetRandom(int count)
        {
            var list = _members.Keys.ToList();
            var result = new List<SwimMember>();

            var r = new Random();

            while (count > 0 && list.Count > 0)
            {
                var index = r.Next(0, list.Count);
                var item = list[index];
                list.RemoveAt(index);

                result.Add(_members[item]);
            }

            return result;
        }

        private bool TryAdd(SwimMember member)
        {
            if (!_members.TryAdd(member.Host, member))
            {
                return false;
            }
            _queue.Enqueue(member);

            return true;
        }

        private void UpdateAlive(SwimMember member)
        {
            if (member.Host == Local.Host)
            {
                if (Local.TryIncarnate(member, false, out _local))
                {
                    OnUpdated?.Invoke(this, new MembershipUpdatedEventArgs(Local));
                }
                else
                {
                    OnUpdateDropped?.Invoke(this, new MemberDroppedEventArgs(member));
                }

                return;
            }

            var memberCurrent = GetFromHost(member.Host);
            if (memberCurrent?.State == SwimMemberState.Faulty && memberCurrent.Incarnation >= member.Incarnation)
            {
                OnUpdateDropped?.Invoke(this, new MemberDroppedEventArgs(member));
                return;
            }

            if (memberCurrent == null || member.Incarnation > memberCurrent.Incarnation)
            {
                if (memberCurrent == null)
                {
                    TryAdd(member);
                    OnJoined?.Invoke(this, new MembershipChangedEventArgs(member));
                }
                else
                {
                    _members.TryUpdate(member.Host, member, memberCurrent);
                }

                OnUpdated?.Invoke(this, new MembershipUpdatedEventArgs(member));
            }
            else
            {
                OnUpdateDropped?.Invoke(this, new MemberDroppedEventArgs(member));
            }
        }
    }
}
