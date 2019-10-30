using System;
using System.Collections.Generic;
using System.Text;

namespace Jot.Configuration.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class TrackableAttribute : Attribute
    {
    }
}
