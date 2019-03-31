using Serilog;
using Toxon.Swim.Serialization;

namespace Toxon.Swim.Networking
{
    public class UdpTransportOptions
    {
        public UdpTransportOptions(IMessageSerializer messageSerializer, ILogger logger)
        {
            MessageSerializer = messageSerializer;
            Logger = logger.ForContext<UdpTransport>();
        }

        public IMessageSerializer MessageSerializer { get; }

        public ILogger Logger { get; }
        
        public uint MaxDatagramSize { get; set; } = 512;
    }
}