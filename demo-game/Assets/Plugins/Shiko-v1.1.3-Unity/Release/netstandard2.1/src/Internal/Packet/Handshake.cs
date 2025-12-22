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
    internal class Handshake : IFlatbufferMarshaler, IFlatbufferUnmarshaler
    {
        public string UID { get; set; }

        // Constructor
        public Handshake()
        {
            UID = string.Empty;
        }

        // Constructor with string
        public Handshake(string uid)
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
                    return protobuf.Marshal(new PacketPb.Handshake { Uid = UID });

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
                    var handshake = JsonConvert.DeserializeObject<Handshake>(jsonString);
                    UID = handshake?.UID ?? string.Empty;
                    break;

                case Protobuf:
                    var protoHandshake = PacketPb.Handshake.Parser.ParseFrom(data);
                    UID = protoHandshake.Uid;
                    break;

                case Flatbuffer:
                    var flatbufferHandshake = PacketFbs.Handshake.GetRootAsHandshake(new ByteBuffer(data));
                    UID = Encoding.UTF8.GetString(flatbufferHandshake.GetUidArray());
                    break;

                default:
                    throw new NotSupportedException("Unsupported serializer");
            }
        }

        public byte[] MarshalFlatbuffer(FlatBufferBuilder builder)
        {
            var uidOffset = builder.CreateString(UID);
            PacketFbs.Handshake.StartHandshake(builder);
            PacketFbs.Handshake.AddUid(builder, uidOffset);

            var ackOffset = PacketFbs.Handshake.EndHandshake(builder);
            builder.Finish(ackOffset.Value);

            return builder.SizedByteArray();
        }

        public void UnmarshalFlatbuffer(ByteBuffer data, int offset)
        {
            data.Position = offset;

            var handshake = PacketFbs.Handshake.GetRootAsHandshake(data);
            UID = Encoding.UTF8.GetString(handshake.GetUidArray());
        }
    }
}
