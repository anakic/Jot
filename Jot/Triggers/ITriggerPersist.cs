using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jot.Triggers
{
    /// <summary>
    /// An interface that should be implemented by objects that determine when a global persist should be performed.
    /// </summary>
    public interface ITriggerPersist
    {
        /// <summary>
        /// Fired when a global persist operation should be performed.
        /// </summary>
        event EventHandler PersistRequired;
    }
}
