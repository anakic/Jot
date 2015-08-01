using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ursus.SessionEndNotification
{
    public interface ITriggerPersist
    {
        event EventHandler PersistRequired;
    }
}
