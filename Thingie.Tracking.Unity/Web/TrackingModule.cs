using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.Practices.Unity;
using System.Web.SessionState;
using System.Web.UI;
using Thingie.Tracking.DefaultObjectStoreUtil.Serialization;
using Thingie.Tracking.DefaultObjectStoreUtil.SerializedStorage;

namespace Thingie.Tracking.Unity.Web
{
    /// <summary>
    /// Implement this interface in your ASP.NET application (Global.asax) to enable
    /// the tracking behavior
    /// </summary>
    public interface IUnityContainerAccessor
    {
        IUnityContainer Container { get; }
    }

    public static class AspNetTrackerNames
    {
        public const string SESSION = "SESSION";
        public const string USERPROFILE = "USER";
    }

    /// <summary>
    /// Module for adding tracking to ASP.NET pages. Needs to be registered in web.config.
    /// Nuget registers it automatically.
    /// </summary>
    public class TrackingModule : IHttpModule
    {
        static IUnityContainer _container;

        IUnityContainer _UC;
        [Dependency]
        public IUnityContainer UC
        {
            get { return _UC; }
            set { _UC = value; }
        }

        public TrackingModule()
        {

        }

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            if (_container == null)
            {
                var appAsUnityAccessor = (context as IUnityContainerAccessor);
                if (appAsUnityAccessor == null)
                    throw new Exception("The http application must implement \"Thingie.Tracking.Unity.ASPNET.IUnityContainerAccessor\" for the TrackingModule to work! Please implement the interface in global.asax.");

                _container = appAsUnityAccessor.Container;
                RegisterTrackers(_container);
                _container.AddExtension(new TrackingExtension());
            }

            context.PreRequestHandlerExecute += new EventHandler(context_PreRequestHandlerExecute);
            context.PostRequestHandlerExecute += new EventHandler(context_PostRequestHandlerExecute);
        }

        protected virtual void RegisterTrackers(IUnityContainer container)
        {
            //session level tracker - for properties with [Trackable(Name="SESSION")]
            _container.RegisterType<SettingsTracker>(AspNetTrackerNames.SESSION, new SessionLifetimeManager(), new InjectionFactory(iocCont => new SettingsTracker(new SessionStore(), new BinarySerializer(), null) { Name = AspNetTrackerNames.SESSION }));
            //user level tracker - for properties with [Trackable(Name="USER")]
            _container.RegisterType<SettingsTracker>(AspNetTrackerNames.USERPROFILE, new RequestLifetimeManager(), new InjectionFactory(c => new SettingsTracker(new ProfileStore(), new BinarySerializer(), null) { Name = AspNetTrackerNames.USERPROFILE }));
        }

        void context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            if (HttpContext.Current.Handler is Page)
            {
                Page page = HttpContext.Current.Handler as Page;
                _container.BuildUp(page.GetType(), page);
                page.InitComplete += (s, a) =>
                    {
                        foreach (Control c in GetControlTree(page))
                            _container.BuildUp(c.GetType(), c);
                    };
            }
        }

        void context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            if (HttpContext.Current.Handler is IRequiresSessionState || HttpContext.Current.Handler is IReadOnlySessionState)
            {
                //named trackers
                foreach (SettingsTracker tracker in _container.ResolveAll<SettingsTracker>())
                    tracker.PersistAll();

                //unnamed tracker
                if (_container.IsRegistered<SettingsTracker>())
                    _container.Resolve<SettingsTracker>().PersistAll();
            }
        }

        private IEnumerable<Control> GetControlTree(Control root)
        {
            foreach (Control child in root.Controls)
            {
                yield return child;
                foreach (Control c in GetControlTree(child))
                {
                    yield return c;
                }
            }
        }
    }
}
