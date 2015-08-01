using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ursus;
using Ursus.Configuration;
using Ursus.Unity;

namespace Tracking.Tracking.Unity.Web.Desktop
{
    public class WinFormsTrackingExtension : TrackingExtension
    {
        protected override void CustomizeConfiguration(TrackingConfiguration configuration)
        {
            Form window = configuration.TargetReference.Target as Form;
            if (window != null)
            {
                configuration
                    .AddProperties<Form>(f => f.Left, f => f.Top, f => f.Height, f => f.Width, f => f.WindowState)
                    .RegisterPersistTrigger("Closed");
            }
            base.CustomizeConfiguration(configuration);
        }
    }
}
