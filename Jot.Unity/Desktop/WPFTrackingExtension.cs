using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Jot;
using Jot.Configuration;
using Jot.Unity;

namespace Tracking.Tracking.Unity.Web.Desktop
{
    public class WPFTrackingExtension : TrackingExtension
    {
        protected override void CustomizeConfiguration(TrackingConfiguration configuration)
        {
            Window window = configuration.TargetReference.Target as Window;
            if (window != null)
            {
                configuration
                    .AddProperties<Window>(w => w.Left, w => w.Top, w => w.Height, w => w.Width, w => w.WindowState)
                    .RegisterPersistTrigger("Closed");
            }
            base.CustomizeConfiguration(configuration);
        }
    }
}
