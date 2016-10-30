using Jot.Tests.TestDataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jot.Tests.Tools
{
    class FooConfigurationInitializer : IConfigurationInitializer
    {
        public Type ForType
        {
            get { return typeof(Foo); }
        }

        public void InitializeConfiguration(TrackingConfiguration configuration)
        {
            configuration.AddProperties<Foo>(t => t.A, t => t.B, t => t.Double, t => t.Int, t => t.Timespan);
        }
    }
}
