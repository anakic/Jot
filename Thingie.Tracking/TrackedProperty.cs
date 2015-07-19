using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thingie.Tracking
{
    public class TrackedProperty
    {
        public string Name { get; private set; }
        public Func<object> Getter { get; private set; }
        public Action<object, object> Setter { get; private set; }
        public Func<object> DefaultValueGetter { get; private set; }

        public TrackedProperty(string name, Func<object> getter, Action<object, object> setter, Func<object> defaultValueGetter)
        {
            Getter = () => getter();
            Setter = (t,v) => setter(t,v);
            DefaultValueGetter = defaultValueGetter;
        }
    }
}
