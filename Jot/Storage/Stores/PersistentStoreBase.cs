using System.Collections.Generic;

namespace Jot.Storage
{
    /// <summary>
    /// Base class for objects that serialize and persist data. Keeps all data in a dictionary, and loads/saves the dictionary when necessary.
    /// </summary>
    public abstract class PersistentStoreBase : IStore
    {
        Dictionary<string, object> _values;

        private Dictionary<string, object> Values
        {
            get
            {
                return _values ?? (_values = LoadValues());
            }
        }

        /// <summary>
        /// Indicates if the store contains data for the specified key.
        /// </summary>
        /// <param name="identifier">The identifier too look for.</param>
        /// <returns>True if the store contains data for the specified key, otherwise False.</returns>
        public bool ContainsKey(string identifier)
        {
            return Values.ContainsKey(identifier);
        }
        /// <summary>
        /// Stores a value for the specified key.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <param name="key">The key that identifies the stored value, i.e. the name of the property whose value is stored.</param>
        public void Set(object value, string key)
        {
            Values[key] = value;
        }
        /// <summary>
        /// Gets the value stored under the specified key.
        /// </summary>
        /// <param name="key">The key that identifies the value to return.</param>
        /// <returns>The value stored under the specified key</returns>
        public object Get(string key)
        {
            return Values[key];
        }
        
        /// <summary>
        /// Commits the new data to the store. For a file-based store, this is where data should be written to the file.
        /// </summary>
        public void CommitChanges()
        {
            SaveValues(Values);
        }
        /// <summary>
        /// Loads values from the backing storage into a dictionary.
        /// </summary>
        /// <returns>A dictionary with the retrieved values.</returns>
        protected abstract Dictionary<string, object> LoadValues();
        /// <summary>
        /// Persists the dictionary of values to a backing store.
        /// </summary>
        /// <param name="values">The values to store.</param>
        protected abstract void SaveValues(Dictionary<string, object> values);
    }
}
