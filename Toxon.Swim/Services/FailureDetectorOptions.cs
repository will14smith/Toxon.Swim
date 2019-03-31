using System;
using Serilog;

namespace Toxon.Swim.Services
{
    public class FailureDetectorOptions
    {
        public FailureDetectorOptions(ILogger logger)
        {
            Logger = logger.ForContext<FailureDetector>();
        }

        public ILogger Logger { get; }

        public TimeSpan PingTimeout { get; set; } = TimeSpan.FromMilliseconds(5);
        public TimeSpan PingReqTimeout { get; set; } = TimeSpan.FromMilliseconds(15);
        public TimeSpan PingInterval { get; set; } = TimeSpan.FromMilliseconds(25);
    }
}