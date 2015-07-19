using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Thingie.Tracking.Description
{
    /// <summary>
    /// Implement this interface in classes that want to specify when their instances should be persisted.
    /// <remarks>
    /// Primarily useful with Manual persist mode, although not limited to Manual mode (you could use it to persist targets more often so changes would not be lost).
    /// </remarks>
    /// </summary>
    public interface INotifyPersistenceRequired
    {
        /// <summary>
        /// Raise this event to request persisting the settings of this object. 
        /// </summary>
        event EventHandler PersistenceRequired;
    }
}
