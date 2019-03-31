using Serilog;
using Toxon.Swim.Models;
using Toxon.Swim.Serialization;

namespace Toxon.Swim.Networking
{
    public class UdpTransportOptions
    {
        public UdpTransportOptions(SwimHost local, IMessageSerializer messageSerializer, ILogger logger)
        {
            Local = local;
            MessageSerializer = messageSerializer;
            Logger = logger.ForContext<UdpTransport>();
        }

        public SwimHost Local { get; }

        public IMessageSerializer MessageSerializer { get; }

        public ILogger Logger { get; }


        public uint MaxDatagramSize { get; set; } = 512;
    }
}