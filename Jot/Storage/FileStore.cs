using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
using System.IO.IsolatedStorage;

namespace Jot.Storage
{
    public class FileStore : XmlStoreBase
    {
        public string FilePath { get; set; }

        public FileStore(System.Environment.SpecialFolder baseFolder)
            : this(ConstructPath(baseFolder))
        {

        }

        public FileStore(System.Environment.SpecialFolder baseFolder, string subFolder, string fileName)
            : this(Path.Combine(Path.Combine(Environment.GetFolderPath(baseFolder), subFolder), fileName))
        {
        }

        public FileStore(string filePath)
        {
            FilePath = filePath;
            string directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        protected override string GetXml()
        {
            if (File.Exists(FilePath))
                return File.ReadAllText(FilePath);
            else
                return null;
        }

        protected override void SaveXML(string contents)
        {
            File.WriteAllText(FilePath, contents);
        }

        #region helper
        private static string ConstructPath(System.Environment.SpecialFolder baseFolder)
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

            string backingFilePath = Path.Combine(
                Environment.GetFolderPath(baseFolder),
                string.Format(@"{0}{1}tracked_settings.xml", companyPart, appNamePart)
            );

            return backingFilePath;
        }
        #endregion
    }
}
