using System;
namespace Thingie.Tracking.DataStoring
{
    /// <summary>
    /// <remarks>
    /// One could easily implement this interface to use 
    /// for example MongoDB to save property values
    /// </remarks>
    /// </summary>
    public interface IObjectStore
    {
        bool ContainsKey(string key);
        void Persist(object target, string key);
        object Retrieve(string key);
    }
}
