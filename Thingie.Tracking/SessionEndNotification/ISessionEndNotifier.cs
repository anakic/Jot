using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thingie.Tracking.SessionEndNotification
{
    public interface ISessionEndNotifier
    {
        event EventHandler SessionEnd;
    }
}
