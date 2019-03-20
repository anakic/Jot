using System.Collections.Generic;

namespace Jot.Storage
{
    public interface IStore
    {
        void SetData(string id, IDictionary<string, object> values);
        IDictionary<string, object> GetData(string id);
    }
}
