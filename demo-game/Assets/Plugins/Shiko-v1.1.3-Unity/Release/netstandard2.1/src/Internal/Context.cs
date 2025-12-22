using System.Text;
using Newtonsoft.Json;

#nullable enable
namespace Shiko.Context
{
    public class Context
    {
        private readonly byte[] data;

        public Context(byte[] data)
        {
            this.data = data;
        }

        // Deserializes the data into the provided type.
        public T? Struct<T>() where T : class
        {
            var json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(json);
        }

        // Returns the data as a byte array.
        public byte[] Bytes()
        {
            return data;
        }

        // Returns the data as a string, trimmed of quotes.
        public string String()
        {
            var jsonString = Encoding.UTF8.GetString(data);
            return jsonString.Trim('"');
        }
    }
}
