using Jot.Configuration;
using System;
using System.Windows;
using System.Windows.Controls;
using TestWPF.Settings;

namespace TestWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            //set up tracking and apply state to the application settings object
            Services.Tracker.Track(App.Settings);
            
            // in addition to tracking standard window properties, also track selected tab for MainWindow instances
            Services.Tracker.Configure<MainWindow>().Property(w => w.tabControl.SelectedIndex, "SelectedTab");
        
            //set up tracking and apply state for the main window
            Services.Tracker.Track(this);

            this.DataContext = App.Settings;
        }
    }
}