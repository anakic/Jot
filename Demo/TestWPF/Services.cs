using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Thingie.Tracking;
using Thingie.Tracking.Persistent.SerializedStorage;
using Thingie.Tracking.Persistent.Serialization;

namespace TestWPF
{
    //this class can be replaced by the use of an IOC container
    static class Services
    {
        public static SettingsTracker Tracker = SettingsTracker.CreateTrackerForDesktop();
    }
}
