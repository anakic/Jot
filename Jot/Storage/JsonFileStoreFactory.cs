using Jot.Storage.Stores;
using System;
using System.IO;
using System.Reflection;

namespace Jot.Storage
{
    /// <summary>
    /// Creates JsonFileStore instances for storing target object data.
    /// </summary>
    public class JsonFileStoreFactory : IStoreFactory
    {
        /// <summary>
        /// The folder in which the store files will be located.
        /// </summary>
        public string StoreFolderPath { get; set; }

        /// <summary>
        /// Creates a JsonFileStoreFactory that will store files in a per-user folder (%appdata%\[companyname]\[productname]). 
        /// </summary>
        /// <remarks>
        /// CompanyName and ProductName are read from the entry assembly's attributes.
        /// </remarks>
        public JsonFileStoreFactory()
            : this(true)
        {
        }

        /// <summary>
        /// Creates a JsonFileStoreFactory that will store files in a per-user or per-machine folder. (%appdata% or %allusersprofile%  + \[companyname]\[productname]). 
        /// </summary>
        /// <param name="perUser">Specified if a per-user or per-machine folder will be used for storing the data.</param>
        /// <remarks>
        /// CompanyName and ProductName are read from the entry assembly's attributes.
        /// </remarks>
        public JsonFileStoreFactory(bool perUser)
            : this(ConstructPath(perUser ? Environment.SpecialFolder.ApplicationData : Environment.SpecialFolder.CommonApplicationData))
        {
        }

        /// <summary>
        /// Creates a JsonFileStoreFactory that will store files in the specified folder.
        /// </summary>
        /// <param name="folder">The folder inside which the json files for tracked objects will be stored.</param>
        public JsonFileStoreFactory(Environment.SpecialFolder folder)
            : this (Environment.GetFolderPath(folder))
        {
        }

        /// <summary>
        /// Creates a JsonFileStoreFactory that will store files in the specified folder.
        /// </summary>
        /// <param name="storeFolderPath">The folder inside which the json files for tracked objects will be stored.</param>
        public JsonFileStoreFactory(string storeFolderPath)
        {
            StoreFolderPath = storeFolderPath;
        }

        /// <summary>
        /// Creates a JsonFileStore for the object with the specified id. The Id needs to be a valid file name without the file extension!
        /// </summary>
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
