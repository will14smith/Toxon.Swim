using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toxon.Swim.Messages;
using Toxon.Swim.Models;

namespace Toxon.Swim.Networking
{
    public interface ITransport
    {
        event TransportMessageEvent OnMessage;
        
        Task StartAsync();
        /// <summary>
        /// Sends as many buffers as possible and returns the un-sent ones
        /// </summary>
        Task<IReadOnlyCollection<ReadOnlyMemory<byte>>> SendAsync(IReadOnlyCollection<ReadOnlyMemory<byte>> buffers, SwimHost host);
        Task StopAsync();
    }
}