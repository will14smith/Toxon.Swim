using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toxon.Swim.Messages;
using Toxon.Swim.Models;
using Toxon.Swim.Networking;

namespace Toxon.Swim
{
    public class SwimClient
    {
        private readonly SwimClientOptions _options;

        public SwimHost Local { get; }
        public SwimMeta Meta { get; private set; }
        
        internal ITransport Transport { get; private set; }

        public event MembershipChangedEvent MembershipChanged;
        public event MembershipUpdatedEvent MembershipUpdated;

        public SwimClient(SwimHost local, SwimMeta initialMeta, SwimClientOptions options)
        {
            Local = local;
            _options = options;

            Meta = initialMeta;

            Transport = new UdpTransport(this, new UdpTransportOptions(local, options.MessageSerializer, options.Logger));
        }

        public async Task StartAsync()
        {
            await Transport.StartAsync();

            // throw new NotImplementedException("Start listening on local, start services and timers");
        }

        public async Task JoinAsync(IReadOnlyCollection<SwimHost> hosts)
        {
            await Transport.SendAsync(new[]
            {
                new PingMessage(1),
            }, hosts.First());
            // throw new NotImplementedException("Try to contact hosts to join a cluster");
        }

        public Task UpdateMetaAsync(SwimMeta newMeta)
        {
            Meta = newMeta;
            throw new NotImplementedException("Tell others...");
        }

        public async Task LeaveAsync()
        {
            await Transport.StopAsync();

            throw new NotImplementedException();
        }
    }
}
