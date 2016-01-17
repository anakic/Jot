using Jot.Storage.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jot.Storage
{
    public class IsolatedStorageStore : XmlStoreBase
    {
        IsolatedStorageFile _file;

        public IsolatedStorageStore(IsolatedStorageFile file)
            : this(file, new JsonSerializer())
        { }

        public IsolatedStorageStore(IsolatedStorageFile file, ISerializer serializer)
            : base(serializer)
        {
            _file = file;
        }

        protected override string GetXml()
        {
            using (var stream = _file.OpenFile("state", FileMode.Open))
            using (StreamReader reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        protected override void SaveXML(string contents)
        {
            using (var stream = _file.OpenFile("state", FileMode.Create))
            using (StreamWriter writer = new StreamWriter(stream))
                writer.Write(contents);
        }
    }
}
