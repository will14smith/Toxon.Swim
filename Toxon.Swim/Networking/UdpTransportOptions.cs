using Toxon.Swim.Models;
using Toxon.Swim.Serialization;

namespace Toxon.Swim.Networking
{
    public class UdpTransportOptions
    {
        public UdpTransportOptions(SwimHost local, IMessageSerializer messageSerializer)
        {
            Local = local;
            MessageSerializer = messageSerializer;
        }

        public SwimHost Local { get; }

        public IMessageSerializer MessageSerializer { get; }

        public uint MaxDatagramSize { get; set;  } = 512;
    }
}