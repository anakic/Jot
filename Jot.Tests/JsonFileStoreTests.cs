using Jot.Storage;
using Jot.Tests.TestData;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Xunit;

namespace Jot.Tests
{
    public class JsonFileStoreTests
    {
        JsonFileStore _store = new JsonFileStore();

        [Fact]
        public void StoresPrimitiveData()
        {
            var data = new Dictionary<string, object>()
            {
                ["Int"] = 123,
                ["String"] = "abc"
            };

            _store.SetData("id", data);

            var data2 = _store.GetData("id");
            Assert.Equal(123, data2["Int"]);
            Assert.Equal("abc", data2["String"]);
        }


        [Fact]
        public void StoresObjectGraph()
        {
            var id = Guid.NewGuid();

            var data = new Dictionary<string, object>()
            {
                ["Int"] = 123,
                ["Obj"] = new Bar() { Id = id, Str = "SomeString" }
            };

            _store.SetData("id", data);

            var data2 = _store.GetData("id");
            Assert.Equal(123, data2["Int"]);
            Assert.Equal(id, (data2["Obj"] as Bar).Id);
            Assert.Equal("SomeString", (data2["Obj"] as Bar).Str);
        }


        [Fact]
        public void StoresSpecialType_IPAddress()
        {
            var id = Guid.NewGuid();

            var data = new Dictionary<string, object>()
            {
                ["Int"] = 123,
                ["myip"] = new IPAddress(new byte[] { 1, 2, 3, 4 })
            };

            _store.SetData("id", data);

            var data2 = _store.GetData("id");
            Assert.Equal(123, data2["Int"]);
            Assert.Equal("1.2.3.4", data2["myip"].ToString());
        }
    }
}
