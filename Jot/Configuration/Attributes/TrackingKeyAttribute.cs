using System;
using System.Collections.Generic;
using System.Text;

namespace Jot.Configuration.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TrackingIdAttribute : Attribute
    {
        public bool IncludeType { get; }

        public object Namespace { get; }

        public TrackingIdAttribute(object @namespace = null, bool includeType = false)
        {
            IncludeType = includeType;
            Namespace = @namespace;
        }
    }
}
