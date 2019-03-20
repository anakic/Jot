using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Jot.Configuration;
using Jot.Storage;

namespace TestWinForms
{
    public partial class Form1 : Form, ITrackingAware<Form1>
    {
        public Form1()
        {
            InitializeComponent();

            dataGridView1.DataSource = new List<Person>()
            {
                new Person { Name = "Joe", LastName="Smith", Age = 34 },
                new Person { Name = "Misha", LastName="Anderson", Age = 45 },
            };
            // NOTE: 
            // We cannot call Track(this) in the constructor. Winfors overwrites Top/Left 
            // properties after the constructor, so we must set them in OnLoad instead.
        }

        protected override void OnLoad(EventArgs e)
        {
            // track this form
            Services.Tracker.Track(this);

            // track color pickers as separate objects
            // ColorPicker also implements ITrackingAware so no configuration is needed here
            Services.Tracker.Track(colorPicker1);
            Services.Tracker.Track(colorPicker2);
        }

        public void ConfigureTracking(TrackingConfiguration<Form1> configuration)
        {
            // include selected tab index when tracking this form
            configuration.Property(f => f.tabControl1.SelectedIndex);

            // include data grid column widths when tracking this form
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                var idx = i; // capture i into a variable (cannot use i directly since it changes in each iteration)
                configuration.Property(f => f.dataGridView1.Columns[idx].Width, "grid_column_" + dataGridView1.Columns[idx].Name);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", (Services.Tracker.Store as JsonFileStore).FolderPath);
        }
    }
}
