using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thingie.Tracking.Serialization;

namespace Thingie.Tracking.DataStoring
{
    public class ObjectStore : IObjectStore
    {
        public IDataStore DataStore { get; private set; }
        public ISerializer Serializer { get; private set; }
        public bool CacheObjects { get; set; }
        public bool RemoveBadData { get; set; }

        Dictionary<string, object> _createdInstances = new Dictionary<string, object>();

        public ObjectStore(IDataStore dataStore, ISerializer serializer)
        {
            DataStore = dataStore;
            Serializer = serializer;
            CacheObjects = true;
            RemoveBadData = true;
        }

        public void Persist(object target, string key)
        {
            _createdInstances[key] = target;
            if (target == null)
                DataStore.RemoveData(key);
            else
                DataStore.SetData(Serializer.Serialize(target), key);
        }

        public bool ContainsKey(string key)
        {
            return DataStore.ContainsKey(key);
        }

        public object Retrieve(string key)
        {
            if (!CacheObjects || !_createdInstances.ContainsKey(key))
            {
                object obj = null;
                try 
                { 
                    obj = Serializer.Deserialize(DataStore.GetData(key)); 
                }
                catch 
                { 
                    if(RemoveBadData)
                        DataStore.RemoveData(key);
                }
                _createdInstances[key] = obj;
            }
            return _createdInstances[key];
        }
    }
}
