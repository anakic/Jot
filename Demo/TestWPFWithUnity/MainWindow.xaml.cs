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
using TestWPFWithUnity.Settings;
using Ursus;
using Ursus.Configuration;

namespace TestWPFWithUnity
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ITrackingAware
    {
        public MainWindow(AppSettings settings)
        {
            //nothing is needed here to set up tracking of the window
            //the WPF tracking extension takes care of tracking window properties (height, width, left, top, windowstate)

            InitializeComponent();
            this.DataContext = settings;
        }

        public void InitConfiguration(TrackingConfiguration configuration)
        {
            configuration.SettingsTracker.Configure(tabControl).AddProperties<TabControl>(tc=>tc.SelectedIndex).Apply();
            configuration.SettingsTracker.Configure(col).AddProperties<ColumnDefinition>(tc => tc.Width).Apply();
        }
    }
}
