using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eidetic.Configuration
{
    public class TrackedPropertyDescriptor
    {
        public Func<object, object> Getter { get; private set; }
        public Action<object, object> Setter { get; private set; }
        public bool IsDefaultSpecified { get; private set; }
        public object DefaultValue { get; private set; }

        internal TrackedPropertyDescriptor(Func<object, object> getter, Action<object, object> setter)
            : this(getter, setter, false, null)
        {
        }

        internal TrackedPropertyDescriptor(Func<object, object> getter, Action<object, object> setter, bool isDefaultSpecified, object defaultValue)
        {
            Getter = getter;
            Setter = setter;
            IsDefaultSpecified = isDefaultSpecified;
            DefaultValue = defaultValue;
        }
    }
}
