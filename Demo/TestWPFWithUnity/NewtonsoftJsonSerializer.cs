using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json;
using Jot.Storage.Serialization;

namespace TestWPFWithUnity
{
    /// <summary>
    /// Since the default .NET implementation can't serialize fonts and ColumnWidths
    /// I use Newtonsoft.Json serializer which doesn't have any issues serializing these types.
    /// I did not want to have a dependency to another library so I left it out of the library.
    /// </summary>
    class NewtonsoftJsonSerializer : ISerializer
    {
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public object Deserialize(string serialized, Type originalType)
        {
            return JsonConvert.DeserializeObject(serialized, originalType);
        }
    }
}
