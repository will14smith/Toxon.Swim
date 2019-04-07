using System;
using System.Collections.Generic;
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
                return (IMessagePackFormatter<T>)this;
            }

            return MessagePack.Resolvers.StandardResolver.Instance.GetFormatter<T>();
        }

        public int Serialize(ref byte[] bytes, int offset, SwimMessage value, IFormatterResolver formatterResolver)
        {
            var length = MessagePackBinary.WriteByte(ref bytes, offset, (byte)value.Type);

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
                    var pingReqDest = pingReqMessage.Destination;
                    length += SerializeHost(ref bytes, offset + length, pingReqDest);
                    break;
                case SyncMessage syncMessage:
                    length += SerializeMember(ref bytes, offset + length, syncMessage.Member);
                    break;
                case UpdateMessage updateMessage:
                    length += SerializeMember(ref bytes, offset + length, updateMessage.Member);
                    break;

                default: throw new ArgumentOutOfRangeException(nameof(value));
            }

            return length;
        }

        public SwimMessage Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            readSize = 0;

            var type = (SwimMessageType)MessagePackBinary.ReadByte(bytes, offset, out var r);
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
                    var pingReqDest = DeserializeHost(bytes, offset + readSize, out r);
                    readSize += r;

                    return new PingReqMessage(pingReqSeq, pingReqDest);
                case SwimMessageType.Ack:
                    var ackSeq = MessagePackBinary.ReadUInt64(bytes, offset + readSize, out r);
                    readSize += r;

                    return new AckMessage(ackSeq);
                case SwimMessageType.Sync:
                    var syncMember = DeserializeMember(bytes, offset + readSize, out r);
                    readSize += r;

                    return new SyncMessage(syncMember);
                case SwimMessageType.Update:
                    var updateMember = DeserializeMember(bytes, offset + readSize, out r);
                    readSize += r;

                    return new UpdateMessage(updateMember);

                default: throw new ArgumentOutOfRangeException();
            }
        }

        private static int SerializeHost(ref byte[] bytes, int offset, SwimHost host)
        {
            var endpoint = host.AsIPEndPoint();

            var length = 0;

            length += MessagePackBinary.WriteBytes(ref bytes, offset + length, endpoint.Address.GetAddressBytes());
            length += MessagePackBinary.WriteUInt16(ref bytes, offset + length, (ushort)endpoint.Port);

            return length;
        }

        private SwimHost DeserializeHost(byte[] bytes, int offset, out int readSize)
        {
            readSize = 0;

            var addressBytes = MessagePackBinary.ReadBytes(bytes, offset + readSize, out var r);
            readSize += r;
            var port = MessagePackBinary.ReadUInt16(bytes, offset + readSize, out r);
            readSize += r;

            return new SwimHost(new IPEndPoint(new IPAddress(addressBytes), port));
        }


        private int SerializeMember(ref byte[] bytes, int offset, SwimMember member)
        {
            var length = 0;

            length += SerializeHost(ref bytes, offset + length, member.Host);
            length += MessagePackBinary.WriteMapHeader(ref bytes, offset + length, member.Meta.Fields.Count);
            foreach (var kvp in member.Meta.Fields)
            {
                length += MessagePackBinary.WriteString(ref bytes, offset + length, kvp.Key);
                length += MessagePackBinary.WriteString(ref bytes, offset + length, kvp.Value);
            }

            length += MessagePackBinary.WriteByte(ref bytes, offset + length, (byte)member.State);
            length += MessagePackBinary.WriteInt32(ref bytes, offset + length, member.Incarnation);

            return length;
        }

        private SwimMember DeserializeMember(byte[] bytes, int offset, out int readSize)
        {
            readSize = 0;

            var host = DeserializeHost(bytes, offset + readSize, out var r);
            readSize += r;

            var metaCount = MessagePackBinary.ReadMapHeader(bytes, offset + readSize, out r);
            readSize += r;
            var meta = new Dictionary<string, string>();
            for(var i = 0; i < metaCount; i++)
            {
                var key = MessagePackBinary.ReadString(bytes, offset + readSize, out r);
                readSize += r;
                var value = MessagePackBinary.ReadString(bytes, offset + readSize, out r);
                readSize += r;

                meta.Add(key, value);
            }

            var state = (SwimMemberState)MessagePackBinary.ReadByte(bytes, offset + readSize, out r);
            readSize += r;
            var incarnation = MessagePackBinary.ReadInt32(bytes, offset + readSize, out r);
            readSize += r;

            return new SwimMember(host, new SwimMeta(meta), state, incarnation);
        }
    }
}