using System;
namespace Jot.Storage
{
    /// <summary>
    /// Implementors of this interface delare that they can store and retrieve arbitrary objects. 
    /// SettingsTracker uses an implementation of this interface to store and retrieve property values. 
    /// <remarks>
    /// One could easily implement this interface to use for example MongoDB to save property values
    /// </remarks>
    /// </summary>
    public interface IObjectStore
    {
        bool ContainsKey(string key);
        void Set(object target, string key);
        object Get(string key);

        void Initialize();
        void CommitChanges();
    }
}
