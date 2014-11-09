using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Reflection;

namespace Thingie.Tracking.DataStoring
{
    public class FileDataStore : IDataStore
    {
        XDocument _document;

        const string ROOT_TAG = "Data";
        const string ITEM_TAG = "Item";
        const string ID_ATTRIBUTE = "Id";

        public string FilePath { get; private set; }

        public FileDataStore(System.Environment.SpecialFolder baseFolder)
            : this(ConstructPath(baseFolder))
        {
            
        }

        public FileDataStore(System.Environment.SpecialFolder baseFolder, string subFolder, string fileName)
            : this(Path.Combine(Path.Combine(Environment.GetFolderPath(baseFolder), subFolder), fileName))
        {
        }

        public FileDataStore(string filePath)
        {
            FilePath = filePath;

            if (File.Exists(FilePath))
            {
                _document = XDocument.Load(FilePath);
            }
            else
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                _document = new XDocument();
                _document.Add(new XElement(ROOT_TAG));
            }
        }

        public byte[] GetData(string identifier)
        {
            XElement itemElement = GetItem(identifier);
            if (itemElement == null)
                return null;
            else
                return Convert.FromBase64String((string)itemElement.Value);
        }

        public void SetData(byte[] data, string identifier)
        {
            XElement itemElement = GetItem(identifier);
            if (itemElement == null)
            {
                itemElement = new XElement(ITEM_TAG, new XAttribute(ID_ATTRIBUTE, identifier));
                _document.Root.Add(itemElement);
            }

            itemElement.Value = Convert.ToBase64String(data);
            _document.Save(FilePath);
        }

        public void RemoveData(string identifier)
        { 
            XElement itemElement = GetItem(identifier);
            if (itemElement != null)
                itemElement.Remove();
        }

        private XElement GetItem(string identifier)
        {
            return _document.Root.Elements(ITEM_TAG).SingleOrDefault(el => (string)el.Attribute(ID_ATTRIBUTE) == identifier);
        }

        public bool ContainsKey(string identifier)
        {
            return GetItem(identifier) != null;
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

            string settingsFilePath = Path.Combine(
                Environment.GetFolderPath(baseFolder),
                string.Format(@"{0}{1}tracked_settings.xml", companyPart, appNamePart)
            );

            return settingsFilePath;
        }
        #endregion
    }
}
