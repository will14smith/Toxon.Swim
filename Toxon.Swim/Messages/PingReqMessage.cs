using Toxon.Swim.Models;

namespace Toxon.Swim.Messages
{
    public class PingReqMessage : SwimMessage
    {
        public PingReqMessage(ulong sequenceNumber, SwimHost destination)
        {
            SequenceNumber = sequenceNumber;
            Destination = destination;
        }

        public override SwimMessageType Type => SwimMessageType.PingReq;

        public ulong SequenceNumber { get; }
        public SwimHost Destination { get; }
    }
}