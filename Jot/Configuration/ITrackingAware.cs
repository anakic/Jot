using System;
using System.Collections.Generic;
using System.Text;

namespace Jot.Configuration
{
    public interface ITrackingAware
    {
        /// <summary>
        /// Called when the object's tracking configuration is first created.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns>Return false to cancel applying state</returns>
        void ConfigureTracking(TrackingConfiguration configuration);
    }
}
