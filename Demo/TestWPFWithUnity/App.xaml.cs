using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Unity;

namespace TestWPFWithUnity
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }


        void App_Startup(object sender, StartupEventArgs e)
        {
            BootStrapper bootStrapper = new BootStrapper();
            bootStrapper.Initialize();
            bootStrapper.Container.Resolve<MainWindow>().Show();
        }
    }
}
