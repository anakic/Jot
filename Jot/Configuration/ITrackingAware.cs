using System;
using System.Collections.Generic;
using System.Text;

namespace Jot.Configuration
{
    public interface ITrackingAware
    {
        /// <summary>
        /// Allows an object to configure its tracking.
        /// </summary>
        /// <param name="configuration"></param>
        void ConfigureTracking(TrackingConfiguration configuration);
    }
}
