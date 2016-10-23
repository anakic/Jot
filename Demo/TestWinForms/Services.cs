using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Jot;
using System.IO.IsolatedStorage;
using Jot.Storage;
using Jot.Triggers;

namespace TestWinForms
{
    //this class can be replaced by the use of an IOC container
    static class Services
    {
        public static StateTracker Tracker = new StateTracker();
    }
}
