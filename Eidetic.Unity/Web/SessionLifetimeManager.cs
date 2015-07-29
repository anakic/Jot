using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using System.Web;

namespace Eidetic.Unity.Web
{
    public class SessionLifetimeManager : LifetimeManager
    {
        private string _key = Guid.NewGuid().ToString();

        public override object GetValue()
        {
            return HttpContext.Current.Session[_key];
        }

        public override void SetValue(object value)
        {
            HttpContext.Current.Session[_key] = value;
        }

        public override void RemoveValue()
        {
            HttpContext.Current.Session.Remove(_key);
        }
    }
}
