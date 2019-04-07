using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toxon.Swim.Membership;
using Toxon.Swim.Models;
using Toxon.Swim.Networking;
using Toxon.Swim.Services;

namespace Toxon.Swim
{
    public class SwimClient
    {
        public SwimHost Local { get; }
        public MembershipList Members { get; }

        internal Disseminator Disseminator { get; }
        internal SwimTransport Transport { get; }
        internal FailureDetector FailureDetector { get; }
        internal MembershipMonitor MembershipMonitor { get; }

        public SwimClient(SwimHost local, SwimMeta initialMeta, SwimClientOptions options)
        {
            Local = local;

            Members = new MembershipList(local, initialMeta);
            Disseminator = new Disseminator(Members, new DisseminatorOptions());
            Transport = new SwimTransport(new UdpTransport(local, new UdpTransportOptions()), Disseminator, options.MessageSerializer);
            FailureDetector = new FailureDetector(Transport, Members, new FailureDetectorOptions(options.Logger));
            MembershipMonitor = new MembershipMonitor(Members, Transport, FailureDetector, new MembershipMonitorOptions());

            Members.OnJoined += (_, args) => options.Logger.Information("Host {host} joined", args.Member.Host);
            Members.OnUpdated += (_, args) => options.Logger.Information("Host {host} updated", args.Member.Host);
            Members.OnLeft += (_, args) => options.Logger.Information("Host {host} left", args.Member.Host);
        }

        public async Task StartAsync()
        {
            await Disseminator.StartAsync();
            await Transport.StartAsync();
            await FailureDetector.StartAsync();
            await MembershipMonitor.StartAsync();
        }

        public async Task JoinAsync(IReadOnlyCollection<SwimHost> hosts)
        {
            var filteredHosts = hosts.Where(x => x != Local);

            await MembershipMonitor.SyncWithAsync(filteredHosts);

            // TODO check at least 1 host has responded
        }

        public void UpdateMeta(SwimMeta meta)
        {
            Members.UpdateMeta(meta);
        }

        public async Task LeaveAsync()
        {
            // TODO tell others?

            await MembershipMonitor.StopAsync();
            await FailureDetector.StopAsync();
            await Transport.StopAsync();
            await Disseminator.StopAsync();
        }
    }
}
