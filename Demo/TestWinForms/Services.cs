using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Thingie.Tracking;
using System.IO.IsolatedStorage;
using Thingie.Tracking.DefaultObjectStoreUtil.SerializedStorage;
using Thingie.Tracking.DefaultObjectStoreUtil.Serialization;
using Thingie.Tracking.SessionEndNotification;
using Thingie.Tracking.DefaultObjectStoreUtil;

namespace TestWinForms
{
    //this class can be replaced by the use of an IOC container
    static class Services
    {
        public static SettingsTracker Tracker = new SettingsTracker(new IsolatedStorageStore(IsolatedStorageFile.GetUserStoreForDomain()), new ConverterSerializer(), new DesktopSessionEndNotifier());
    }
}
