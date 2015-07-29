using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Eidetic.Configuration
{
    public class TrackingOperationEventArgs : CancelEventArgs
    {
        public TrackingConfiguration Configuration { get; private set; }

        public string Property { get; set; }

        public TrackingOperationEventArgs(TrackingConfiguration configuration, string property)
        {
            Configuration = configuration;
            Property = property;
        }
    }
}
