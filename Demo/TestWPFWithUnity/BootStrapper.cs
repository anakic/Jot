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
using Thingie.Tracking.SessionEndNotification;

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
            _container.RegisterInstance(SettingsTracker.CreateTrackerForDesktop());
            _container.RegisterType<AppSettings>(new ContainerControlledLifetimeManager());//only one AppSettings object
            _container.AddExtension(new WPFTrackingExtension());//adds automatic tracking for all objects that can describe how they want to be persisted, as well as all WPF windows
        }
    }
}
