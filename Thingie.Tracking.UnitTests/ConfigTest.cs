using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thingie.Tracking.Attributes;
using Thingie.Tracking;

namespace Thingie.Tracking.UnitTests
{
    #region dummy classes for testing attribute-based tracking configuration
    [Trackable]
    class DummyClass1
    {
        [TrackingKey]
        public int Key { get; set; }
        public int Property1 { get; set; }
        public string Property2 { get; set; }
    }

    class DummyClass2
    {
        [Trackable]
        public int Property1 { get; set; }
        public string Property2 { get; set; }
    }

    [Trackable]
    class DummyClass3
    {
        public int Property1 { get; set; }
        [Trackable(false)]
        public string Property2 { get; set; }
    }

    [Trackable(TrackerName="1")]
    class DummyClass4
    {
        public int Property1 { get; set; }
        [Trackable(true, TrackerName="2")]
        public string Property2 { get; set; }
    }

    #endregion

    [TestClass]
    public class ConfigTest
    {
        [TestMethod]
        public void Config_TrackableAttributeHonoredOnClass()
        {
            TrackingConfiguration config = new TrackingConfiguration(new DummyClass1() { Key = 5 }, null);
            Assert.AreEqual(2, config.Properties.Count);
            Assert.AreEqual("Property1", config.Properties.ElementAt(0));
            Assert.AreEqual("Property2", config.Properties.ElementAt(1));
            Assert.AreEqual("5", config.Key);
        }

        [TestMethod]
        public void Config_TrackableAttributeHonoredOnProperties()
        {
            TrackingConfiguration config = new TrackingConfiguration(new DummyClass2(), null);
            Assert.AreEqual(config.Properties.Count, 1);
            Assert.AreEqual("Property1", config.Properties.ElementAt(0));
        }

        [TestMethod]
        public void Config_TrackableAttributeExcludeHonored()
        {
            TrackingConfiguration config = new TrackingConfiguration(new DummyClass3(), null);
            Assert.AreEqual(1, config.Properties.Count);
            Assert.AreEqual("Property1", config.Properties.ElementAt(0));
        }

        [TestMethod]
        public void Config_TrackerNameHonred()
        {
            TrackingConfiguration config = new TrackingConfiguration(new DummyClass4(), new SettingsTracker() { Name = "1" });
            Assert.AreEqual(2, config.Properties.Count);
            Assert.AreEqual("Property1", config.Properties.ElementAt(0));
            Assert.AreEqual("Property2", config.Properties.ElementAt(1));

            config = new TrackingConfiguration(new DummyClass4(), new SettingsTracker() { Name = "2" });
            Assert.AreEqual(1, config.Properties.Count);
            Assert.AreEqual("Property2", config.Properties.ElementAt(0));
        }

        [TestMethod]
        public void Config_ReleaseReference()
        {
            TrackingConfiguration config = new TrackingConfiguration(new DummyClass1(), null);
            GC.Collect(2, GCCollectionMode.Forced);
            GC.WaitForFullGCComplete();
            Assert.IsFalse(config.TargetReference.IsAlive);
        }
    }
}
