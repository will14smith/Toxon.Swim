using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Toxon.Swim.Membership;
using Toxon.Swim.Messages;
using Toxon.Swim.Models;
using Toxon.Swim.Networking;
using Timer = System.Timers.Timer;

namespace Toxon.Swim.Services
{
    public class FailureDetector
    {
        private readonly SwimTransport _transport;
        private readonly MembershipList _membershipList;
        private readonly FailureDetectorOptions _options;
        private readonly Timer _timer;

        private long _seq;

        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _seqTimeouts = new ConcurrentDictionary<ulong, CancellationTokenSource>();
        private readonly ConcurrentDictionary<ulong, Action> _seqCallbacks = new ConcurrentDictionary<ulong, Action>();

        public event HostSuspectEvent OnHostSuspect;

        public FailureDetector(SwimTransport transport, MembershipList membershipList, FailureDetectorOptions options)
        {
            _transport = transport;
            _membershipList = membershipList;
            _options = options;

            _timer = new Timer
            {
                Interval = _options.PingInterval.TotalMilliseconds,
                AutoReset = true
            };
            _timer.Elapsed += TickPing;
        }

        public Task StartAsync()
        {
            _transport.OnPing += HandlePing;
            _transport.OnPingReq += HandlePingReq;
            _transport.OnAck += HandleAck;

            _timer.Start();

            return Task.CompletedTask;
        }
        public Task StopAsync()
        {
            _timer.Stop();

            _transport.OnPing -= HandlePing;
            _transport.OnPingReq -= HandlePingReq;
            _transport.OnAck -= HandleAck;

            _seqTimeouts.Clear();
            _seqCallbacks.Clear();

            return Task.CompletedTask;
        }

        public async Task PingAsync(SwimHost host)
        {
            var seq = (ulong)Interlocked.Increment(ref _seq);

            var cts = TimerUtils.SetTimer(() =>
            {
                CleanupSeq(seq);
                PingReqAsync(host);
            }, _options.PingTimeout);
            _seqTimeouts.AddOrUpdate(seq, cts, (_, __) => cts);

            await _transport.SendAsync(new[]
            {
                new PingMessage(seq),
            }, host);
        }

        private async Task PingReqAsync(SwimHost targetHost)
        {
            // TODO ping random N hosts
            var relayHosts = _membershipList.GetRandom(_options.PingReqGroupSize).Select(member => member.Host);

            var cts = TimerUtils.SetTimer(() =>
            {
                OnHostSuspect?.Invoke(this, new HostSuspectEventArgs(targetHost));
            }, _options.PingReqTimeout);

            foreach (var relayHost in relayHosts)
            {
                await PingReqAsync(targetHost, relayHost, () => cts.Cancel());
            }
        }

        private async Task PingReqAsync(SwimHost targetHost, SwimHost relayHost, Action callback)
        {
            var seq = (ulong)Interlocked.Increment(ref _seq);

            var cts = TimerUtils.SetTimer(() => CleanupSeq(seq), _options.PingTimeout);
            void SeqCallback()
            {
                CleanupSeq(seq);
                callback();
            }

            _seqTimeouts.AddOrUpdate(seq, cts, (_, __) => cts);
            _seqCallbacks.AddOrUpdate(seq, SeqCallback, (_, __) => SeqCallback);

            await _transport.SendAsync(new[]
            {
                new PingReqMessage(seq, targetHost),
            }, relayHost);
        }

        private void TickPing(object sender, ElapsedEventArgs e)
        {
            var host = _membershipList.Next()?.Host;
            if (host == null)
            {
                return;
            }

            PingAsync(host);
        }

        private void HandlePing(object sender, TransportPingEventArgs args)
        {
            var seq = args.Message.SequenceNumber;
            var remote = args.Remote;

            _transport.SendAsync(new[]
            {
                new AckMessage(seq),
            }, remote);
        }
        private void HandlePingReq(object sender, TransportPingReqEventArgs args)
        {
            var seq = (ulong)Interlocked.Increment(ref _seq);

            var destinationHost = args.Message.Destination;
            var relayHost = args.Remote;

            var cts = TimerUtils.SetTimer(() => CleanupSeq(seq), _options.PingTimeout);
            Action callback = () =>
            {
                CleanupSeq(seq);
                _transport.SendAsync(new[]
                {
                    new AckMessage(args.Message.SequenceNumber),
                }, relayHost);
            };

            _seqTimeouts.AddOrUpdate(seq, cts, (_, __) => cts);
            _seqCallbacks.AddOrUpdate(seq, callback, (_, __) => callback);

            _transport.SendAsync(new[]
            {
                new PingMessage(seq),
            }, destinationHost);
        }
        private void HandleAck(object sender, TransportAckEventArgs args)
        {
            var seq = args.Message.SequenceNumber;

            var hasCallback = _seqCallbacks.TryGetValue(seq, out var callback);
            CleanupSeq(seq);

            if (hasCallback)
            {
                callback();
            }
        }

        private void CleanupSeq(ulong seq)
        {
            if (_seqTimeouts.TryRemove(seq, out var cancellation))
            {
                cancellation.Cancel();
            }
            _seqCallbacks.TryRemove(seq, out _);
        }
    }
}
