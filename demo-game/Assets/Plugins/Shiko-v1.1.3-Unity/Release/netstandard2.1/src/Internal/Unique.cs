using System;
using System.Collections.Generic;
using System.Text;

namespace Shiko.Internal.Unique
{
    internal class Unique
    {
        private static readonly Unique instance = new Unique();
        private Dictionary<string, ulong> counter = new Dictionary<string, ulong>();
        private readonly object lockObject = new object();
        private static readonly uint[] CRC32Table = CreateCRC32Table();

        private Unique() { }
        public static Unique Instance => instance;

        public static string Generate(string key)
        {
            lock (instance.lockObject)
            {
                if (!instance.counter.ContainsKey(key))
                    instance.counter[key] = 0;

                // Avoid overflow
                if (instance.counter[key] == ulong.MaxValue)
                    instance.counter[key] = 0;

                instance.counter[key]++;

                return string.Format(
                    "{0}-{1}-{2}",
                    CalculateCRC32(key),
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1_000_000,
                    instance.counter[key]
                );
            }
        }

        private static uint CalculateCRC32(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            uint crc = 0xFFFFFFFF;

            for (int i = 0; i < bytes.Length; i++)
                crc = (crc >> 8) ^ CRC32Table[(crc ^ bytes[i]) & 0xFF];

            return ~crc;
        }

        private static uint[] CreateCRC32Table()
        {
            uint[] table = new uint[256];
            uint polynomial = 0xEDB88320;

            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;

                for (int j = 0; j < 8; j++)
                    crc = (crc & 1) == 1 ? (crc >> 1) ^ polynomial : crc >> 1;

                table[i] = crc;
            }

            return table;
        }
    }
}
