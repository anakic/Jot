using System;

namespace Jot.Triggers
{
    /// <summary>
    /// An implementation of ITriggerPersist that fires PersistRequired when a desktop application is about to shut down. 
    /// Applicable to WinForms and WPF applications.
    /// </summary>
    public class DesktopPersistTrigger : ITriggerPersist
    {
        /// <summary>
        /// Creates a new instance of DesktopPersistTrigger.
        /// </summary>
        public DesktopPersistTrigger()
        {
            if (System.Windows.Application.Current != null)//wpf
                System.Windows.Application.Current.Exit += (s, e) => { OnApplicationClosing(); };
            else //winforms
                System.Windows.Forms.Application.ApplicationExit += (s, e) => { OnApplicationClosing(); };
        }

        private void OnApplicationClosing()
        {
            PersistRequired?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fired when a desktop application is shutting down to indicate a global persist should be performed.
        /// </summary>
        public event EventHandler PersistRequired;
    }
}
