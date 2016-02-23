using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Jot.Storage.Serialization
{
    public class JsonSerializer : ISerializer
    {
        public virtual string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public virtual object Deserialize(string serialized, Type originalType)
        {
            return JsonConvert.DeserializeObject(serialized, originalType);
        }
    }
}
