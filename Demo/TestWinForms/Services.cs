using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Thingie.Tracking.DataStoring;
using Thingie.Tracking.Serialization;
using Thingie.Tracking;

namespace TestWinForms
{
    //this class can be replaced by the use of an IOC container
    static class Services
    {
        public static SettingsTracker Tracker = new SettingsTracker(new FileDataStore(Environment.SpecialFolder.ApplicationData), new JsonSerializer());
    }
}
