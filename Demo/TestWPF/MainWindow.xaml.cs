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
using Jot;

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

            this.DataContext = _settings;
            this.SourceInitialized += MainWindow_SourceInitialized;

            //set up tracking and apply state to the settings object
            Services.Tracker.Configure(_settings).Apply();
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            //set up tracking and apply state for the main window
            Services.Tracker.Configure(this).Apply();

            //track tabcontrol's selected index
            Services.Tracker.Configure(tabControl, tabControl.Name)
                .AddProperties<TabControl>(tc => tc.SelectedIndex).Apply();
        }
    }
}