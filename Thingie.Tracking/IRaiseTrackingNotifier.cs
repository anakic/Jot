using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Thingie.Tracking
{
    public interface IRaiseTrackingNotifier
    {
        /// <summary>
        /// Raise this event to request persisting the settings for this object. 
        /// <remarks>
        /// Primarily useful with Manual persist mode, to specify when the object should be persisted,
        /// but it's not limited to Manual mode (you could use it to persist targets more often so changes would not be lost).
        /// </remarks>
        /// </summary>
        event EventHandler SettingsPersistRequest;
    }
}
