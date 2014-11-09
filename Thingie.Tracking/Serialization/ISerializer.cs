using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.ComponentModel;

namespace Thingie.Tracking.Serialization
{
    public interface ISerializer
    {
        byte[] Serialize(object obj);
        object Deserialize(byte[] bytes);
    }
}
