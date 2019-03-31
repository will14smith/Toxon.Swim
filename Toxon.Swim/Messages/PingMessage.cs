namespace Toxon.Swim.Messages
{
    public class PingMessage : SwimMessage
    {
        public PingMessage(ulong sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
        }

        public override SwimMessageType Type => SwimMessageType.Ping;

        public ulong SequenceNumber { get; }
    }
}