namespace Toxon.Swim.Messages
{
    public abstract class SwimMessage
    {
        public abstract SwimMessageType Type { get; }
    }

    public enum SwimMessageType
    {
        Ping = 1,
        PingReq = 2,
        Ack = 3
    }
}
