using System.Collections.Generic;
using System.Threading.Tasks;
using Toxon.Swim.Messages;
using Toxon.Swim.Models;

namespace Toxon.Swim.Networking
{
    public interface ITransport
    {
        Task StartAsync();
        Task SendAsync(IReadOnlyCollection<SwimMessage> messages, SwimHost host);
        Task StopAsync();
    }
}