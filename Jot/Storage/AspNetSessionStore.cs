using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Jot.Storage
{
    public class AspNetSessionStore : IObjectStore
    {
        public void Persist(object target, string key)
        {
            HttpContext.Current.Session[key] = target;
        }

        public object Retrieve(string key)
        {
            return HttpContext.Current.Session[key];
        }

        public void Remove(string key)
        {
            HttpContext.Current.Session.Remove(key);
        }

        public bool ContainsKey(string key)
        {
            return HttpContext.Current.Session.Keys.OfType<string>().Contains(key);
        }
    }
}
