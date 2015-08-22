using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jot.Triggers
{
    public interface ITriggerPersist
    {
        event EventHandler PersistRequired;
    }
}
