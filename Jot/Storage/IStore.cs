using System.Collections.Generic;

namespace Jot.Storage
{
    public interface IStore
    {
        IEnumerable<string> ListIds();
        void SetData(string id, IDictionary<string, object> values);
        IDictionary<string, object> GetData(string id);
        void ClearData(string id);
        void ClearAll();
    }
}
