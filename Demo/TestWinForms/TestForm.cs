using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Jot;

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

            //use convenience method for setting up Form tracking
            Services.Tracker.ConfigureForm(this).Apply();
            
            //Track colorpicker1 usercontrol (based on specified attributes)
            Services.Tracker.Configure(colorPicker1).Apply();

            //Track colorpicker2 usercontrol (based on specified attributes)
            Services.Tracker.Configure(colorPicker2).Apply();
        }
    }
}
