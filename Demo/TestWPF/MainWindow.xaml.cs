using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TestWPF.Settings;
using Thingie.Tracking;

namespace TestWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AppSettings _settings = new AppSettings();

        public MainWindow()
        {
            InitializeComponent();

            //set up tracking and apply state for the main window
            Services.Tracker.Configure(this)
                .AddProperties<MainWindow>(w => w.Height, w => w.Width, w => w.Left, w => w.Top, w => w.WindowState)
                .SetKey("MainWindow")//not really needed since only one instance of MainWindow will ever exist
                .Apply();

            //set up tracking and apply state to the settings object
            Services.Tracker.Configure(_settings).Apply();
            
            //track tabcontrol's selected index
            Services.Tracker.Configure(tabControl)
                .SetKey(tabControl.Name)
                .AddProperties<TabControl>(tc => tc.SelectedIndex).Apply();

            this.DataContext = _settings;
        }
    }
}