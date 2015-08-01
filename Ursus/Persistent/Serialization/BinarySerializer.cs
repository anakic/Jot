using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Ursus.Persistent.Serialization
{
    public class BinarySerializer : ISerializer
    {
        BinaryFormatter _formatter = new BinaryFormatter();

        public string Serialize(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                _formatter.Serialize(ms, obj);
                return Convert.ToBase64String(ms.GetBuffer());
            }
        }

        public object Deserialize(string serialized, Type originalType)
        {
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(serialized)))
            {
                return _formatter.Deserialize(ms);
            }
        }
    }
}
