using System;
using System.Text;
using Google.FlatBuffers;
using Newtonsoft.Json;
using Shiko.Serialize;
using Shiko.Serialize.Flatbuffer;
using Shiko.Serialize.Json;
using Shiko.Serialize.Protobuf;

namespace Shiko.Internal.Packet
{
    internal class Ack : IFlatbufferMarshaler, IFlatbufferUnmarshaler
    {
        public string UID { get; set; }

        // Constructor
        public Ack()
        {
            UID = string.Empty;
        }

        // Constructor with string
        public Ack(string uid)
        {
            UID = uid ?? string.Empty;
        }
        public string Info()
        {
            return $"UID: {UID}";
        }

        public byte[] Marshal(ISerializer serializer)
        {
            switch (serializer)
            {
                case Json:
                    return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));

                case Protobuf protobuf:
                    return protobuf.Marshal(new PacketPb.Ack { Uid = UID });

                case Flatbuffer flatbuffer:
                    return flatbuffer.Marshal(this);

                default:
                    throw new NotSupportedException("Unsupported serializer");
            }
        }

        public void Unmarshal(ISerializer serializer, byte[] data)
        {
            switch (serializer)
            {
                case Json:
                    var jsonString = Encoding.UTF8.GetString(data);
                    var ack = JsonConvert.DeserializeObject<Ack>(jsonString);
                    UID = ack?.UID ?? string.Empty;
                    break;

                case Protobuf:
                    var protoAck = PacketPb.Ack.Parser.ParseFrom(data);
                    UID = protoAck.Uid;
                    break;

                case Flatbuffer:
                    var flatbufferAck = PacketFbs.Ack.GetRootAsAck(new ByteBuffer(data));
                    UID = Encoding.UTF8.GetString(flatbufferAck.GetUidArray());
                    break;

                default:
                    throw new NotSupportedException("Unsupported serializer");
            }
        }

        public byte[] MarshalFlatbuffer(FlatBufferBuilder builder)
        {
            var uidOffset = builder.CreateString(UID);
            PacketFbs.Ack.StartAck(builder);
            PacketFbs.Ack.AddUid(builder, uidOffset);

            var ackOffset = PacketFbs.Ack.EndAck(builder);
            builder.Finish(ackOffset.Value);

            return builder.SizedByteArray();
        }

        public void UnmarshalFlatbuffer(ByteBuffer data, int offset)
        {
            data.Position = offset;

            var ack = PacketFbs.Ack.GetRootAsAck(data);
            UID = Encoding.UTF8.GetString(ack.GetUidArray());
        }
    }
}
