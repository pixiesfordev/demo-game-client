using Newtonsoft.Json;
using Newtonsoft.Json.Utilities;
using UnityEngine;

namespace Shiko.Serialize.Json
{
    public class Json : ISerializer
    {
        public byte[] Marshal(object obj)
        {
            var jsonString = JsonConvert.SerializeObject(obj);
            return System.Text.Encoding.UTF8.GetBytes(jsonString);
        }

        public void Unmarshal(byte[] data, object obj)
        {
            var jsonString = System.Text.Encoding.UTF8.GetString(data);
            var json = JsonConvert.DeserializeObject(jsonString, obj.GetType());
            if (json != null)
            {
                foreach (var property in obj.GetType().GetProperties())
                {
                    var value = property.GetValue(json);
                    property.SetValue(obj, value);
                }
            }
        }
    }
}

// Using AotHelper to enforce precompilation of certain JSON types
public class AotTypeEnforcer : MonoBehaviour
{
    public void Awake()
    {
        AotHelper.EnsureList<int>();
    }
}
