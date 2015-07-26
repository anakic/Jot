using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Thingie.Tracking;
using System.IO.IsolatedStorage;
using Thingie.Tracking.Persistent.SerializedStorage;
using Thingie.Tracking.Persistent.Serialization;
using Thingie.Tracking.SessionEndNotification;
using Thingie.Tracking.Persistent;

namespace TestWinForms
{
    //this class can be replaced by the use of an IOC container
    static class Services
    {
        public static SettingsTracker Tracker = new SettingsTracker(new PersistentObjectStore(new IsolatedStorageStore(IsolatedStorageFile.GetUserStoreForDomain()), new ConverterSerializer()), new DesktopPersistTrigger());
    }
}
