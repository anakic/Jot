namespace Jot.Storage
{
    /// <summary>
    /// Implementors of this interface delare that they can store and retrieve arbitrary objects. 
    /// SettingsTracker uses an implementation of this interface to store and retrieve property values. 
    /// <remarks>
    /// One could easily implement this interface to use for example MongoDB to save property values
    /// </remarks>
    /// </summary>
    public interface IStore
    {
        /// <summary>
        /// Indicates if the store contains data for the specified key.
        /// </summary>
        /// <param name="key">The key too look for.</param>
        /// <returns>True if the store contains data for the specified key, otherwise False.</returns>
        bool ContainsKey(string key);
        /// <summary>
        /// Stores a value for the specified key.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <param name="key">The key that identifies the stored value, i.e. the name of the property whose value is stored.</param>
        void Set(object value, string key);
        /// <summary>
        /// Gets the value stored under the specified key.
        /// </summary>
        /// <param name="key">The key that identifies the value to return.</param>
        /// <returns>The value stored under the specified key</returns>
        object Get(string key);
        /// <summary>
        /// Commits the new data to the store. For a file-based store, this is where data should be written to the file.
        /// </summary>
        void CommitChanges();
    }
}
