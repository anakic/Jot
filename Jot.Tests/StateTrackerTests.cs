using Jot.Storage;
using Jot.Tests.TestDataClasses;
using Jot.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;

namespace Jot.Tests
{
    [TestClass]
    public class StateTrackerTests
    {
        string tempFolder = Path.Combine(Path.GetTempPath(), "JotTestingData");
        TestPersistTrigger _trigger;

        public StateTrackerTests()
        {
            _trigger =  new TestPersistTrigger();
        }

        private StateTracker CreateStateTracker()
        {
            return new StateTracker(new JsonFileStoreFactory(tempFolder), _trigger);
        }

        [TestInitialize, TestCleanup]
        public void Init()
        {
            if(Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);
        }

        [TestMethod]
        public void ApplyDoesNothingIfNoSavedData()
        {
            var testData = new Foo() { Double = -99.9f };
            var stateTracker = CreateStateTracker();
            stateTracker.Configure(testData).AddProperty(nameof(testData.Double));
            stateTracker.Configure(testData).Apply();
            Assert.AreEqual(testData.Double, -99.9f);
        }

        [TestMethod]
        public void ApplyiesDefaultValueIfSpecifiedAndNoSavedData()
        {
            var testData = new Foo() { Double = 0 };
            var stateTracker = CreateStateTracker();
            stateTracker.Configure(testData).AddProperty(nameof(testData.Double), 987f);//supply default value
            stateTracker.Configure(testData).Apply();
            Assert.AreEqual(testData.Double, 987f);//verify default value was used
        }

        [TestMethod]
        public void ApplyPrimitiveProperties()
        {
            //save some data
            var testData1 = new Foo() { Double = 123.45, Int=456, Timespan = new TimeSpan(99,99,99) };
            CreateStateTracker()
                .Configure(testData1)
                .IdentifyAs("x")
                .AddProperties(nameof(testData1.Double), nameof(testData1.Int), nameof(testData1.Timespan))
                .Persist();

            //simulate application restart and read the saved data
            var testData2 = new Foo();
            CreateStateTracker()
                .Configure(testData2)
                .IdentifyAs("x")
                .AddProperties(nameof(testData2.Double), nameof(testData2.Int), nameof(testData2.Timespan))
                .Apply();

            //verify  the original data object and the restored data object are the same
            Assert.AreEqual(testData1.Double, testData2.Double);
            Assert.AreEqual(testData1.Int, testData2.Int);
            Assert.AreEqual(testData1.Timespan, testData2.Timespan);
        }

        [TestMethod]
        public void AppliesOnlySelectedProperties()
        {
            //save some data
            var testData1 = new Foo() { Double = 123.45, Int = 456, Timespan = new TimeSpan(99, 99, 99) };
            CreateStateTracker()
                .Configure(testData1)
                .IdentifyAs("x")
                .AddProperties(nameof(testData1.Double), nameof(testData1.Int))//not saving the "TimeSpan" property
                .Persist();

            //simulate application restart and read the saved data
            var testData2 = new Foo();
            CreateStateTracker()
                .Configure(testData2)
                .IdentifyAs("x")
                .AddProperties(nameof(testData2.Double), nameof(testData2.Int))
                .Apply();

            Assert.AreEqual(testData1.Double, testData2.Double);
            Assert.AreEqual(testData1.Int, testData2.Int);
            Assert.AreEqual(testData2.Timespan, new TimeSpan(0,0,0));//we did not track the TimeSpan property
        }

        [TestMethod]
        public void DoesNotUseOtherObjectsData()
        {
            //save some data
            var testData1 = new Foo() { Double = 123.45, Int = 456, Timespan = new TimeSpan(99, 99, 99) };
            CreateStateTracker()
                .Configure(testData1)
                .IdentifyAs("x")
                .AddProperties(nameof(testData1.Double), nameof(testData1.Int))//not saving the "TimeSpan" property
                .Persist();

            //simulate application restart and read the saved data
            var testData2 = new Foo();
            CreateStateTracker()
                .Configure(testData2)
                .IdentifyAs("some different name")
                .AddProperties(nameof(testData2.Double), nameof(testData2.Int))
                .Apply();

            //verify we did not get data from object "x"
            Assert.AreEqual(0, testData2.Double);
            Assert.AreEqual(0, testData2.Int);
        }

