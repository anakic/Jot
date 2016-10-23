using System.Collections.Generic;

namespace Jot.Storage
{
    public abstract class PersistentStoreBase : IObjectStore
    {
        public Dictionary<string, object> _values;

        public bool ContainsKey(string identifier)
        {
            return _values.ContainsKey(identifier);
        }

        public void Set(object value, string key)
        {
            _values[key] = value;
        }

        public object Get(string key)
        {
            return _values[key];
        }

        public void Initialize()
        {
            _values = LoadValues();
        }

        public void CommitChanges()
        {
            SaveValues(_values);
        }

        protected abstract Dictionary<string, object> LoadValues();
        protected abstract void SaveValues(Dictionary<string, object> _values);
    }
}
