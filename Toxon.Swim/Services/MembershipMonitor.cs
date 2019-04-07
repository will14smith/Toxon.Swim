using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toxon.Swim.Membership;
using Toxon.Swim.Messages;
using Toxon.Swim.Models;
using Toxon.Swim.Networking;

namespace Toxon.Swim.Services
{
    public class MembershipMonitor
    {
        private readonly SwimTransport _transport;
        private readonly FailureDetector _failureDetector;
        private readonly MembershipMonitorOptions _options;
        private readonly MembershipList _membership;

        private readonly ConcurrentDictionary<SwimHost, CancellationTokenSource> _suspectTimeouts = new ConcurrentDictionary<SwimHost, CancellationTokenSource>();

        public MembershipMonitor(MembershipList membership, SwimTransport transport, FailureDetector failureDetector, MembershipMonitorOptions options)
        {
            _transport = transport;
            _failureDetector = failureDetector;
            _options = options;
            _membership = membership;
        }

        public Task StartAsync()
        {
            _failureDetector.OnHostSuspect += HandleHostSuspect;
            _transport.OnAck += HandleAck;
            _transport.OnSync += HandleSync;
            _transport.OnUpdate += HandleUpdate;
            _membership.OnUpdated += HandleMemberUpdated;

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _membership.OnUpdated -= HandleMemberUpdated;
            _transport.OnUpdate -= HandleUpdate;
            _transport.OnSync -= HandleSync;
            _transport.OnAck -= HandleAck;
            _failureDetector.OnHostSuspect -= HandleHostSuspect;

            return Task.CompletedTask;
        }

        public async Task SyncWithAsync(IEnumerable<SwimHost> hosts)
        {
            var messages = new List<SwimMessage>
            {
                new SyncMessage(_membership.Local)
            };
            
            foreach (var member in _membership.GetAll(includeLocal: false, includeFaulty: true))
            {
                messages.Add(new UpdateMessage(member));
            }

            foreach (var host in hosts)
            {
                await _transport.SendAsync(messages, host);
            }
        }

        private void HandleHostSuspect(object sender, HostSuspectEventArgs args)
        {
            var suspectTimeout = TimerUtils.SetTimer(() =>
            {
                _suspectTimeouts.TryRemove(args.Host, out _);
                _membership.UpdateState(args.Host, SwimMemberState.Faulty);
            }, _options.SuspectTimeout);

            if (_suspectTimeouts.TryRemove(args.Host, out var previousSuspectTimeout))
            {
                previousSuspectTimeout.Cancel();
            }
            if (!_suspectTimeouts.TryAdd(args.Host, suspectTimeout))
            {
                // throw new NotImplementedException("Replace with remove(+cancel)/add with 'upsert'(+cancel)");
            }

            _membership.UpdateState(args.Host, SwimMemberState.Suspect);
        }

        private void HandleAck(object sender, TransportAckEventArgs args)
        {
            var member = _membership.GetFromHost(args.Remote);
            if (member == null) return;

            if (member.State != SwimMemberState.Suspect) return;

            _transport.SendAsync(new[]
            {
                new UpdateMessage(member), 
            }, member.Host);
        }

        private void HandleSync(object sender, TransportSyncEventArgs args)
        {
            _membership.Update(args.Message.Member);
            
            var messages = _membership
                .GetAll(includeLocal: true, includeFaulty: true)
                .Select(member => new UpdateMessage(member))
                .ToList();

            _transport.SendAsync(messages, args.Remote);
        }

        private void HandleUpdate(object sender, TransportUpdateEventArgs args)
        {
            _membership.Update(args.Message.Member);
        }

        private void HandleMemberUpdated(object sender, MembershipUpdatedEventArgs args)
        {
            if (_suspectTimeouts.TryRemove(args.Member.Host, out var suspectTimeout))
            {
                suspectTimeout.Cancel();
            }
        }
    }
}
