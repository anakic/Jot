using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jot.Storage.Stores;
using System.IO;
using System.Drawing;
using Jot.Tests.TestDataClasses;

namespace Jot.Tests
{
    [TestClass]
    public class JsonFileStoreTests
    {
        string _testFile;
        public JsonFileStoreTests()
        {
            _testFile = Path.Combine(Path.GetTempPath(), "JsonFileStoreTests.json");
        }

        [TestInitialize]
        public void Initialize()
        {
            File.Delete(_testFile);
        }

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete(_testFile);
        }

        [TestMethod]
        public void SavesOnlyWhenCommited()
        {
            string key = "key";
            object value = 123;

            //create two stores
            var store1 = CreateStore();
            var store2 = CreateStore();

            //put data only in the first one
            store1.Set(value, key);

            //verify only the first one has the value (we haven't committed yet)
            Assert.IsTrue(store1.ContainsKey(key));
            Assert.IsFalse(store2.ContainsKey(key));

            //commit the data in the first store
            store1.CommitChanges();

            //verify we can see the saved value in a new store (that uses the same file)
            var store3 = CreateStore();
            Assert.AreEqual(value, store3.Get(key));

        }

        [TestMethod]
        public void SetGetPrimitiveValues()
        {
            string key1 = "key1";
            string key2 = "key2";
            string key3 = "key3";
            string key4 = "key4";
            string key5 = "key5";
            string key6 = "key6";

            object value1 = 123;
            object value2 = true;
            object value3 = 123.4f;
            object value4 = DateTime.Now;
            object value5 = new Point(123,456);
            object value6 = "test string";

            var store1 = CreateStore();
            store1.Set(value1, key1);
            store1.Set(value2, key2);
            store1.Set(value3, key3);
            store1.Set(value4, key4);
            store1.Set(value5, key5);
            store1.Set(value6, key6);
            store1.CommitChanges();

            var store2 = CreateStore();
            Assert.AreEqual(store2.Get(key1), value1);
            Assert.AreEqual(store2.Get(key2), value2);
            Assert.AreEqual(store2.Get(key3), value3);
            Assert.AreEqual(store2.Get(key4), value4);
            Assert.AreEqual(store2.Get(key5), value5);
            Assert.AreEqual(store2.Get(key6), value6);
        }

        [TestMethod]
        public void SetGetComplexValue()
        {
            string key = "key";
            Foo value = new Foo()
            {
                A = new Bar() { Id = Guid.NewGuid(), DateTime = DateTime.Now, Str = "testStr" },
                B = null,
                Double = 999.65f,
                Int = 555,
                Timespan = DateTime.Now - DateTime.Now.AddMilliseconds(123123)
            };


            var store1 = CreateStore();
            store1.Set(value, key);
            store1.CommitChanges();

            var store2 = CreateStore();
            Foo value2 = (Foo)store2.Get(key);

            Assert.AreEqual(value.A.DateTime, value.A.DateTime);
            Assert.AreEqual(value.A.Id, value.A.Id);
            Assert.AreEqual(value.A.Str, value.A.Str);
            Assert.AreEqual(value.B, value2.B);
            Assert.AreEqual(value.Double, value2.Double);
            Assert.AreEqual(value.Int, value2.Int);
            Assert.AreEqual(value.Timespan, value2.Timespan);
        }

        private JsonFileStore CreateStore()
        {
            var store = new JsonFileStore(_testFile);
            return store;
        }
    }
}
