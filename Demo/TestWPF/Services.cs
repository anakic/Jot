using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Thingie.Tracking;
using Thingie.Tracking.DefaultObjectStoreUtil.SerializedStorage;
using Thingie.Tracking.DefaultObjectStoreUtil.Serialization;

namespace TestWPF
{
    //this class can be replaced by the use of an IOC container
    static class Services
    {
        public static SettingsTracker Tracker = new SettingsTracker();
    }
}
