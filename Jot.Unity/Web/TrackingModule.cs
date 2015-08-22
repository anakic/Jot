using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.Practices.Unity;
using System.Web.SessionState;
using System.Web.UI;
using Jot.Storage.Serialization;
using Jot.Storage;

namespace Jot.Unity.Web
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
            _container.RegisterType<StateTracker>(AspNetTrackerNames.SESSION, new SessionLifetimeManager(), new InjectionFactory(iocCont => new StateTracker(new SessionStore(), null) { Name = AspNetTrackerNames.SESSION }));
            //user level tracker - for properties with [Trackable(Name="USER")]
            _container.RegisterType<StateTracker>(AspNetTrackerNames.USERPROFILE, new RequestLifetimeManager(), new InjectionFactory(c => new StateTracker(new ProfileStore(), null) { Name = AspNetTrackerNames.USERPROFILE }));
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
                foreach (StateTracker tracker in _container.ResolveAll<StateTracker>())
                    tracker.RunAutoPersist();

                //unnamed tracker
                if (_container.IsRegistered<StateTracker>())
                    _container.Resolve<StateTracker>().RunAutoPersist();
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
