using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Eidetic;
using System.IO.IsolatedStorage;
using Eidetic.Persistent.SerializedStorage;
using Eidetic.Persistent.Serialization;
using Eidetic.SessionEndNotification;
using Eidetic.Persistent;

namespace TestWinForms
{
    //this class can be replaced by the use of an IOC container
    static class Services
    {
        public static SettingsTracker Tracker = new SettingsTracker(new PersistentObjectStore(new IsolatedStorageStore(IsolatedStorageFile.GetUserStoreForDomain()), new ConverterSerializer()), new DesktopPersistTrigger());
    }
}
