using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using Thingie.Tracking.Description;

namespace TestWPF.Settings
{
    [Trackable]
    public class AppSettings
    {
        public DisplaySettings DisplaySettings { get; set; }
        public GeneralSettings GeneralSettings { get; set; }

        public AppSettings()
        {
            DisplaySettings = new DisplaySettings();
            GeneralSettings = new GeneralSettings();
        }
    }
}
