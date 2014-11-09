using Thingie.Tracking.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Thingie.Tracking.UnitTests
{
    [Serializable]
    class DummySubClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [Serializable]
    class DummSerializableClass
    {
        public bool BoolProperty { get; set; }
        public int IntProperty { get; set; }
        public long LongProperty { get; set; }
        public decimal DecimalProperty { get; set; }
        public double DoubleProperty { get; set; }
        public string StringProperty { get; set; }
        public Point PointProperty { get; set; }
        public DummySubClass SubClassProperty { get; set; }

        public List<DummySubClass> ListProperty { get; set; }
    }

    [TestClass]
    public class SerializerTests
    {
        [TestMethod]
        public void BinarySerializer_Test()
        {
            TestSerialization(new BinarySerializer());
        }

        [TestMethod]
        public void JSONSerializer_Test()
        {
            TestSerialization(new JsonSerializer());
        }

        void TestSerialization(ISerializer serializer)
        {
            DummSerializableClass dummyTestClass = new DummSerializableClass();
            dummyTestClass.BoolProperty = true;
            dummyTestClass.IntProperty = 5;
            dummyTestClass.LongProperty = (long)int.MaxValue * 2;
            dummyTestClass.DecimalProperty = (decimal)5.1;
            dummyTestClass.DoubleProperty = 5.3;
            dummyTestClass.StringProperty = "test";
            dummyTestClass.SubClassProperty = new DummySubClass() { Id = 15, Name = "test name" };
            dummyTestClass.ListProperty = new List<DummySubClass>();
            dummyTestClass.ListProperty.Add(new DummySubClass() { Id = 200, Name = "child 1 test name" });
            dummyTestClass.ListProperty.Add(new DummySubClass() { Id = 201, Name = "child 2 test name" });

            DummSerializableClass deserialized = (DummSerializableClass)serializer.Deserialize(serializer.Serialize(dummyTestClass));
            Assert.AreNotEqual(dummyTestClass, deserialized);
            Assert.AreEqual(dummyTestClass.BoolProperty, deserialized.BoolProperty);
            Assert.AreEqual(dummyTestClass.IntProperty, deserialized.IntProperty);
            Assert.AreEqual(dummyTestClass.LongProperty, deserialized.LongProperty);
            Assert.AreEqual(dummyTestClass.DecimalProperty, deserialized.DecimalProperty);
            Assert.AreEqual(dummyTestClass.DoubleProperty, deserialized.DoubleProperty);
            Assert.AreEqual(dummyTestClass.StringProperty, deserialized.StringProperty);
            Assert.AreEqual(dummyTestClass.SubClassProperty.Name, deserialized.SubClassProperty.Name);
            Assert.AreEqual(dummyTestClass.SubClassProperty.Id, deserialized.SubClassProperty.Id);
            Assert.AreEqual(dummyTestClass.ListProperty.Count(), deserialized.ListProperty.Count());
            Assert.AreEqual(dummyTestClass.ListProperty[0].Id, deserialized.ListProperty[0].Id);
            Assert.AreEqual(dummyTestClass.ListProperty[0].Name, deserialized.ListProperty[0].Name);
            Assert.AreEqual(dummyTestClass.ListProperty[1].Id, deserialized.ListProperty[1].Id);
            Assert.AreEqual(dummyTestClass.ListProperty[1].Name, deserialized.ListProperty[1].Name);
        }
    }
}
