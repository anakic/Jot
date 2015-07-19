using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.ComponentModel;

namespace Thingie.Tracking.DefaultObjectStoreUtil.Serialization
{
    public interface ISerializer
    {
        string Serialize(object obj);
        object Deserialize(string serialized, Type originalType);
    }
}
