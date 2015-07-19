using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thingie.Tracking.SessionEndNotification
{
    public class DesktopSessionEndNotifier : ISessionEndNotifier
    {
        public DesktopSessionEndNotifier()
        {
            if (System.Windows.Application.Current != null)//wpf
                System.Windows.Application.Current.Exit += (s, e) => { OnSessionEnd(); };
            else //winforms
                System.Windows.Forms.Application.ApplicationExit += (s, e) => { OnSessionEnd(); };
        }

        private void OnSessionEnd()
        {
            if (SessionEnd != null)
            {
                SessionEnd(this, EventArgs.Empty);
            }
        }

        public event EventHandler SessionEnd;
    }
}
