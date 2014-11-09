using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Microsoft.Practices.Unity;
using Thingie.Tracking.Serialization;
using Thingie.Tracking.DataStoring;
using Thingie.Tracking;
using Thingie.Tracking.Unity;
using Thingie.Tracking.Unity.Web;

namespace TestWeb
{
    public class Global : System.Web.HttpApplication, IUnityContainerAccessor
    {
        public static UnityContainer _uc = new UnityContainer();

        public IUnityContainer Container
        {
            get { return _uc; }
        }
    }
}
