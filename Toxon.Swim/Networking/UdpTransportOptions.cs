using Serilog;
using Toxon.Swim.Serialization;

namespace Toxon.Swim.Networking
{
    public class UdpTransportOptions
    {
        public UdpTransportOptions(ILogger logger)
        {
            Logger = logger.ForContext<UdpTransport>();
        }

        public ILogger Logger { get; }
        
        public uint MaxDatagramSize { get; set; } = 512;
    }
}