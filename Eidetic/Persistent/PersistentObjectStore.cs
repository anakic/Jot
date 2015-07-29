using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eidetic.Persistent.Serialization;
using Eidetic.Persistent.SerializedStorage;

namespace Eidetic.Persistent
{
    public class PersistentObjectStore : IObjectStore
    {
        public IDataStore DataStore { get; private set; }
        public ISerializer Serializer { get; private set; }
        public bool CacheObjects { get; set; }
        public bool RemoveBadData { get; set; }

        Dictionary<string, object> _createdInstances = new Dictionary<string, object>();

        public PersistentObjectStore(IDataStore dataStore, ISerializer serializer)
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
                DataStore.SetData(null, key);//todo: handle null in datastore
            else
                DataStore.SetData(new StoreData(Serializer.Serialize(target), target.GetType()), key);
        }

        public bool ContainsKey(string key)
        {
            return DataStore.ContainsKey(key);
        }

        public object Retrieve(string key)
        {
            if (!CacheObjects || !_createdInstances.ContainsKey(key))
            {
                try
                {
                    StoreData data = DataStore.GetData(key);
                    if (data.Serialized == null)
                        return null;
                    else
                        _createdInstances[key] = Serializer.Deserialize(data.Serialized, data.OriginalType);
                }
                catch
                {
                    if (RemoveBadData)
                        DataStore.RemoveData(key);
                    throw;
                }
            }
            return _createdInstances[key];
        }

        public void Remove(string key)
        {
            DataStore.RemoveData(key);
        }
    }
}
