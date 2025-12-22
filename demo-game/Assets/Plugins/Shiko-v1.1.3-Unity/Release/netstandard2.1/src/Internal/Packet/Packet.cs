using System;
using System.IO;
using System.Text;

namespace Shiko.Internal.Packet
{
    class Packet
    {
        public Type Type { get; set; }
        public int Length { get; set; }
        public byte[] Data { get; set; }

        public const int HeaderSize = 5;

        // Constructor for empty packet
        public Packet()
        {
            Data = Array.Empty<byte>();
        }

        // Constructor for complete packet
        public Packet(Type type, int length, byte[] data)
        {
            Type = type;
            Length = length;
            Data = data;
        }

        public string Info()
        {
            return $"Type: {Type}, Length: {Length}, Data: {BitConverter.ToString(Data)}";
        }

        public byte[] ToBytes()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.BigEndianUnicode))
            {
                // Type (1-byte)
                writer.Write((byte)Type);

                byte[] lengthBytes = BitConverter.GetBytes(Length);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(lengthBytes);

                // Length (4-bytes)
                writer.Write(lengthBytes);

                // Data (N-bytes)
                writer.Write(Data);

                return stream.ToArray();
            }
        }

        public void ReadHeader(byte[] data)
        {
            if (data.Length != HeaderSize)
                throw new InvalidOperationException("Invalid header size");

            // Type (1-byte)
            Type = (Type)data[0];

            // Read the Length field in big-endian format
            Length = BitConverter.ToInt32(data, 1);
            if (BitConverter.IsLittleEndian)
                // Reverse the byte order if the system is little-endian
                Array.Reverse(data, 1, 4);

            // Length (4-bytes)
            Length = BitConverter.ToInt32(data, 1);
        }
    }
}