        [TestMethod]
        public void UsesConfigurationInitializer()
        {
            var stateTracker1 = CreateStateTracker();
            stateTracker1.RegisterConfigurationInitializer(new FooConfigurationInitializer());//add initializer

            var testData1 = new Foo() { Double = 123.45f, Int = 456 };
            var cfg1 = stateTracker1.Configure(testData1).IdentifyAs("x");
            Assert.AreEqual(5, cfg1.TrackedProperties.Count);
            Assert.IsTrue(cfg1.TrackedProperties.ContainsKey("A"));
            Assert.IsTrue(cfg1.TrackedProperties.ContainsKey("B"));
            Assert.IsTrue(cfg1.TrackedProperties.ContainsKey("Int"));
            Assert.IsTrue(cfg1.TrackedProperties.ContainsKey("Double"));
            Assert.IsTrue(cfg1.TrackedProperties.ContainsKey("Timespan"));
        }

        [TestMethod]
        public void WorksProperlyWithConfigurationInitializer()
        {
            //The idea: we're relying on FooConfigurationInitializer to initialize the configuration for our Foo object

            var stateTracker1 = CreateStateTracker();
            stateTracker1.RegisterConfigurationInitializer(new FooConfigurationInitializer());//add initializer for Foo objects
            var testData1 = new Foo() { Double = 123.45f, Int = 456 };
            var cfg1 = stateTracker1.Configure(testData1).IdentifyAs("x");
            _trigger.Fire();
            
            var testData2 = new Foo();
            var stateTracker2 = CreateStateTracker();
            stateTracker2.RegisterConfigurationInitializer(new FooConfigurationInitializer());//add initializer for Foo objects
            var cfg2 = stateTracker2.Configure(testData2).IdentifyAs("x");
            cfg2.Apply();

            //verify that the properties of the original data object and the restored data object are the same
            Assert.AreEqual(testData1.Double, testData2.Double);
            Assert.AreEqual(testData1.Int, testData2.Int);
            Assert.AreEqual(testData1.Timespan, testData2.Timespan);
        }

        [TestMethod]
        public void GlobalTriggerCausesPersist()
        {
            //The idea: verify that firing the PersistRequired event causes the data store to commit the data

            Mock<IStore> storeMoq = new Mock<IStore>();
            Mock<IStoreFactory> storeFactoryMoq = new Mock<IStoreFactory>();
            storeFactoryMoq.Setup(sf => sf.CreateStoreForObject(It.IsAny<string>())).Returns(storeMoq.Object);

            var stateTracker1 = new StateTracker(storeFactoryMoq.Object, _trigger);
            stateTracker1.RegisterConfigurationInitializer(new FooConfigurationInitializer());//add initializer

            var testData1 = new Foo() { Double = 123.45f, Int = 456 };
            var cfg1 = stateTracker1.Configure(testData1).IdentifyAs("x");

            //verify changes were committed once the persist trigger was fired
            storeMoq.Verify(s => s.CommitChanges(), Times.Never);
            _trigger.Fire();
            storeMoq.Verify(s => s.CommitChanges(), Times.Once);
        }

        [TestMethod]
        public void TrackingAwareObject()
        {
            //The idea: 
            //TestTrackingAware class implements ITrackingAware, we're relying on it to set 
            //up it's own configuration (add Value1 and Value2 to list of tracked properties)

            var data = new TestTrackingAware() { Value1 = "abc", Value2 = 123 };
            CreateStateTracker().Configure(data).Persist();

            var data2 = new TestTrackingAware();
            CreateStateTracker().Configure(data2).Apply();

            Assert.AreEqual("abc", data2.Value1);
            Assert.AreEqual(123, data2.Value2);
        }
    }
}
