using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Thingie.Tracking.Serialization
{
    public class JsonSerializer : ISerializer
    {
        JsonSerializerSettings _serializationSettings = new JsonSerializerSettings() { TypeNameHandling=TypeNameHandling.Objects };

        public byte[] Serialize(object obj)
        {
            return GetBytes(JsonConvert.SerializeObject(obj, Formatting.None, _serializationSettings));
        }

        public object Deserialize(byte[] bytes)
        {
            object obj = JsonConvert.DeserializeObject(GetString(bytes), _serializationSettings);
            
            //HACK: 
            //JSON.NET deserializes all Int values as Int64 because it's less likely to overflow.
            //This is a problem because reflection throws an exception when setting the Int64 value to a smaller
            //type (byte/Int16/Int32) property. The reverse (i.e. Int32 value -> Int64 property)is not a problem. 
            //Because of this I convert integer values to the smallest possible type.
            if (obj is Int64)
            {
                Int64 value = (Int64)obj;
                if (value >= 0 && value <= byte.MaxValue)
                    obj = Convert.ToByte(obj);
                else if (Math.Abs(value) <= Int16.MaxValue)
                    obj = Convert.ToInt16(obj);
                else if (Math.Abs(value) <= Int32.MaxValue)
                    obj = Convert.ToInt32(obj);
            }

            return obj;
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}
