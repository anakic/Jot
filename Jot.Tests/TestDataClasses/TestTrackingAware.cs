using Jot.DefaultInitializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jot.Tests.TestDataClasses
{
    class TestTrackingAware : ITrackingAware
    {
        public string Value1 { get; set; }
        public int Value2 { get; set; }

        public void InitConfiguration(TrackingConfiguration configuration)
        {
            configuration.AddProperties("Value1", "Value2");
        }
    }
}
