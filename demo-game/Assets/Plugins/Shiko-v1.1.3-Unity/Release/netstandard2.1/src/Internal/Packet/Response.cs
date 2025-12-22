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
    internal class Response : IFlatbufferMarshaler, IFlatbufferUnmarshaler
    {
        public string MID { get; set; }
        public string Route { get; set; }
        public byte[] Data { get; set; }

        // Default constructor
        public Response()
        {
            MID = string.Empty;
            Route = string.Empty;
            Data = Array.Empty<byte>();
        }

        // Constructor with parameters
        public Response(string mid, string route, byte[] data)
        {
            MID = mid ?? string.Empty;
            Route = route ?? string.Empty;
            Data = data ?? Array.Empty<byte>();
        }

        public string Info()
        {
            return $"MID: {MID}, Route: {Route}, Data: {BitConverter.ToString(Data)}";
        }

        public byte[] Marshal(ISerializer serializer)
        {
            switch (serializer)
            {
                case Json:
                    return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));

                case Protobuf protobuf:
                    return protobuf.Marshal(new PacketPb.Response
                    {
                        Mid = MID,
                        Route = Route,
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
                case Json:
                    var jsonString = Encoding.UTF8.GetString(data);
                    var response = JsonConvert.DeserializeObject<Response>(jsonString);
                    if (response != null)
                    {
                        MID = response.MID;
                        Route = response.Route;
                        Data = response.Data;
                    }
                    break;

                case Protobuf:
                    var protoResponse = PacketPb.Response.Parser.ParseFrom(data);
                    MID = protoResponse.Mid;
                    Route = protoResponse.Route;
                    Data = protoResponse.Data.ToByteArray();
                    break;

                case Flatbuffer:
                    var flatbufferResponse = PacketFbs.Response.GetRootAsResponse(new ByteBuffer(data));
                    MID = flatbufferResponse.Mid;
                    Route = flatbufferResponse.Route;
                    Data = flatbufferResponse.GetDataArray();
                    break;

                default:
                    throw new NotSupportedException("Unsupported serializer");
            }
        }

        public byte[] MarshalFlatbuffer(FlatBufferBuilder builder)
        {
            var midOffset = builder.CreateString(MID);
            var routeOffset = builder.CreateString(Route);
            var dataOffset = PacketFbs.Response.CreateDataVector(builder, Data);

            PacketFbs.Response.StartResponse(builder);
            PacketFbs.Response.AddMid(builder, midOffset);
            PacketFbs.Response.AddRoute(builder, routeOffset);
            PacketFbs.Response.AddData(builder, dataOffset);

            var responseOffset = PacketFbs.Response.EndResponse(builder);
            builder.Finish(responseOffset.Value);

            return builder.SizedByteArray();
        }

        public void UnmarshalFlatbuffer(ByteBuffer data, int offset)
        {
            data.Position = offset;

            var response = PacketFbs.Response.GetRootAsResponse(data);
            MID = response.Mid;
            Route = response.Route;
            Data = response.GetDataArray();
        }
    }
}