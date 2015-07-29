using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace Eidetic.Persistent.SerializedStorage
{
    public class StoreData
    {
        public string Serialized { get; set; }
        public Type OriginalType { get; set; }

        public StoreData(string data, Type originalType)
        {
            Serialized = data;
            OriginalType = originalType;
        }

        public static StoreData NullData = new StoreData(null, typeof(object));
    }

    public interface IDataStore
    {
        bool ContainsKey(string identifier);
        StoreData GetData(string identifier);
        void SetData(StoreData data, string identifier);
        void RemoveData(string identifier);
    }
}
