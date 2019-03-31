using System;
using Toxon.Swim.Messages;

namespace Toxon.Swim.Serialization
{
    public class MessageSerializer : IMessageSerializer
    {
        public ReadOnlyMemory<byte> Serialize(SwimMessage message)
        {
            return new byte[0];
        }

        public SwimMessage Deserialize(ref ReadOnlySpan<byte> buffer)
        {
            return new PingMessage(0);
        }
    }
}