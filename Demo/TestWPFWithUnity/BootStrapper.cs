using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using System.IO;
using TestWPFWithUnity.Settings;
using Thingie.Tracking;
using Tracking.Tracking.Unity.Web.Desktop;
using Thingie.Tracking.Persistent.SerializedStorage;
using Thingie.Tracking.Persistent.Serialization;
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

            //only one AppSettings object
            _container.RegisterType<AppSettings>(new ContainerControlledLifetimeManager());

            //adds automatic tracking have [Trackable] attribute, or implement ITrackable + all WPF windows
            _container.AddExtension(new WPFTrackingExtension());
        }
    }
}
