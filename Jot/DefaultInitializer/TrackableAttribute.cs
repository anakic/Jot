using System;

namespace Jot.DefaultInitializer
{
    /// <summary>
    /// If applied to a property specifies if the property should be tracked.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class TrackableAttribute : Attribute
    {
        /// <summary>
        /// The name of the StateTracker that will care about this attribute. Useful for scenarios with multiple state trackers (e.g. one per-user, another per-machine).
        /// </summary>
        public string TrackerName { get; private set; }

		public bool IsDefaultSpecified { get; private set; }

		public object DefaultValue { get; private set; }

		public TrackableAttribute()
		{
		}

		public TrackableAttribute(string trackerName)
		{
			TrackerName = TrackerName;
		}

		public TrackableAttribute(string trackerName, object defaultValue)
		{
			TrackerName = trackerName;
			IsDefaultSpecified = true;
			DefaultValue = defaultValue;
		}
	}
}
