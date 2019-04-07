using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toxon.Swim.Messages;
using Toxon.Swim.Models;
using Toxon.Swim.Serialization;

namespace Toxon.Swim.Networking
{
    public class SwimTransport
    {
        private readonly ITransport _transport;
        private readonly IMessageSerializer _messageSerializer;

        public SwimTransport(ITransport transport, IMessageSerializer messageSerializer)
        {
            _transport = transport;
            _messageSerializer = messageSerializer;
        }

        public event TransportPingEvent OnPing;
        public event TransportPingReqEvent OnPingReq;
        public event TransportAckEvent OnAck;
        public event TransportSyncEvent OnSync;
        public event TransportUpdateEvent OnUpdate;

        public Task StartAsync()
        {
            _transport.OnMessage += HandleMessage;

            return _transport.StartAsync();
        }

        public async Task SendAsync(IReadOnlyCollection<SwimMessage> messages, SwimHost host)
        {
            IReadOnlyCollection<ReadOnlyMemory<byte>> buffers = messages.Select(x => _messageSerializer.Serialize(x)).ToList();

            while (buffers.Count > 0)
            {
                var newBuffers = await _transport.SendAsync(buffers, host);
                if (newBuffers.Count == buffers.Count)
                {
                    throw new Exception("Failed to send first message");
                }

                buffers = newBuffers;
            }
        }

        public Task StopAsync()
        {
            _transport.OnMessage -= HandleMessage;

            return _transport.StopAsync();
        }

        private void HandleMessage(object sender, TransportMessageEventArgs args)
        {
            var buffer = args.Buffer;
            var message = _messageSerializer.Deserialize(buffer.Span);

            var remote = new SwimHost(args.RemoteEndpoint);

            switch (message.Type)
            {
                case SwimMessageType.Ping:
                    OnPing?.Invoke(this, new TransportPingEventArgs(message as PingMessage, remote));
                    break;
                case SwimMessageType.PingReq:
                    OnPingReq?.Invoke(this, new TransportPingReqEventArgs(message as PingReqMessage, remote));
                    break;
                case SwimMessageType.Ack:
                    OnAck?.Invoke(this, new TransportAckEventArgs(message as AckMessage, remote));
                    break;
                case SwimMessageType.Sync:
                    OnSync?.Invoke(this, new TransportSyncEventArgs(message as SyncMessage, remote));
                    break;
                case SwimMessageType.Update:
                    OnUpdate?.Invoke(this, new TransportUpdateEventArgs(message as UpdateMessage, remote));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
