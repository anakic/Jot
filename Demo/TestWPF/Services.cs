using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ursus;
using Ursus.Persistent.SerializedStorage;
using Ursus.Persistent.Serialization;

namespace TestWPF
{
    //this class can be replaced by the use of an IOC container
    static class Services
    {
        public static StateTracker Tracker = StateTracker.CreateTrackerForDesktop();
    }
}
