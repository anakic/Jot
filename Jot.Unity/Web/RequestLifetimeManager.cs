using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using System.Web;

namespace Jot.Unity.Web
{
    public class RequestLifetimeManager : LifetimeManager
    {
        private string _key = Guid.NewGuid().ToString();

        public override object GetValue()
        {
            return HttpContext.Current.Items[_key];
        }

        public override void SetValue(object value)
        {
            HttpContext.Current.Items[_key] = value;
        }

        public override void RemoveValue()
        {
            HttpContext.Current.Items.Remove(_key);
        }
    }
}
