using System;

namespace Jot
{
    /// <summary>
    /// Provides a mechanism for initializing the configuration for tracking objects of a certain type.
    /// </summary>
    public interface IConfigurationInitializer
    {
        /// <summary>
        /// Type the initializer applies to. 
        /// </summary>
        /// <remarks>
        /// The most specific initializer will be applied.
        /// </remarks>
        Type ForType { get; }

        /// <summary>
        /// Initializes the configuration object as needed.
        /// </summary>
        /// <param name="configuration"></param>
        void InitializeConfiguration(TrackingConfiguration configuration);
    }
}
