using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Thingie.Tracking;
using Thingie.Tracking.DataStoring;
using Thingie.Tracking.Serialization;

namespace TestWinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            //NOTE:We can't apply settings in the constructor because 
            //Top and Left won't be honored for some (WinForms related) reason.
            //We must do it at OnLoad.
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //WinForms are tricky: if WindowState != Normal, size and location properties are bogus and shouldn't be persisted. 
            //This extension method configures the tracking so that size and location aren't persisted when WindowState != Normal. 
            this.ConfigureFormTracking(Services.Tracker).Apply();
            
            //Track colorpicker1 usercontrol (based on specified attributes)
            Services.Tracker.Configure(colorPicker1).Apply();

            //Track colorpicker2 usercontrol (based on specified attributes)
            Services.Tracker.Configure(colorPicker2).Apply();
        }
    }
}
