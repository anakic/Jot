using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;

namespace Thingie.Tracking
{
    public static class FormsHelper
    {
        /// <summary>
        /// WinForms are tricky: if WindowState != Normal, size and location properties are bogus and shouldn't be persisted. 
        /// This helper configures the tracking so that size and location aren't persisted when WindowState != Normal. 
        /// </summary>
        /// <param name="formTrackingConfig"></param>
        /// <returns></returns>
        public static TrackingConfiguration ConfigureFormTracking(this Form form, SettingsTracker tracker)
        {
            return ConfigureFormTracking(tracker.Configure(form));
        }

        /// <summary>
        /// WinForms are tricky: if WindowState != Normal, size and location properties are bogus and shouldn't be persisted. 
        /// This helper configures the tracking so that size and location aren't persisted when WindowState != Normal. 
        /// </summary>
        /// <param name="formTrackingConfig"></param>
        /// <returns></returns>
        public static TrackingConfiguration ConfigureFormTracking(TrackingConfiguration formTrackingConfig)
        {
            if (formTrackingConfig.TargetReference.Target is Form)
            {
                Expression<Func<Form, object>>[] propertiesToTrack = { f => f.Height, f => f.Width, f => f.Left, f => f.Top, f => f.WindowState };

                formTrackingConfig.
                    AddProperties<Form>(propertiesToTrack)
                    .RegisterPersistTrigger("Resize")
                    .RegisterPersistTrigger("Move");

                formTrackingConfig.PersistingState += (s, e) =>
                {
                    if ((e.Configuration.TargetReference.Target as Form).WindowState == FormWindowState.Normal)
                        e.Configuration.AddProperties<Form>(propertiesToTrack);
                    else
                    {
                        e.Configuration.RemoveProperties(propertiesToTrack);
                        e.Configuration.AddProperties<Form>(f => f.WindowState);
                    }
                };

                return formTrackingConfig;
            }
            else
                throw new ArgumentException("Invalid configuration object passed. Only form config objects are allowed.");
        }
    }
}
