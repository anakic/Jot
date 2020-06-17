using Jot.Configuration;

namespace Jot.Tests.TestData
{
    class TrackingAwareTestClass : Foo, ITrackingAware
    {
        public void ConfigureTracking(TrackingConfiguration configuration)
        {
            configuration.AsGeneric<TrackingAwareTestClass>()
                .Id(f => "x")
                .Properties(f => new { f.Double, f.Int, f.Timespan });
        }
    }
}
