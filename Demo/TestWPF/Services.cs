using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Jot;
using System.Windows;
using System.Windows.Forms;

namespace TestWPF
{
    //this class can be replaced by the use of an IOC container
    static class Services
    {
        public static Tracker Tracker = new Tracker();

        static Services()
        {
            Tracker
                .Configure<Window>()
                .Id(w => w.Name, SystemInformation.VirtualScreen.Size)
                .Properties(w => new { w.Top, w.Width, w.Height, w.Left, w.WindowState })
                .PersistOn(nameof(Window.Closing))
                .StopTrackingOn(nameof(Window.Closing));
        }
    }
}
