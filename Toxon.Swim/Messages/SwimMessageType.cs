namespace Toxon.Swim.Messages
{
    public enum SwimMessageType : byte
    {
        Ping = 1,
        PingReq = 2,
        Ack = 3,
        Sync = 4,
        Update = 5,
    }
}