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
using Thingie.Tracking;
using Thingie.Tracking.Attributes;

namespace TestWPFWithUnity
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //expose selected tab index to the settingstracker
        [Trackable]
        public int SelectedTabIndex
        {
            get { return tabControl.SelectedIndex; }
            set { tabControl.SelectedIndex = value; }
        }

        public MainWindow(AppSettings settings)
        {
            //nothing is needed here to set up tracking of the window
            //the WPF tracking extension takes care of tracking window properties (height, width, left, top, windowstate)

            InitializeComponent();
            this.DataContext = settings;
        }
    }
}
