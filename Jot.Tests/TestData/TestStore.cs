using Jot.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jot.Tests.TestData
{
    class TestStore : IStore
    {
        Dictionary<string, IDictionary<string, object>> data = new Dictionary<string, IDictionary<string, object>>();

        public void ClearAll()
        {
            data.Clear();
        }

        public void ClearData(string id)
        {
            data.Remove(id);
        }

        public IDictionary<string, object> GetData(string id)
        {
            if (data.ContainsKey(id))
                return data[id];
            else
                return null;
        }

        public IEnumerable<string> ListIds()
            => data.Keys;

        public void SetData(string id, IDictionary<string, object> values)
        {
            data[id] = values;
        }
    }

}
