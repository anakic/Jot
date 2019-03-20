using System;

namespace Jot.Configuration
{
    /// <summary>
    /// An object that decribes the tracking information for a target object's property.
    /// </summary>
    public class TrackedPropertyInfo
    {
        /// <summary>
        /// Function that gets the value of the property.
        /// </summary>
        public Func<object, object> Getter { get; }
        /// <summary>
        /// Action that sets the value of the property.
        /// </summary>
        public Action<object, object> Setter { get; }
        /// <summary>
        /// Indicates if a default value is provided for the property.
        /// </summary>
        public bool IsDefaultSpecified { get; }
        /// <summary>
        /// The value that will be applied to a tracked property if no existing persisted data is found.
        /// </summary>
        public object DefaultValue { get; }

        internal TrackedPropertyInfo(Func<object, object> getter, Action<object, object> setter)
            : this(getter, setter, null)
        {
            IsDefaultSpecified = false;
        }

        internal TrackedPropertyInfo(Func<object, object> getter, Action<object, object> setter, object defaultValue)
        {
            Getter = getter;
            Setter = setter;
            IsDefaultSpecified = true;
            DefaultValue = defaultValue;
        }
    }
}
