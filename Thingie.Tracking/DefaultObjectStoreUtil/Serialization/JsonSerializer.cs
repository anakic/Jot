using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Thingie.Tracking.DefaultObjectStoreUtil.Serialization
{
    public class JsonSerializer : ISerializer
    {
        JsonSerializerSettings _serializationSettings = new JsonSerializerSettings() { TypeNameHandling=TypeNameHandling.Objects };

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None, _serializationSettings);
        }

        public object Deserialize(string serialized, Type originalType)
        {
            return JsonConvert.DeserializeObject(serialized, originalType, _serializationSettings);
        }
    }
}
