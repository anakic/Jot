using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using System.IO;
using TestWPFWithUnity.Settings;
using Thingie.Tracking;
using Tracking.Tracking.Unity.Web.Desktop;
using Thingie.Tracking.DefaultObjectStoreUtil.SerializedStorage;
using Thingie.Tracking.DefaultObjectStoreUtil.Serialization;

namespace TestWPFWithUnity
{
    public class BootStrapper
    {
        IUnityContainer _container = new UnityContainer();

        public IUnityContainer Container
        {
            get { return _container; }
        }

        public void Initialize()
        {
            _container.RegisterInstance(new SettingsTracker(new FileStore(Environment.SpecialFolder.ApplicationData), new JsonSerializer()));
            _container.RegisterType<AppSettings>(new ContainerControlledLifetimeManager());
            _container.AddExtension(new WinFormsTrackingExtension());
        }
    }
}
