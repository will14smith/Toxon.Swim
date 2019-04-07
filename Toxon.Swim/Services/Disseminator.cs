using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toxon.Swim.Membership;
using Toxon.Swim.Messages;
using Toxon.Swim.Models;

namespace Toxon.Swim.Services
{
    public class Disseminator
    {
        private readonly MembershipList _membership;
        private readonly DisseminatorOptions _options;

        private readonly ConcurrentDictionary<SwimHost, SwimMessage> _hostMessages = new ConcurrentDictionary<SwimHost, SwimMessage>();
        private readonly ConcurrentDictionary<SwimMessage, int> _messageAttempts = new ConcurrentDictionary<SwimMessage, int>();

        private int _attemptLimit;

        public Disseminator(MembershipList membership, DisseminatorOptions options)
        {
            _membership = membership;
            _options = options;
        }

        public Task StartAsync()
        {
            UpdateAttemptLimit();

            _membership.OnJoined += HandleMemberJoined;
            _membership.OnUpdated += HandleMemberUpdated;
            _membership.OnLeft += HandleMemberLeft;

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _membership.OnLeft -= HandleMemberLeft;
            _membership.OnUpdated -= HandleMemberUpdated;
            _membership.OnJoined -= HandleMemberJoined;

            return Task.CompletedTask;
        }

        public IReadOnlyCollection<SwimMessage> GetMessages()
        {
            return _messageAttempts.OrderBy(x => x.Value).Select(x => x.Key).ToList();
        }
        public void MarkMessageAsSent(SwimMessage message)
        {
            var attempts = _messageAttempts.AddOrUpdate(message, 1, (_, previous) => previous + 1);
            if (attempts <= _attemptLimit) return;
            
            _messageAttempts.TryRemove(message, out _);
            // TODO ...
            var host = _hostMessages.FirstOrDefault(x => ReferenceEquals(x.Value, message)).Key;
            if (host != null)
            {
                _hostMessages.TryRemove(host, out _);
            }
        }

        private void HandleMemberJoined(object sender, MembershipChangedEventArgs args)
        {
            HandleMessage(args.Member.Host, new UpdateMessage(args.Member));
            UpdateAttemptLimit();
        }
        private void HandleMemberUpdated(object sender, MembershipUpdatedEventArgs args)
        {
            HandleMessage(args.Member.Host, new UpdateMessage(args.Member));
        }
        private void HandleMemberLeft(object sender, MemberLeftEventArgs args)
        {
            HandleMessage(args.Member.Host, new UpdateMessage(args.Member));
            UpdateAttemptLimit();
        }

        private void HandleMessage(SwimHost host, SwimMessage message)
        {
            if (_hostMessages.TryRemove(host, out var oldMessage))
            {
                // TODO handle false
                _messageAttempts.TryRemove(oldMessage, out _);
            }
            
            if(_hostMessages.TryAdd(host, message))
            {
                // TODO handle false
                _messageAttempts.TryAdd(message, 0);
            }
        }

        private void UpdateAttemptLimit()
        {
            _attemptLimit = _options.AttemptLimit(_options.AttemptFactor, _membership.Count(includeLocal: true, includeFaulty: false));
        }
    }

    public class DisseminatorOptions
    {
        public delegate int DisseminatorAttemptLimit(decimal factor, int memberCount);

        public decimal AttemptFactor = 3;
        public DisseminatorAttemptLimit AttemptLimit { get; set; } = DefaultAttemptLimit;

        private static int DefaultAttemptLimit(decimal factor, int memberCount)
        {
            return (int)Math.Ceiling(factor * (decimal)Math.Log10(memberCount + 1));
        }
    }
}
