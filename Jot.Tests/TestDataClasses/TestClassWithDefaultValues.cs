using Jot.DefaultInitializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jot.Tests.TestDataClasses
{
	class TestClassWithDefaultValues
	{
		[Trackable(null, 123)]
		public int IntPropWithDefault123 { get; set; }
		[Trackable(null, "ABC")]
		public string StringPropWithDefaultABC { get; set; }
	}
}
