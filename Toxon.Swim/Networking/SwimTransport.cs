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
            var remoteEndpoint = args.RemoteEndpoint;

            switch (message.Type)
            {
                case SwimMessageType.Ping:
                    OnPing?.Invoke(this, new TransportPingEventArgs(message as PingMessage, remoteEndpoint));
                    break;
                case SwimMessageType.PingReq:
                    OnPingReq?.Invoke(this, new TransportPingReqEventArgs(message as PingReqMessage, remoteEndpoint));
                    break;
                case SwimMessageType.Ack:
                    OnAck?.Invoke(this, new TransportAckEventArgs(message as AckMessage, remoteEndpoint));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
