using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toxon.Swim.Messages;
using Toxon.Swim.Models;

namespace Toxon.Swim.Networking
{
    public class SwimTransport
    {
        private readonly ITransport _transport;

        public SwimTransport(ITransport transport)
        {
            _transport = transport;
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

        public Task SendAsync(IReadOnlyCollection<SwimMessage> messages, SwimHost host)
        {
            return _transport.SendAsync(messages, host);
        }

        public Task StopAsync()
        {
            _transport.OnMessage -= HandleMessage;

            return _transport.StopAsync();
        }

        private void HandleMessage(object sender, TransportMessageEventArgs args)
        {
            var message = args.Message;
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
