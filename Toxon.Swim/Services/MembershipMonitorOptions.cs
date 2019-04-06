using System;

namespace Toxon.Swim.Services
{
    public class MembershipMonitorOptions
    {
        public TimeSpan SuspectTimeout { get; set; } = TimeSpan.FromMilliseconds(10);
    }
}