using Jot.Storage.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jot.Storage
{
    public class JsonFileStoreFactory : IStoreFactory
    {
        public string StoreFolderPath { get; set; }

        public JsonFileStoreFactory(string storeFolderPath)
        {
            StoreFolderPath = storeFolderPath;
        }

        public IObjectStore CreateStoreForObject(string objectId)
        {
            return new JsonFileStore(Path.Combine(StoreFolderPath, string.Format("{0}.json", objectId)));
        }
    }
}
