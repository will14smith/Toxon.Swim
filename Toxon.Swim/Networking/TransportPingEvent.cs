using System.Net;
using Toxon.Swim.Messages;
using Toxon.Swim.Models;

namespace Toxon.Swim.Networking
{
    public delegate void TransportPingEvent(object sender, TransportPingEventArgs args);

    public class TransportPingEventArgs
    {
        public PingMessage Message { get; }
        public SwimHost Remote { get; }

        public TransportPingEventArgs(PingMessage message, SwimHost remote)
        {
            Message = message;
            Remote = remote;
        }
    }
}