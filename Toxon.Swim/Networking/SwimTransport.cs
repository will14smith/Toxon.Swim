using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toxon.Swim.Messages;
using Toxon.Swim.Models;
using Toxon.Swim.Serialization;
using Toxon.Swim.Services;

namespace Toxon.Swim.Networking
{
    public class SwimTransport
    {
        private readonly ITransport _transport;
        private readonly Disseminator _disseminator;
        private readonly IMessageSerializer _messageSerializer;

        public SwimTransport(ITransport transport, Disseminator disseminator, IMessageSerializer messageSerializer)
        {
            _transport = transport;
            _disseminator = disseminator;
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
            var piggybackMessages = _disseminator.GetMessages().ToList();
            var allMessages = messages.Concat(piggybackMessages);

            IReadOnlyCollection<ReadOnlyMemory<byte>> buffers = allMessages.Select(x => _messageSerializer.Serialize(x)).ToList();

            while (buffers.Count > piggybackMessages.Count)
            {
                var newBuffers = await _transport.SendAsync(buffers, host);
                if (newBuffers.Count == buffers.Count)
                {
                    throw new Exception("Failed to send first message");
                }
                
                buffers = newBuffers;
            }

            var numOfPiggybackMessagesSent = piggybackMessages.Count - buffers.Count;
            for (var i = 0; i < numOfPiggybackMessagesSent; i++)
            {
                _disseminator.MarkMessageAsSent(piggybackMessages[i]);
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
