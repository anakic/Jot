using Jot.Configuration;
using Jot.Storage;
using Jot.Tests.TestData;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Jot.Tests
{
    public class TrackerShould
    {
        Tracker _tracker;
        TestStore _store = new TestStore();

        public TrackerShould()
        {
            _tracker = new Tracker(_store);
        }

        [Fact]
        public void DoNothing_IfNoSavedData()
        {
            var testData = new Foo() { Double = -99.9f };
            _tracker.Configure<Foo>().Properties(f => new { f.Double });
            _tracker.Track(testData);
            Assert.Equal(testData.Double, -99.9f);
        }

        [Fact]
        public void ApplyDefaultValues_IfProvidedAndNoSavedData()
        {
            _tracker.Configure<Foo>()
                .Id(x => "some new id")
                .Property(f => f.Double, 123, "myprop")
                .Property(f => f.Timespan, new TimeSpan(11, 22, 33), "2");

            var testData = new Foo();
            _tracker.Track(testData);

            Assert.Equal(123, testData.Double);
            Assert.Equal(new TimeSpan(11, 22, 33), testData.Timespan);
        }

        [Fact]
        public void TrackPrimitiveProperties()
        {
            // save some data
            var testData1 = new Foo() { Double = 123.45, Int = 456, Timespan = new TimeSpan(99, 99, 99) };
            _tracker
                .Configure<Foo>()
                .Id(f => "x")
                .Properties(f => new { f.Double, f.Int, f.Timespan });
            _tracker.Track(testData1);
            _tracker.Persist(testData1);

            // simulate application restart and read the saved data
            _tracker = new Tracker(_store);

            var testData2 = new Foo();
            _tracker.Configure<Foo>()
                .Id(f => "x")
                .Properties(f => new { f.Double, f.Int, f.Timespan });
            _tracker.Track(testData2);

            // verify  the original data object and the restored data object are the same
            Assert.Equal(testData1.Double, testData2.Double);
            Assert.Equal(testData1.Int, testData2.Int);
            Assert.Equal(testData1.Timespan, testData2.Timespan);
        }

        [Fact]
        public void TrackOnlySelectedProperties()
        {
            //save some data
            var testData1 = new Foo() { Double = 123.45, Int = 456, Timespan = new TimeSpan(99, 99, 99) };
            _tracker
                .Configure<Foo>()
                .Id(f => "x")
                .Properties(f => new { f.Double, f.Int });
            _tracker.Track(testData1);

            // simulate application restart and read the saved data
            _tracker.PersistAll();
            _tracker = new Tracker(_store);

            var testData2 = new Foo();
            _tracker.Configure<Foo>()
                .Id(f => "x")
                .Properties(f => new { f.Double, f.Int });
            _tracker.Track(testData2);

            Assert.Equal(testData1.Double, testData2.Double);
            Assert.Equal(testData1.Int, testData2.Int);
            Assert.Equal(testData2.Timespan, new TimeSpan(0, 0, 0));//we did not track the TimeSpan property
        }

        [Fact]
        public void NotUseOtherObjectsData()
        {
            //save some data
            var testData1 = new Foo() { Double = 123.45, Int = 456, Timespan = new TimeSpan(99, 99, 99) };
            _tracker
                .Configure<Foo>()
                .Id(f => "x")
                .Properties(f => new { f.Double, f.Int, f.Timespan });
            _tracker.Track(testData1);

            // simulate application restart and read the saved data
            _tracker = new Tracker(_store);

            var testData2 = new Foo();
            _tracker.Configure<Foo>()
                .Id(f => "not the same id")
                .Properties(f => new { f.Double, f.Int, f.Timespan });
            _tracker.Track(testData2);

            //verify we did not get data from object "x"
            Assert.Equal(0, testData2.Double);
            Assert.Equal(0, testData2.Int);
        }

        [Fact]
        public void PersistTrackedObjects_WhenPersistAllCalled()
        {
            //The idea: verify that firing the PersistRequired event causes the data store to commit the data

            Mock<IStore> storeMoq = new Mock<IStore>();

            var tracker = new Tracker(storeMoq.Object);
            tracker.Configure<Foo>().Properties(f => new { f.Double, f.Int }).Id(f => "abc");//add initializer

            var testData1 = new Foo() { Double = 123.45f, Int = 456 };

            //verify changes were committed once the persist trigger was fired
            storeMoq.Verify(s => s.SetData(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
            tracker.PersistAll();
            storeMoq.Verify(s => s.SetData(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
        }

        [Fact]
        public void HonorITrackingAware()
        {
            var data = new TestTrackingAware() { Value1 = "abc", Value2 = 123 };
            _tracker.Track(data);
            _tracker.Persist(data);

            var data2 = new TestTrackingAware();
            _tracker.Track(data2);

            Assert.Equal("abc", data2.Value1);
            Assert.Equal(123, data2.Value2);
        }

        [Fact]
        public void ReturnSameConfig_IfSameTarget()
        {
            var cfg1 = _tracker.Configure<Foo>();
            var cfg2 = _tracker.Configure<Foo>();

            Assert.Same(cfg1, cfg2);
        }

        [Fact]
        public void Persist_WhenCalled()
        {
            var cfg1 = _tracker.Configure<Foo>()
                .Id(f => f.Int.ToString())
                .Properties(f => new { f.B, x = f.Timespan })
                .PersistOn(nameof(Foo.Event1));

            var cfg2 = _tracker.Configure<Foo2>()
                .PersistOn(nameof(Foo2.DerivedEvent))
                .Properties(f2 => new { f2.DerivedFooProp1 });

            var foo2 = new Foo2()
            {
                Double = 123,
                Int = 321,
                Timespan = new TimeSpan(1, 2, 3),
                DerivedFooProp1 = "str1",
                DerivedFooProp2 = "str2",
                B = new Bar() { Id = Guid.NewGuid(), Str = "BarStr" }
            };

            _tracker.Track(foo2);
            _tracker.Persist(foo2);

            Assert.NotSame(cfg1, cfg2);

            var data = _store.GetData(foo2.Int.ToString());
            Assert.Equal(3, data.Count);
            Assert.Equal(foo2.DerivedFooProp1, data["DerivedFooProp1"]);
            Assert.Equal(foo2.Timespan, data["x"]);
            Assert.Equal(foo2.B.Id, (data["B"] as Bar).Id);
            Assert.Equal(foo2.B.Str, (data["B"] as Bar).Str);
        }


        [Fact]
        public void HonorsIdNamespace()
        {
            var cfg1 = _tracker.Configure<Foo>()
                .Id(f => f.Int.ToString(), "context1")
                .Properties(f => new { f.B, x = f.Timespan })
                .PersistOn(nameof(Foo.Event1));

            var cfg2 = _tracker.Configure<Foo2>()
                .PersistOn(nameof(Foo2.DerivedEvent))
                .Property(f2 => f2.DerivedFooProp1);

            var foo2 = new Foo2()
            {
                Double = 123,
                Int = 321,
                Timespan = new TimeSpan(1, 2, 3),
                DerivedFooProp1 = "str1",
                DerivedFooProp2 = "str2",
                B = new Bar() { Id = Guid.NewGuid(), Str = "BarStr" }
            };

            _tracker.Track(foo2);
            _tracker.Persist(foo2);

            Assert.NotSame(cfg1, cfg2);

            var data = _store.GetData("context1.321");
            Assert.Equal(3, data.Count);
            Assert.Equal(foo2.DerivedFooProp1, data["DerivedFooProp1"]);
            Assert.Equal(foo2.Timespan, data["x"]);
            Assert.Equal(foo2.B.Id, (data["B"] as Bar).Id);
            Assert.Equal(foo2.B.Str, (data["B"] as Bar).Str);
        }

        [Fact]
        public void PersistNestedProperties()
        {
            var cfg1 = _tracker.Configure<Foo>()
                .Id(f => f.Int.ToString())
                .Properties(f => new { f.B.Str, x = f.Timespan })
                .PersistOn(nameof(Foo.Event1));

            var foo2 = new Foo2()
            {
                Double = 123,
                Int = 321,
                Timespan = new TimeSpan(1, 2, 3),
                DerivedFooProp1 = "str1",
                DerivedFooProp2 = "str2",
                B = new Bar() { Id = Guid.NewGuid(), Str = "BarStr" }
            };

            _tracker.Track(foo2);
            _tracker.Persist(foo2);

            var data = _store.GetData(foo2.Int.ToString());
            Assert.Equal(2, data.Count);
            Assert.Equal(foo2.Timespan, data["x"]);
            Assert.Equal(foo2.B.Str, data["Str"]);
        }


        [Fact]
        public void PersistNestedPropertiesWithDynamicNames()
        {
            var cfg1 = _tracker.Configure<Foo>()
                .Id(f => f.Int.ToString())
                .Properties(f => new { x = f.Timespan })
                .PersistOn(nameof(Foo.Event1));

            cfg1.Property(f => f.B.Str, "0");

            var foo2 = new Foo2()
            {
                Double = 123,
                Int = 321,
                Timespan = new TimeSpan(1, 2, 3),
                DerivedFooProp1 = "str1",
                DerivedFooProp2 = "str2",
                B = new Bar() { Id = Guid.NewGuid(), Str = "BarStr" }
            };

            _tracker.Track(foo2);
            _tracker.Persist(foo2);

            var data = _store.GetData(foo2.Int.ToString());
            Assert.Equal(2, data.Count);
            Assert.Equal(foo2.Timespan, data["x"]);
            Assert.Equal(foo2.B.Str, data["0"]);
        }

        [Fact]
        public void Persist_UsingBaseConfiguration()
        {
            // 1. arrange
            // prepare cfg for Foo
            _tracker.Configure<Foo>()
                .Id(f => f.Int.ToString())
                .Properties(f => new { f.B })
                .PersistOn(nameof(Foo.Event1));
            // create Foo2 instance (derived from Foo)
            var foo2 = new Foo2()
            {
                Double = 123,
                Int = 321,
                Timespan = new TimeSpan(1, 2, 3),
                DerivedFooProp1 = "str1",
                DerivedFooProp2 = "str2",
                B = new Bar() { Id = Guid.NewGuid(), Str = "BarStr" }
            };

            // 2. act (start tracking and call persist)
            _tracker.Track(foo2);
            _tracker.Persist(foo2);

            // 3. assert (properties set in base cfg are saved)
            var data = _store.GetData(foo2.Int.ToString());
            Assert.Equal(1, data.Count);
            Assert.Equal(foo2.B.Id, (data["B"] as Bar).Id);
            Assert.Equal(foo2.B.Str, (data["B"] as Bar).Str);
        }

        [Fact]
        public void Persist_WhenDerivedEventFired()
        {
            var cfg1 = _tracker.Configure<Foo>()
                .Id(f => f.Int.ToString())
                .Properties(f => new { f.B, x = f.Timespan })
                .PersistOn(nameof(Foo.Event1));

            var cfg2 = _tracker.Configure<Foo2>()
                .PersistOn(nameof(Foo2.DerivedEvent));

            var foo2 = new Foo2()
            {
                Double = 123,
                Int = 321,
                Timespan = new TimeSpan(1, 2, 3),
                DerivedFooProp1 = "str1",
                DerivedFooProp2 = "str2",
                B = new Bar() { Id = Guid.NewGuid(), Str = "BarStr" }
            };

            _tracker.Track(foo2);
            foo2.FireDerivedEvent1();

            Assert.NotSame(cfg1, cfg2);

            var data = _store.GetData(foo2.Int.ToString());
            Assert.Equal(2, data.Count);
            Assert.Equal(foo2.Timespan, data["x"]);
            Assert.Equal(foo2.B.Id, (data["B"] as Bar).Id);
            Assert.Equal(foo2.B.Str, (data["B"] as Bar).Str);
        }

        [Fact]
        public void Persist_BaseAndOwnedProperties()
        {
            var cfg1 = _tracker.Configure<Foo>()
                .Id(f => f.Int.ToString())
                .Properties(f => new { f.B, x = f.Timespan })
                .PersistOn(nameof(Foo.Event1));

            var cfg2 = _tracker.Configure<Foo2>()
                .PersistOn(nameof(Foo2.DerivedEvent))
                .Property(f2 => f2.DerivedFooProp1);

            var foo2 = new Foo2()
            {
                Double = 123,
                Int = 321,
                Timespan = new TimeSpan(1, 2, 3),
                DerivedFooProp1 = "str1",
                DerivedFooProp2 = "str2",
                B = new Bar() { Id = Guid.NewGuid(), Str = "BarStr" }
            };

            _tracker.Track(foo2);
            _tracker.Persist(foo2);

            Assert.NotSame(cfg1, cfg2);

            var data = _store.GetData(foo2.Int.ToString());
            Assert.Equal(3, data.Count);
            Assert.Equal(foo2.DerivedFooProp1, data["DerivedFooProp1"]);
            Assert.Equal(foo2.Timespan, data["x"]);
            Assert.Equal(foo2.B.Id, (data["B"] as Bar).Id);
            Assert.Equal(foo2.B.Str, (data["B"] as Bar).Str);
        }


        [Fact]
        public void StopTracking_WhenRequested()
        {
            var cfg1 = _tracker.Configure<Foo>()
                .Id(f => f.Int.ToString())
                .Properties(f => new { f.Double })
                .PersistOn(nameof(Foo.Event1));

            var foo = new Foo()
            {
                Int = 321,
                Double = 444
            };

            _tracker.Track(foo);
            _tracker.Persist(foo);

            // stop tracking
            _tracker.StopTracking(foo);

            foo.Double = 555;

            // normally would cause tracking
            foo.FireEvent1();

            var data = _store.GetData(foo.Int.ToString());
            Assert.Equal(444.0, data["Double"]);
        }

        [Fact]
        public void StopTracking_OnEvent()
        {
            var cfg1 = _tracker.Configure<Foo>()
                .Id(f => f.Int.ToString())
                .Properties(f => new { f.Double })
                .PersistOn(nameof(Foo.Event1))
                .StopTrackingOn(nameof(Foo.Event2));

            var foo = new Foo()
            {
                Int = 321,
                Double = 444
            };

            _tracker.Track(foo);
            _tracker.Persist(foo);

            // stop tracking
            foo.FireEvent2();

            foo.Double = 555;

            // normally would cause tracking
            foo.FireEvent1();

            var data = _store.GetData(foo.Int.ToString());
            Assert.Equal(444.0, data["Double"]);
        }

        [Fact]
        public void StopTracking_OnEventOtherObject()
        {
            var fooOther = new Foo();

            var cfg1 = _tracker.Configure<Foo>()
                .Id(f => f.Int.ToString())
                .Properties(f => new { f.Double })
                .PersistOn(nameof(Foo.Event1))
                .StopTrackingOn(nameof(Foo.Event2), fooOther);

            var foo = new Foo()
            {
                Int = 321,
                Double = 444
            };

            _tracker.Track(foo);
            _tracker.Persist(foo);

            // stop tracking
            fooOther.FireEvent2();

            foo.Double = 555;

            // normally would cause tracking
            foo.FireEvent1();

            var data = _store.GetData(foo.Int.ToString());
            Assert.Equal(444.0, data["Double"]);
        }

        [Fact]
        public void CallApplyingHandler()
        {
            List<Tuple<Foo, PropertyOperationData>> callsLog = new List<Tuple<Foo, PropertyOperationData>>();
            _tracker.Configure<Foo>()
                .Id(f => f.Int.ToString())
                .Properties(f => new { f.Double, f.Timespan })
                .WhenApplyingProperty((f, pd) => callsLog.Add(new Tuple<Foo, PropertyOperationData>(f, pd)));

            _store.SetData("321", new Dictionary<string, object> { ["Double"] = 444, ["Timespan"] = new TimeSpan(1, 2, 3) });

            var foo = new Foo()
            {
                Int = 321,
            };

            _tracker.Track(foo);

            Assert.Equal(2, callsLog.Count);
            Assert.Equal(foo, callsLog[0].Item1);
            Assert.Equal("Double", callsLog[0].Item2.Property);
            Assert.Equal(444, callsLog[0].Item2.Value);
            Assert.Equal(foo, callsLog[1].Item1);
            Assert.Equal("Timespan", callsLog[1].Item2.Property);
            Assert.Equal(new TimeSpan(1, 2, 3), callsLog[1].Item2.Value);
        }

        [Fact]
        public void CancelApplyingWhenRequested()
        {
            _tracker.Configure<Foo>()
                .Id(f => f.Int.ToString())
                .Properties(f => new { f.Double, f.Timespan })
                .WhenApplyingProperty((f, pd) => pd.Cancel = pd.Property == "Double");

            _store.SetData("321", new Dictionary<string, object> { ["Double"] = 444, ["Timespan"] = new TimeSpan(1, 2, 3) });

            var foo = new Foo()
            {
                Int = 321,
            };

            _tracker.Track(foo);

            Assert.Equal(default(double), foo.Double);
            Assert.Equal(new TimeSpan(1, 2, 3), foo.Timespan);
        }

        [Fact]
        public void NotifyWhenApplied()
        {
            List<Foo> callsLog = new List<Foo>();
            _tracker.Configure<Foo>()
                .Id(f => f.Int.ToString())
                .Properties(f => new { f.Double, f.Timespan })
                .WhenAppliedState(f => callsLog.Add(f));

            _store.SetData("321", new Dictionary<string, object> { ["Double"] = 444, ["Timespan"] = new TimeSpan(1, 2, 3) });

            var foo = new Foo()
            {
                Int = 321,
            };

            Assert.Empty(callsLog);
            _tracker.Track(foo);
            Assert.Equal(foo, callsLog.Single());
        }


        [Fact]
        public void CallPersistingHandler()
        {
            List<Tuple<Foo, PropertyOperationData>> callsLog = new List<Tuple<Foo, PropertyOperationData>>();
            _tracker.Configure<Foo>()
                .Id(f => f.Int.ToString())
                .Properties(f => new { f.Double, f.Timespan })
                .WhenPersistingProperty((f, pd) => callsLog.Add(new Tuple<Foo, PropertyOperationData>(f, pd)));

            var foo = new Foo()
            {
                Int = 321,
                Double = 444,
                Timespan = new TimeSpan(1, 2, 3)
            };

            _tracker.Track(foo);
            _tracker.Persist(foo);

            Assert.Equal(2, callsLog.Count);
            Assert.Equal(foo, callsLog[0].Item1);
            Assert.Equal("Double", callsLog[0].Item2.Property);
            Assert.Equal(444.0, callsLog[0].Item2.Value);
            Assert.Equal(foo, callsLog[1].Item1);
            Assert.Equal("Timespan", callsLog[1].Item2.Property);
            Assert.Equal(new TimeSpan(1, 2, 3), callsLog[1].Item2.Value);
        }
        [Fact]
        public void CancelPersistingWhenRequested()
        {
            // arrange (set up cancel for property == Double)
            _tracker.Configure<Foo>()
                .Id(f => f.Int.ToString())
                .Properties(f => new { f.Double, f.Timespan })
                .WhenPersistingProperty((f, pd) => pd.Cancel = pd.Property == "Double");

            // act (persist)
            var foo = new Foo()
            {
                Int = 321,
                Double = 444,
                Timespan = new TimeSpan(1, 2, 3)
            };
            _tracker.Track(foo);
            _tracker.Persist(foo);

            // assert (property "Double" is not persisted, but "Timespan" is)
            var data = _store.GetData("321");
            Assert.Equal(1, data.Count);
            Assert.Equal(new TimeSpan(1, 2, 3), data["Timespan"]);
        }
        [Fact]
        public void NotifyWhenPersisted()
        {
            List<Foo> callsLog = new List<Foo>();
            _tracker.Configure<Foo>()
                .Id(f => f.Int.ToString())
                .Properties(f => new { f.Double, f.Timespan })
                .WhenPersisted(f => callsLog.Add(f));

            _store.SetData("321", new Dictionary<string, object> { ["Double"] = 444, ["Timespan"] = new TimeSpan(1, 2, 3) });

            var foo = new Foo()
            {
                Int = 321,
            };
            _tracker.Track(foo);

            Assert.Empty(callsLog);
            _tracker.Persist(foo);
            Assert.Equal(foo, callsLog.Single());
        }

        // + properties are merged
        // + persists when requested
        // + stops tracking when reqested
        // + stops tracking when reqested by other object
        // notifies when applied
        // notifies when persisted
        // cancels applying when needed
        // cancels persisting when needed
    }
}
