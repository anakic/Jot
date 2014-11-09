using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Thingie.Tracking.Attributes;
using Thingie.Tracking;

namespace TestWPFWithUnity.Settings
{
    //Notice the Trackable attribute on the class - all properties will be tracked
    //by default. In this case, if you don't want to track a specific property, mark
    //the property with Trackable(false).
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
