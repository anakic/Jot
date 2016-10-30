using System;

namespace Jot
{
    public interface IConfigurationInitializer
    {
        Type ForType { get; }
        void InitializeConfiguration(TrackingConfiguration configuration);
    }
}
