namespace Toxon.Swim.Messages
{
    public class AckMessage : SwimMessage
    {
        public AckMessage(ulong sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
        }

        public override SwimMessageType Type => SwimMessageType.Ping;

        public ulong SequenceNumber { get; }
    }
}
