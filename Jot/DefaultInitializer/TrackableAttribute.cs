using System;

namespace Jot.DefaultInitializer
{
    /// <summary>
    /// If applied to a property specifies if the property should be tracked.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class TrackableAttribute : Attribute
    {
        public string TrackerName { get; set; }
    }
}
