using Thingie.Tracking.DataStoring;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Thingie.Tracking;

namespace Thingie.Tracking.UnitTests
{
    [TestClass]
    public class TrackerTests
    {
        SettingsTracker _tracker = new SettingsTracker();
        SettingsTracker _namedTracker1 = new SettingsTracker() { Name = "1" };
        SettingsTracker _namedTracker2 = new SettingsTracker() { Name = "2" };

        //TODO: test handling of ITrackingAware

        [TestMethod]
        public void SettingsTracker_PersistApplyState_AutomaticMode()
        {
            //some object, created, configured and manupulated
            DummyClass1 beforeShutdown = new DummyClass1();
            _tracker.Configure(beforeShutdown).SetMode(PersistModes.Automatic);
            beforeShutdown.Property1 = 5;
            beforeShutdown.Property2 = "testValue";

            //simulate app shutdown
            _tracker.PersistAutomaticTargets();

            //simulate app restarted
            DummyClass1 afterRestart = new DummyClass1();
            new SettingsTracker().Configure(afterRestart).Apply();
            Assert.AreEqual(beforeShutdown.Property1, afterRestart.Property1);
            Assert.AreEqual(beforeShutdown.Property2, afterRestart.Property2);
        }

        [TestMethod]
        public void SettingsTracker_PersistApplyState_Named()
        {
            DummyClass4 beforeShutdown = new DummyClass4();
            _namedTracker1.Configure(beforeShutdown);
            _namedTracker2.Configure(beforeShutdown);
            beforeShutdown.Property1 = 5;
            beforeShutdown.Property2 = "testValue";

            //simulate app shutdown
            _namedTracker1.PersistAutomaticTargets();
            _namedTracker2.PersistAutomaticTargets();

            //simulate app startup
            DummyClass4 afterRestart_name1 = new DummyClass4();
            _namedTracker1.Configure(afterRestart_name1).Apply();
            Assert.AreEqual(beforeShutdown.Property1, afterRestart_name1.Property1);
            Assert.AreEqual(beforeShutdown.Property2, afterRestart_name1.Property2);

            DummyClass4 afterRestart_name2 = new DummyClass4();
            _namedTracker2.Configure(afterRestart_name2).Apply();
            Assert.AreEqual(0, afterRestart_name2.Property1);
            Assert.AreEqual(beforeShutdown.Property2, afterRestart_name2.Property2);
        }

        [TestMethod]
        public void SettingsTracker_PersistApplyState_KeyIdentification()
        {
            DummyClass1 beforeShutdown1 = new DummyClass1() { Key = 100 };
            DummyClass1 beforeShutdown2 = new DummyClass1() { Key = 200 };
            _tracker.Configure(beforeShutdown1);
            _tracker.Configure(beforeShutdown2);
            beforeShutdown1.Property1 = 5;
            beforeShutdown1.Property2 = "first test value";
            beforeShutdown2.Property1 = 500;
            beforeShutdown2.Property2 = "second test value";

            //simulate app shutdown
            _tracker.PersistAutomaticTargets();

            //simulate app restarted
            DummyClass1 afterRestart1 = new DummyClass1() { Key = 100 };
            DummyClass1 afterRestart2 = new DummyClass1() { Key = 200 };
            SettingsTracker newSettingsTracker = new SettingsTracker();
            newSettingsTracker.Configure(afterRestart1).Apply();
            newSettingsTracker.Configure(afterRestart2).Apply();

            Assert.AreEqual(beforeShutdown1.Property1, afterRestart1.Property1);
            Assert.AreEqual(beforeShutdown1.Property2, afterRestart1.Property2);
            Assert.AreEqual(beforeShutdown2.Property1, afterRestart2.Property1);
            Assert.AreEqual(beforeShutdown2.Property2, afterRestart2.Property2);
        }

        [TestCleanup]
        public void CleanUp()
        {
            File.Delete(((_tracker.ObjectStore as ObjectStore).DataStore as FileDataStore).FilePath);
        }
    }
}
