using System;

namespace Jot
{
    public class TrackedPropertyInfo
    {
        public Func<object, object> Getter { get; private set; }
        public Action<object, object> Setter { get; private set; }
        public bool IsDefaultSpecified { get; private set; }
        public object DefaultValue { get; private set; }

        internal TrackedPropertyInfo(Func<object, object> getter, Action<object, object> setter)
            : this(getter, setter, false, null)
        {
        }

        internal TrackedPropertyInfo(Func<object, object> getter, Action<object, object> setter, bool isDefaultSpecified, object defaultValue)
        {
            Getter = getter;
            Setter = setter;
            IsDefaultSpecified = isDefaultSpecified;
            DefaultValue = defaultValue;
        }
    }
}
