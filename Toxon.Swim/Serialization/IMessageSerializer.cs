using System;
using Toxon.Swim.Messages;

namespace Toxon.Swim.Serialization
{
    public interface IMessageSerializer
    {
        ReadOnlyMemory<byte> Serialize(SwimMessage message);
        SwimMessage Deserialize(ReadOnlySpan<byte> buffer);
    }
}