using System;

namespace Jot.DefaultInitializer
{
    /// <summary>
    /// Marks the property as the tracking identifier for the object.
    /// The property will in most cases be of type String, Guid or Int
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TrackingKeyAttribute : Attribute
    {
    }
}
