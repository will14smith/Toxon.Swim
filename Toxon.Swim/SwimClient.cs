using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toxon.Swim.Messages;
using Toxon.Swim.Models;
using Toxon.Swim.Networking;
using Toxon.Swim.Services;

namespace Toxon.Swim
{
    public class SwimClient
    {
        private readonly SwimClientOptions _options;

        public SwimHost Local { get; }
        public SwimMeta Meta { get; private set; }
        
        internal SwimTransport Transport { get; }
        internal FailureDetector FailureDetector { get; }

        public event MembershipChangedEvent MembershipChanged;
        public event MembershipUpdatedEvent MembershipUpdated;

        public SwimClient(SwimHost local, SwimMeta initialMeta, SwimClientOptions options)
        {
            Local = local;
            _options = options;

            Meta = initialMeta;

            Transport = new SwimTransport(new UdpTransport(local, new UdpTransportOptions(options.MessageSerializer, options.Logger)));
            FailureDetector = new FailureDetector(Transport, new FailureDetectorOptions(options.Logger));
        }

        public async Task StartAsync()
        {
            await Transport.StartAsync();
            await FailureDetector.StartAsync();
        }

        public async Task JoinAsync(IReadOnlyCollection<SwimHost> hosts)
        {
            await FailureDetector.PingAsync(hosts.First());
            // throw new NotImplementedException("Try to contact hosts to join a cluster");
        }

        public Task UpdateMetaAsync(SwimMeta newMeta)
        {
            Meta = newMeta;
            throw new NotImplementedException("Tell others...");
        }

        public async Task LeaveAsync()
        {
            await FailureDetector.StopAsync();
            await Transport.StopAsync();
        }
    }
}
