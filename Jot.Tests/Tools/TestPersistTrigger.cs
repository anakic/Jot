using Jot.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jot.Tests.Tools
{
    class TestPersistTrigger : ITriggerPersist
    {
        public event EventHandler PersistRequired;

        public void Fire()
        {
            PersistRequired?.Invoke(this, EventArgs.Empty);
        }
    }
}
