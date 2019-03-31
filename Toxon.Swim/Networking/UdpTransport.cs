using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toxon.Swim.Messages;
using Toxon.Swim.Models;

namespace Toxon.Swim.Networking
{
    public class UdpTransport : ITransport
    {
        private Thread _listenerThread;

        private readonly UdpClient _udpClient;

        private readonly SwimClient _swim;
        private readonly UdpTransportOptions _options;

        public UdpTransport(SwimClient swim, UdpTransportOptions options)
        {
            _udpClient = new UdpClient(options.Local.AsIPEndPoint());

            _swim = swim;
            _options = options;
        }

        public Task StartAsync()
        {
            if (_listenerThread != null)
            {
                throw new InvalidOperationException();
            }

            _listenerThread = new Thread(ListenerThread);
            _listenerThread.Start();

            return Task.CompletedTask;
        }

        private void ListenerThread()
        {
            while (true)
            {
                IPEndPoint remoteEndpoint = null;
                var result = _udpClient.Receive(ref remoteEndpoint);
                
                _options.Logger.Debug("Received buffer from {from} with a length of {length} bytes", remoteEndpoint, result.Length);

                throw new NotImplementedException();
            }
        }

        public async Task SendAsync(IReadOnlyCollection<SwimMessage> messages, SwimHost host)
        {
            var buffers = messages.Select(x => _options.MessageSerializer.Serialize(x));
            
            // TODO try and bundle buffers into same datagram
            foreach (var buffer in buffers)
            {
                var remoteEndpoint = host.AsIPEndPoint();
                _options.Logger.Debug("Sending a buffer to {to} with a length of {length} bytes", remoteEndpoint, buffer.Length);

                await _udpClient.SendAsync(buffer.ToArray(), buffer.Length, remoteEndpoint);
            }
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }
    }
}
