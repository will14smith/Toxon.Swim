using System;
using System.Net;
using MessagePack;
using MessagePack.Formatters;
using Toxon.Swim.Messages;
using Toxon.Swim.Models;

namespace Toxon.Swim.Serialization
{
    public class MessagePackMessageSerializer : IMessageSerializer, IFormatterResolver, IMessagePackFormatter<SwimMessage>
    {
        public ReadOnlyMemory<byte> Serialize(SwimMessage message)
        {
            return MessagePackSerializer.Serialize(message, this);
        }

        public SwimMessage Deserialize(ReadOnlySpan<byte> buffer)
        {
            return MessagePackSerializer.Deserialize<SwimMessage>(buffer.ToArray(), this);
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            if (typeof(T) == typeof(SwimMessage))
            {
                return (IMessagePackFormatter<T>) this;
            }

            return MessagePack.Resolvers.StandardResolver.Instance.GetFormatter<T>();
        }

        public int Serialize(ref byte[] bytes, int offset, SwimMessage value, IFormatterResolver formatterResolver)
        {
            var length = MessagePackBinary.WriteByte(ref bytes, offset, (byte) value.Type);

            switch (value)
            {
                case AckMessage ackMessage:
                    length += MessagePackBinary.WriteUInt64(ref bytes, offset + length, ackMessage.SequenceNumber);
                    break;
                case PingMessage pingMessage:
                    length += MessagePackBinary.WriteUInt64(ref bytes, offset + length, pingMessage.SequenceNumber);
                    break;
                case PingReqMessage pingReqMessage:
                    length += MessagePackBinary.WriteUInt64(ref bytes, offset + length, pingReqMessage.SequenceNumber);
                    var pingReqDest = pingReqMessage.Destination.AsIPEndPoint();
                    length += MessagePackBinary.WriteBytes(ref bytes, offset + length, pingReqDest.Address.GetAddressBytes());
                    length += MessagePackBinary.WriteUInt16(ref bytes, offset + length, (ushort) pingReqDest.Port);
                    break;

                default: throw new ArgumentOutOfRangeException(nameof(value));
            }

            return length;
        }

        public SwimMessage Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            readSize = 0;

            var type = (SwimMessageType) MessagePackBinary.ReadByte(bytes, offset, out var r);
            readSize += r;

            switch (type)
            {
                case SwimMessageType.Ping:
                    var pingSeq = MessagePackBinary.ReadUInt64(bytes, offset + readSize, out r);
                    readSize += r;

                    return new PingMessage(pingSeq);
                case SwimMessageType.PingReq:
                    var pingReqSeq = MessagePackBinary.ReadUInt64(bytes, offset + readSize, out r);
                    readSize += r;
                    var pingReqDestIp = MessagePackBinary.ReadBytes(bytes, offset + readSize, out r);
                    readSize += r;
                    var pingReqDestPort = MessagePackBinary.ReadUInt16(bytes, offset + readSize, out r);
                    readSize += r;
                    var pingReqDest = new SwimHost(new IPEndPoint(new IPAddress(pingReqDestIp), pingReqDestPort));

                    return new PingReqMessage(pingReqSeq, pingReqDest);
                case SwimMessageType.Ack:
                    var ackSeq = MessagePackBinary.ReadUInt64(bytes, offset + readSize, out r);
                    readSize += r;

                    return new AckMessage(ackSeq);

                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}