using System;

namespace Jot
{
    /// <summary>
    /// An object that decribes the tracking information for a target object's property.
    /// </summary>
    public class TrackedPropertyInfo
    {
        /// <summary>
        /// Function that gets the value of the property.
        /// </summary>
        public Func<object, object> Getter { get; private set; }
        /// <summary>
        /// Action that sets the value of the property.
        /// </summary>
        public Action<object, object> Setter { get; private set; }
        /// <summary>
        /// Indicates if a default value is provided for the property.
        /// </summary>
        public bool IsDefaultSpecified { get; private set; }
        /// <summary>
        /// The value that will be applied to a tracked property if no existing persisted data is found.
        /// </summary>
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
