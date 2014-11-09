using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using System.IO;
using TestWPFWithUnity.Settings;
using Thingie.Tracking.DataStoring;
using Thingie.Tracking.Serialization;
using Thingie.Tracking;
using Tracking.Tracking.Unity.Web.Desktop;

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
            _container.RegisterInstance(new SettingsTracker(new FileDataStore(Environment.SpecialFolder.ApplicationData), new JsonSerializer()));
            _container.RegisterType<AppSettings>(new ContainerControlledLifetimeManager());
            _container.AddExtension(new WPFTrackingExtension());
        }
    }
}
