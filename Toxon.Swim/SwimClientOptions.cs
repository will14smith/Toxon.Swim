using Serilog;
using Toxon.Swim.Serialization;

namespace Toxon.Swim
{
    public class SwimClientOptions
    {
        public IMessageSerializer MessageSerializer { get; set; } = new MessagePackMessageSerializer();
         
        public ILogger Logger { get; set; } = Log.Logger;
    }
}