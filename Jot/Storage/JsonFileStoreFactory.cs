using Jot.Storage.Stores;
using System;
using System.IO;
using System.Reflection;

namespace Jot.Storage
{
    public class JsonFileStoreFactory : IStoreFactory
    {
        public string StoreFolderPath { get; set; }

        public JsonFileStoreFactory()
            : this(false)
        {
        }

        public JsonFileStoreFactory(bool perUser)
            : this(ConstructPath(perUser ? Environment.SpecialFolder.CommonApplicationData : Environment.SpecialFolder.ApplicationData))
        {
        }

        public JsonFileStoreFactory(string storeFolderPath)
        {
            StoreFolderPath = storeFolderPath;
        }

        public IStore CreateStoreForObject(string objectId)
        {
            return new JsonFileStore(Path.Combine(StoreFolderPath, string.Format("{0}.json", objectId)));
        }

        #region helper
        private static string ConstructPath(Environment.SpecialFolder baseFolder)
        {
            string companyPart = string.Empty;
            string appNamePart = string.Empty;

            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)//for unit tests entryAssembly == null
            {
                AssemblyCompanyAttribute companyAttribute = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyCompanyAttribute));
                if (!string.IsNullOrEmpty(companyAttribute.Company))
                    companyPart = string.Format("{0}\\", companyAttribute.Company);
                AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyTitleAttribute));
                if (!string.IsNullOrEmpty(titleAttribute.Title))
                    appNamePart = string.Format("{0}\\", titleAttribute.Title);
            }

            return Path.Combine(Environment.GetFolderPath(baseFolder), string.Format(@"{0}{1}", companyPart, appNamePart));
        }
        #endregion
    }
}
