using Google.Protobuf;
using System;

namespace Shiko.Serialize.Protobuf
{
    public class Protobuf : ISerializer
    {
        public byte[] Marshal(object obj)
        {
            if (obj is IMessage message)
            {
                return message.ToByteArray();
            }
            throw new ArgumentException("Object must be a Protobuf IMessage.");
        }

        public void Unmarshal(byte[] data, object obj)
        {
            if (obj is IMessage message)
            {
                message.MergeFrom(data);
            }
            else
            {
                throw new ArgumentException("Object must be a Protobuf IMessage.");
            }
        }
    }
}
