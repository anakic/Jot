using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ursus;
using System.IO.IsolatedStorage;
using Ursus.Persistent.SerializedStorage;
using Ursus.Persistent.Serialization;
using Ursus.SessionEndNotification;
using Ursus.Persistent;

namespace TestWinForms
{
    //this class can be replaced by the use of an IOC container
    static class Services
    {
        public static StateTracker Tracker = new StateTracker(new PersistentObjectStore(new IsolatedStorageStore(IsolatedStorageFile.GetUserStoreForDomain()), new ConverterSerializer()), new DesktopPersistTrigger());
    }
}
