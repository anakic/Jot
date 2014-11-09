using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thingie.Tracking.Unity.Web.Desktop
{
    public class WinFormsTrackingExtension : TrackingExtension
    {
        protected override void CustomizeConfiguration(TrackingConfiguration configuration)
        {
            if(configuration.TargetReference.Target is Form)
                FormsHelper.ConfigureFormTracking(configuration);
            base.CustomizeConfiguration(configuration);
        }
    }
}
