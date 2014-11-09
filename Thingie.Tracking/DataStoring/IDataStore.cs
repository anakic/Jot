using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace Thingie.Tracking.DataStoring
{
    public interface IDataStore
    {
        bool ContainsKey(string identifier);
        byte[] GetData(string identifier);
        void SetData(byte [] data, string identifier);
        void RemoveData(string identifier);
    }
}
