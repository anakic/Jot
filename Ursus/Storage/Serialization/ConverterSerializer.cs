using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ursus.Storage.Serialization
{
    public class ConverterSerializer : ISerializer
    {
        public string Serialize(object obj)
        {
            return TypeDescriptor.GetConverter(obj).ConvertToString(obj);
        }

        public object Deserialize(string serialized, Type originalType)
        {
            return TypeDescriptor.GetConverter(originalType).ConvertFromString(serialized);
        }
    }
}
