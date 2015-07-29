using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eidetic.Persistent.SerializedStorage
{
    public class IsolatedStorageStore : XmlStoreBase
    {
        IsolatedStorageFile _file;
        public IsolatedStorageStore(IsolatedStorageFile file)
        {
            _file = file;
        }

        protected override string Read()
        {
            using (var stream = _file.OpenFile("state", FileMode.Open))
            using (StreamReader reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        protected override void Save(string contents)
        {
            using (var stream = _file.OpenFile("state", FileMode.Create))
            using (StreamWriter writer = new StreamWriter(stream))
                writer.Write(contents);
        }
    }
}
