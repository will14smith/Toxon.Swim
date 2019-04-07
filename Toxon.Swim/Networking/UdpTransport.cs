using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Toxon.Swim.Models;

namespace Toxon.Swim.Networking
{
    public class UdpTransport : ITransport
    {
        private readonly SwimHost _local;
        private readonly UdpTransportOptions _options;

        private UdpClient _udpClient;
        private Thread _listenerThread;
        private CancellationTokenSource _listenerThreadCancel;

        public UdpTransport(SwimHost local, UdpTransportOptions options)
        {
            _local = local;
            _options = options;
        }

        public event TransportMessageEvent OnMessage;

        public Task StartAsync()
        {
            _udpClient = new UdpClient(_local.AsIPEndPoint());

            if (_listenerThread != null)
            {
                throw new InvalidOperationException();
            }

            _listenerThreadCancel = new CancellationTokenSource();
            _listenerThread = new Thread(ListenerThread);
            _listenerThread.Start(_listenerThreadCancel.Token);

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _listenerThreadCancel.Cancel();
            _listenerThread = null;
            _udpClient?.Dispose();
            _udpClient = null;

            return Task.CompletedTask;
        }

        private async void ListenerThread(object arg)
        {
            var threadCancellationToken = (CancellationToken) arg;

            while (!threadCancellationToken.IsCancellationRequested)
            {
                UdpReceiveResult result;
                try
                {
                    result = await ReceiveAsync(threadCancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        continue;
                    }

                    throw;
                }

                var offset = 0;
                while (offset < result.Buffer.Length)
                {
                    var buffer = MessagePackBinary.ReadBytes(result.Buffer, offset, out var bytesConsumed);
                    offset += bytesConsumed;

                    OnMessage?.Invoke(this, new TransportMessageEventArgs(buffer, result.RemoteEndPoint));
                }
            }
        }

        private async Task<UdpReceiveResult> ReceiveAsync(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<UdpReceiveResult>();
            cancellationToken.Register(() => tcs.SetCanceled(), useSynchronizationContext: false);

            var receiveTask = _udpClient.ReceiveAsync();
            var cancellationTask = tcs.Task;

            var task = await Task.WhenAny(receiveTask, cancellationTask);

            if (task == cancellationTask)
            {
#pragma warning disable 4014
                receiveTask.ContinueWith(_ => receiveTask.Exception, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
#pragma warning restore 4014
            }

            return await task;
        }

        public async Task<IReadOnlyCollection<ReadOnlyMemory<byte>>> SendAsync(IReadOnlyCollection<ReadOnlyMemory<byte>> buffers, SwimHost host)
        {
            var output = new byte[_options.MaxDatagramSize];

            var remainingBuffers = new Queue<ReadOnlyMemory<byte>>(buffers);

            var length = 0;
            while (length < _options.MaxDatagramSize && remainingBuffers.Count > 0)
            {
                var buffer = remainingBuffers.Dequeue();

                var lengthDelta = MessagePackBinary.WriteBytes(ref output, length, buffer.ToArray());
                if (length + lengthDelta > _options.MaxDatagramSize)
                {
                    break;
                }

                length += lengthDelta;
            }

            if (length > 0)
            {
                await _udpClient.SendAsync(output, length, host.AsIPEndPoint());
            }

            return remainingBuffers.ToList();
        }

    }
}
