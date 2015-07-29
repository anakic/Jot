using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eidetic.SessionEndNotification
{
    public interface ITriggerPersist
    {
        event EventHandler PersistRequired;
    }
}
