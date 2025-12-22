using Shiko.Serialize;
using Shiko.Serialize.Json;

namespace Shiko.Internal.Serializer
{
    internal static class Serializer
    {
        // Singleton
        private static ISerializer serializer = new Json(); // Use Json serializer in default

        public static ISerializer GetSerializer()
        {
            return serializer;
        }

        public static void SetSerializer(ISerializer s)
        {
            serializer = s;
        }
    }
}