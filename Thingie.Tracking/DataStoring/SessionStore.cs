using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Thingie.Tracking.DataStoring
{
    public class SessionStore : IDataStore
    {
        #region IDataStore Members

        public bool ContainsKey(string identifier)
        {
            return HttpContext.Current.Session!= null && HttpContext.Current.Session.Keys.OfType<object>().Contains(identifier);
        }

        public byte[] GetData(string identifier)
        {
            return (byte[])HttpContext.Current.Session[identifier];
        }

        public void SetData(byte[] data, string identifier)
        {
            HttpContext.Current.Session[identifier] = data;
        }

        public void RemoveData(string identifier)
        {
            HttpContext.Current.Session.Remove(identifier);
        }
        #endregion
    }
}
