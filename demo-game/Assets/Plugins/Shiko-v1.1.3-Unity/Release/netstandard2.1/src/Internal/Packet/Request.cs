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
    internal class Request : IFlatbufferMarshaler, IFlatbufferUnmarshaler
    {
        public string MID { get; set; }
        public string CMD { get; set; }
        public byte[] Data { get; set; }

        // Default constructor
        public Request()
        {
            MID = string.Empty;
            CMD = string.Empty;
            Data = Array.Empty<byte>();
        }

        // Constructor with parameters
        public Request(string mid, string cmd, byte[] data)
        {
            MID = mid ?? string.Empty;
            CMD = cmd ?? string.Empty;
            Data = data ?? Array.Empty<byte>();
        }

        public string Info()
        {
            return $"MID: {MID}, CMD: {CMD}, Data: {BitConverter.ToString(Data)}";
        }

        public byte[] Marshal(ISerializer serializer)
        {
            switch (serializer)
            {
                case Json:
                    return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));

                case Protobuf protobuf:
                    return protobuf.Marshal(new PacketPb.Request
                    {
                        Mid = MID,
                        Cmd = CMD,
                        Data = Google.Protobuf.ByteString.CopyFrom(Data)
                    });

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
                case Json json:
                    var jsonString = Encoding.UTF8.GetString(data);
                    var request = JsonConvert.DeserializeObject<Request>(jsonString);
                    if (request != null)
                    {
                        MID = request.MID;
                        CMD = request.CMD;
                        Data = request.Data;
                    }
                    break;

                case Protobuf protobuf:
                    var protoRequest = PacketPb.Request.Parser.ParseFrom(data);
                    MID = protoRequest.Mid;
                    CMD = protoRequest.Cmd;
                    Data = protoRequest.Data.ToByteArray();
                    break;

                case Flatbuffer flatbuffer:
                    var flatbufferRequest = PacketFbs.Request.GetRootAsRequest(new ByteBuffer(data));
                    MID = flatbufferRequest.Mid;
                    CMD = flatbufferRequest.Cmd;
                    Data = flatbufferRequest.GetDataArray();
                    break;

                default:
                    throw new NotSupportedException("Unsupported serializer");
            }
        }

        public byte[] MarshalFlatbuffer(FlatBufferBuilder builder)
        {
            var midOffset = builder.CreateString(MID);
            var cmdOffset = builder.CreateString(CMD);
            var dataOffset = PacketFbs.Request.CreateDataVector(builder, Data);

            PacketFbs.Request.StartRequest(builder);
            PacketFbs.Request.AddMid(builder, midOffset);
            PacketFbs.Request.AddCmd(builder, cmdOffset);
            PacketFbs.Request.AddData(builder, dataOffset);

            var requestOffset = PacketFbs.Request.EndRequest(builder);
            builder.Finish(requestOffset.Value);

            return builder.SizedByteArray();
        }

        public void UnmarshalFlatbuffer(ByteBuffer data, int offset)
        {
            data.Position = offset;

            var request = PacketFbs.Request.GetRootAsRequest(data);
            MID = request.Mid;
            CMD = request.Cmd;
            Data = request.GetDataArray();
        }
    }
}