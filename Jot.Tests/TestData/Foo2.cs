using System;

namespace Jot.Tests.TestData
{
    class Foo2 : Foo
    {
        public string DerivedFooProp1 { get; set; }
        public string DerivedFooProp2 { get; set; }

        public event EventHandler DerivedEvent;

        public void FireDerivedEvent1()
        {
            DerivedEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
