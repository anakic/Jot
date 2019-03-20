using Jot.Configuration;

namespace Jot.Tests.TestData
{
    class TestTrackingAware : ITrackingAware<TestTrackingAware>
    {
        public string Value1 { get; set; }
        public int Value2 { get; set; }

        public void ConfigureTracking(TrackingConfiguration<TestTrackingAware> configuration)
        {
            configuration.Properties(x => new { Value1, Value2 });
        }
    }
}
