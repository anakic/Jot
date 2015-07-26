using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Thingie.Tracking.Configuration;

namespace TestWinForms
{
    public partial class ColorPickerUC : UserControl, ITrackingAware
    {
        [Trackable]
        public byte Red 
        {
            get { return (byte)tbRed.Value; }
            set { tbRed.Value = value; }
        }
        [Trackable]
        public byte Green
        {
            get { return (byte)tbGreen.Value; }
            set { tbGreen.Value = value; }
        }
        [Trackable]
        public byte Blue
        {
            get { return (byte)tbBlue.Value; }
            set { tbBlue.Value = value; }
        }

        public ColorPickerUC()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            groupBox1.Text = this.Name;
        }

        private void tb_ValueChanged(object sender, EventArgs e)
        {
            pnlSample.BackColor = Color.FromArgb(255, tbRed.Value, tbGreen.Value, tbBlue.Value);
        }

        public void InitConfiguration(TrackingConfiguration configuration)
        {
            configuration.IdentifyAs(this.Name);
        }
    }
}


