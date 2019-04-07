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
        private readonly ConcurrentQueue<SwimHost> _queue = new ConcurrentQueue<SwimHost>();

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
                case SwimMemberState.Suspect: UpdateSuspect(member); break;
                case SwimMemberState.Faulty: UpdateFaulty(member); break;

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
        public int Count(bool includeLocal = false, bool includeFaulty = false)
        {
            var members = _members.Count;

            if (includeLocal)
            {
                members++;
            }

            if (!includeFaulty)
            {
                members -= _members.Select(x => x.Value).Count(x => x.State == SwimMemberState.Faulty);
            }

            return members;
        }

        public SwimMember GetFromHost(SwimHost host)
        {
            return _members.TryGetValue(host, out var member) ? member : null;
        }

        public SwimMember Next()
        {
            while (true)
            {
                if (_queue.IsEmpty)
                {
                    RequeueAll();
                }

                if (_queue.TryDequeue(out var host))
                {
                    var member = _members[host];
                    if (member.State == SwimMemberState.Faulty)
                    {
                        continue;
                    }
                    return member;
                }

                return null;
            }
        }

        private void RequeueAll()
        {
            var members = _members.Values.ToList();

            var random = new Random();

            for (var i = members.Count - 1; i > 1; i--)
            {
                var rnd = random.Next(i + 1);

                if (members[i].State != SwimMemberState.Faulty)
                {
                    _queue.Enqueue(members[i].Host);
                }

                members[i] = members[rnd];
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
                var member = _members[list[index]];
                list.RemoveAt(index);

                if (member.State == SwimMemberState.Faulty)
                {
                    continue;
                }
                
                result.Add(member);
            }

            return result;
        }

        public void UpdateMeta(SwimMeta meta)
        {
            Local = Local.WithMeta(meta).IncrementIncarnation();
            OnUpdated?.Invoke(this, new MembershipUpdatedEventArgs(Local));
        }

        private bool TryAdd(SwimMember member)
        {
            if (IsLocal(member))
            {
                throw new InvalidOperationException("Cannot add self to member list");
            }

            if (!_members.TryAdd(member.Host, member))
            {
                return false;
            }
            _queue.Enqueue(member.Host);

            return true;
        }

        private void UpdateAlive(SwimMember member)
        {
            if (IsLocal(member))
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
                    // TODO handle false
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

        private void UpdateSuspect(SwimMember member)
        {
            if (IsLocal(member))
            {
                OnUpdateDropped?.Invoke(this, new MemberDroppedEventArgs(member));
                Local.TryIncarnate(member, true, out _local);
                OnUpdated?.Invoke(this, new MembershipUpdatedEventArgs(Local));
                return;
            }

            var memberCurrent = GetFromHost(member.Host);
            if (memberCurrent?.State == SwimMemberState.Faulty && memberCurrent.Incarnation >= member.Incarnation)
            {
                OnUpdateDropped?.Invoke(this, new MemberDroppedEventArgs(member));
                return;
            }

            if (memberCurrent == null || member.Incarnation > memberCurrent.Incarnation || (member.Incarnation == memberCurrent.Incarnation && memberCurrent.State == SwimMemberState.Alive))
            {
                if (memberCurrent == null)
                {
                    // TODO handle false
                    TryAdd(member);
                    OnJoined?.Invoke(this, new MembershipChangedEventArgs(member));
                }
                else
                {
                    // TODO handle false
                    _members.TryUpdate(member.Host, member, memberCurrent);
                }

                OnUpdated?.Invoke(this, new MembershipUpdatedEventArgs(member));
            }
            else
            {
                OnUpdateDropped?.Invoke(this, new MemberDroppedEventArgs(member));
            }
        }

        private void UpdateFaulty(SwimMember member)
        {
            if (IsLocal(member))
            {
                OnUpdateDropped?.Invoke(this, new MemberDroppedEventArgs(member));
                Local.TryIncarnate(member, true, out _local);
                OnUpdated?.Invoke(this, new MembershipUpdatedEventArgs(Local));
                return;
            }

            var memberCurrent = GetFromHost(member.Host);
            if (memberCurrent != null && member.Incarnation >= memberCurrent.Incarnation && memberCurrent.State != SwimMemberState.Faulty)
            {
                _members.TryUpdate(member.Host, member, memberCurrent);
                OnLeft?.Invoke(this, new MemberLeftEventArgs(member));
                OnUpdated?.Invoke(this, new MembershipUpdatedEventArgs(member));
            }
            else
            {
                OnUpdateDropped?.Invoke(this, new MemberDroppedEventArgs(member));
            }
        }

        private bool IsLocal(SwimMember member)
        {
            return member.Host == Local.Host;
        }
    }
}
