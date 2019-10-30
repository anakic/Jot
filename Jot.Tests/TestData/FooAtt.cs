using Jot.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jot.Tests.TestData
{
    class FooAtt
    {
        [TrackingId]
        public int Int { get; set; }
        [Trackable]
        public double Double { get; set; }
        [Trackable]
        public TimeSpan Timespan { get; set; }

        [PersistOn]
        public event EventHandler RequestPersist;

        [StopTrackingOn]
        public event EventHandler StopTracking;

        public void FireRequest()
        {
            RequestPersist?.Invoke(this, EventArgs.Empty);

        }

        public void FireStop()
        {
            StopTracking?.Invoke(this, EventArgs.Empty);
        }
    }
}
