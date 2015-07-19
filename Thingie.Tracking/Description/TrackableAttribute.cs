using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Thingie.Tracking.Description
{
    /// <summary>
    /// If applied to a class, makes all properties trackable by default.
    /// If applied to a property specifies if the property should be tracked.
    /// <remarks>
    /// Attributes on properties override attributes on the class.
    /// </remarks>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TrackableAttribute : Attribute
    {
        public bool IsTrackable { get; set; }

        public string TrackerName { get; set; }

        public TrackableAttribute()
        {
            IsTrackable = true;
        }

        public TrackableAttribute(bool isTrackabe)
        {
            IsTrackable = isTrackabe;
        }
    }
}
