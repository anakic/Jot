using System;
using System.Drawing;
using System.Windows.Forms;
using Jot.Configuration;

namespace TestWinForms
{
    public partial class ColorPickerUC : UserControl, ITrackingAware<ColorPickerUC>
    {
        public ColorPickerUC()
        {
            InitializeComponent();
        }

        public void ConfigureTracking(TrackingConfiguration<ColorPickerUC> configuration)
        {
            configuration
                .Id(_ => Name)
                .Properties(x => new { red = tbRed.Value, green = tbGreen.Value, blue = tbBlue.Value })
                .PersistOn(nameof(Form.FormClosing), this.FindForm());
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            groupBox1.Text = Name;
        }

        private void tb_ValueChanged(object sender, EventArgs e)
        {
            pnlSample.BackColor = Color.FromArgb(255, tbRed.Value, tbGreen.Value, tbBlue.Value);
        }
    }
}


