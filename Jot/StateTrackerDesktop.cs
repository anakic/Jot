using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jot.Storage;
using Jot.Triggers;
using Jot.Configuration;
using System.Reflection;
using System.IO;

namespace Jot
{
    /// <summary>
    /// State tracker for desktop applications. Uses a JsonFileStore for data storage and DesktopPersistTrigger to detect application shutdown.
    /// Provides convenience methods for setting up tracking for Forms(WinForms) and Windows(WPF).
    /// </summary>
    public class StateTrackerDesktop : StateTracker
    {
        public StateTrackerDesktop()
            : this(false)
        {
        }

        public StateTrackerDesktop(bool perUser)
            : this(ConstructPath(perUser ? Environment.SpecialFolder.ApplicationData : Environment.SpecialFolder.CommonApplicationData))
        {
        }

        public StateTrackerDesktop(string settingsFolder) : base(new JsonFileStoreFactory(settingsFolder), new DesktopPersistTrigger())
        {
        }

        /// <summary>
        /// Creates a configuration object for the form, sets Height/Width/Top/Left/WindowState as tracked properties, and sets ResizeEnd as the persist trigger.
        /// Limits the "Left" property to positive numbers (in case of a disconnected 2nd display) and skips storing size/location info for Maximized/Minimized forms.
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public TrackingConfiguration ConfigureForm(System.Windows.Forms.Form form)
        {
            var configuration = Configure(form, form.Name)
                .AddProperties(nameof(form.Height), nameof(form.Width), nameof(form.Top), nameof(form.Left), nameof(form.WindowState))
                .RegisterPersistTrigger(nameof(form.ResizeEnd));

            configuration.PersistingProperty += (sender, args) =>
            {
                //do not save height/width/top/left when the form is maximized or minimized
                args.Cancel = form.WindowState != System.Windows.Forms.FormWindowState.Normal && args.Property != nameof(form.WindowState);
            };

            configuration.ApplyingProperty += (sender, args) =>
            {
                //for multi-display setup:
                //if a form was last used on the 2nd display, but is being started after the 2nd display was disconnected
                if (args.Property == "Left")
                    args.Value = Math.Max(0, (int)args.Value);
            };

            return configuration;
        }

        /// <summary>
        /// Creates a configuration object for the window, sets Height/Width/Top/Left/WindowState as tracked properties, and sets SizeChanged as the persist trigger.
        /// Limits the "Left" property to positive numbers (in case of a disconnected 2nd display).
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public TrackingConfiguration ConfigureWindow(System.Windows.Window window)
        {
            var configuration = Configure(window, window.Name)
                .AddProperties(nameof(window.Height), nameof(window.Width), nameof(window.Top), nameof(window.Left), nameof(window.WindowState))
                .RegisterPersistTrigger(nameof(window.SizeChanged));

            configuration.ApplyingProperty += (sender, args) =>
            {
                //for multi-display setup:
                //if a form was last used on the 2nd display, but is being started after the 2nd display was disconnected
                if (args.Property == "Left")
                    args.Value = Math.Max(0, (double)args.Value);
            };

            return configuration;
        }

        #region helper functions
        private static string ConstructPath(System.Environment.SpecialFolder baseFolder)
        {
            string companyPart = string.Empty;
            string appNamePart = string.Empty;

            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)//for unit tests entryAssembly == null
            {
                AssemblyCompanyAttribute companyAttribute = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyCompanyAttribute));
                if (!string.IsNullOrEmpty(companyAttribute.Company))
                    companyPart = string.Format("{0}\\", companyAttribute.Company);
                AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyTitleAttribute));
                if (!string.IsNullOrEmpty(titleAttribute.Title))
                    appNamePart = string.Format("{0}\\", titleAttribute.Title);
            }

            string folderPath = Path.Combine(
                Environment.GetFolderPath(baseFolder),
                string.Format(@"{0}{1}Settings\", companyPart, appNamePart)
            );

            return folderPath;
        }
        #endregion
    }
}
