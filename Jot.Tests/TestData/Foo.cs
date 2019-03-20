using System;

namespace Jot.Tests.TestData
{
    class Foo
    {
        public int Int { get; set; }
        public double Double { get; set; }
        public TimeSpan Timespan { get; set; }
        public Bar A { get; set; }
        public Bar B { get; set; }

        public event EventHandler Event1;
        public event EventHandler Event2;

        public void FireEvent1()
        {
            Event1?.Invoke(this, EventArgs.Empty);
        }
        public void FireEvent2()
        {
            Event2?.Invoke(this, EventArgs.Empty);
        }
    }
}
