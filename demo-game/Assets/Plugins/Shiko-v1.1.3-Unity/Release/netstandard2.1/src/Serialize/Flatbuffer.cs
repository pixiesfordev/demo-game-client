using System;
using Google.FlatBuffers;

namespace Shiko.Serialize.Flatbuffer
{
    public class Flatbuffer : ISerializer
    {
        public static readonly Exception ErrWrongValueType = new Exception("flatbuffer: convert on wrong type value");

        public byte[] Marshal(object v)
        {
            if (v is IFlatbufferMarshaler marshaler)
            {
                var builder = new FlatBufferBuilder(1024);
                return marshaler.MarshalFlatbuffer(builder);
            }
            else
            {
                throw ErrWrongValueType;
            }
        }

        public void Unmarshal(byte[] data, object v)
        {
            if (v is IFlatbufferUnmarshaler unmarshaler)
            {
                var buffer = new ByteBuffer(data);
                unmarshaler.UnmarshalFlatbuffer(buffer, data.Length);
            }
            else
            {
                throw ErrWrongValueType;
            }
        }
    }

    public interface IFlatbufferMarshaler
    {
        byte[] MarshalFlatbuffer(FlatBufferBuilder builder);
    }

    public interface IFlatbufferUnmarshaler
    {
        void UnmarshalFlatbuffer(ByteBuffer buffer, int offset);
    }
}
