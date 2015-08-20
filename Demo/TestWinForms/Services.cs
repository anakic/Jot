using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ursus;
using System.IO.IsolatedStorage;
using Ursus.Storage;
using Ursus.Triggers;

namespace TestWinForms
{
    //this class can be replaced by the use of an IOC container
    static class Services
    {
        public static StateTracker Tracker = new StateTracker(new IsolatedStorageStore(IsolatedStorageFile.GetUserStoreForDomain()), new DesktopPersistTrigger());
    }
}
