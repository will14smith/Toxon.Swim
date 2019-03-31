using Toxon.Swim.Models;

namespace Toxon.Swim.Services
{
    public delegate void HostSuspectEvent(object sender, HostSuspectEventArgs args);

    public class HostSuspectEventArgs
    {
        public SwimHost Host { get; }

        public HostSuspectEventArgs(SwimHost host)
        {
            Host = host;
        }
    }
}