using System;
using System.Net;

namespace Toxon.Swim.Networking
{
    public delegate void TransportMessageEvent(object sender, TransportMessageEventArgs args);

    public class TransportMessageEventArgs
    {
        public ReadOnlyMemory<byte> Buffer { get; }
        public IPEndPoint RemoteEndpoint { get; }

        public TransportMessageEventArgs(ReadOnlyMemory<byte> buffer, IPEndPoint remoteEndpoint)
        {
            Buffer = buffer;
            RemoteEndpoint = remoteEndpoint;
        }
    }
}