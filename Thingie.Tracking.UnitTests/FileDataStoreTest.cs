using Thingie.Tracking.DataStoring;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Thingie.Tracking.UnitTests
{
    [TestClass]
    public class FileDataStoreTest
    {
        FileDataStore _store = new FileDataStore("test.data");

        [TestMethod]
        public void FileDataStore_ReadWrite()
        {
            byte[] data = new byte[256];
            for (byte b = 0; b < byte.MaxValue; b++)
                data[b] = b;
            _store.SetData(data, "DummyDataKey");
            Assert.IsTrue(_store.ContainsKey("DummyDataKey"));
            Assert.IsFalse(_store.ContainsKey("NonExistentDummyIdentifier"));

            byte[] res = _store.GetData("DummyDataKey");
            for (byte b = 0; b < byte.MaxValue; b++)
                Assert.AreEqual(b, res[b]);
        }

        [TestCleanup]
        public void CleanUp()
        {
            File.Delete(_store.FilePath);
        }
    }
}
