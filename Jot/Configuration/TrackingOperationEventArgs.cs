using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Jot.Configuration
{
    public class TrackingOperationEventArgs : CancelEventArgs
    {
        public TrackingConfiguration Configuration { get; private set; }

        public string Property { get; set; }

        public object Value { get; set; }

        public TrackingOperationEventArgs(TrackingConfiguration configuration, string property, object value)
        {
            Configuration = configuration;
            Property = property;
            Value = value;
        }
    }
}
