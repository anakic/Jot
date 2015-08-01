using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ursus.Configuration
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
