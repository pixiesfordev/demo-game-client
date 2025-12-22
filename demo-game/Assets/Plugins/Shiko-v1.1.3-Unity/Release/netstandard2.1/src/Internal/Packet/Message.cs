using System;
using System.Text;

namespace Shiko.Internal.Packet
{
    // Message class for bytes or string data serialization
    internal class Message
    {
        public byte[] Raw { get; set; }

        // Constructor for empty message
        public Message()
        {
            Raw = Array.Empty<byte>();
        }

        // Constructor for bytes data
        public Message(byte[] data)
        {
            Raw = data ?? Array.Empty<byte>();
        }

        // Constructor for string data
        public Message(string data)
        {
            Raw = string.IsNullOrEmpty(data) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(data);
        }
    }
}