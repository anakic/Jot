using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using System.IO;
using TestWPFWithUnity.Settings;
using Jot;
using Tracking.Tracking.Unity.Web.Desktop;
using Jot.Storage;

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
            _container.RegisterInstance(
                new StateTracker(
                    new FileStore(Environment.SpecialFolder.ApplicationData) { Serializer = new NewtonsoftJsonSerializer() },
                    new Jot.Triggers.DesktopPersistTrigger()));

            //singleton AppSettings object
            _container.RegisterType<AppSettings>(new ContainerControlledLifetimeManager());

            //adds automatic tracking have [Trackable] attribute, or implement ITrackable + all WPF windows
            _container.AddExtension(new WPFTrackingExtension());
        }
    }
}
