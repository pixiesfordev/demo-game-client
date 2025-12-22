namespace Shiko.Internal.Packet
{
    internal enum Type : byte
    {
        // Skip the 0 value
        HANDSHAKE = 0x01,
        ACK = 0x02,
        HEARTBEAT = 0x03,
        DATA = 0x04,
        KICK = 0x05
    }
}