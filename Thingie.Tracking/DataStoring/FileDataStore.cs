using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Reflection;

namespace Thingie.Tracking.DataStoring
{
    public class FileDataStore : XmlDataStoreBase
    {
        public string FilePath { get; set; }

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
            string directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        protected override string Read()
        {
            if (File.Exists(FilePath))
                return File.ReadAllText(FilePath);
            else
                return null;
        }

        protected override void Save(string contents)
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

            string settingsFilePath = Path.Combine(
                Environment.GetFolderPath(baseFolder),
                string.Format(@"{0}{1}tracked_settings.xml", companyPart, appNamePart)
            );

            return settingsFilePath;
        }
        #endregion
    }

    public abstract class XmlDataStoreBase : IDataStore
    {
        const string ROOT_TAG = "Data";
        const string ITEM_TAG = "Item";
        const string ID_ATTRIBUTE = "Id";

        protected abstract string Read();
        protected abstract void Save(string contents);

        XDocument _document;
        private XDocument Document
        {
            get
            {
                if (_document == null)
                {
                    try
                    {
                        _document = XDocument.Parse(Read());
                        if (_document.Element(ROOT_TAG) == null)
                            _document = null;//a corrupt store, we'll make a new one
                    }
                    catch
                    {
                        _document = null;//a corrupt store, we'll make a new one
                    }


                    if (_document == null)
                    {
                        _document = new XDocument();
                        _document.Add(new XElement(ROOT_TAG));
                    }
                }
                return _document;
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
                Document.Root.Add(itemElement);
            }

            itemElement.Value = Convert.ToBase64String(data);
            Save(Document.ToString());
        }

        public void RemoveData(string identifier)
        {
            XElement itemElement = GetItem(identifier);
            if (itemElement != null)
                itemElement.Remove();
        }

        private XElement GetItem(string identifier)
        {
            return Document.Root.Elements(ITEM_TAG).SingleOrDefault(el => (string)el.Attribute(ID_ATTRIBUTE) == identifier);
        }

        public bool ContainsKey(string identifier)
        {
            return GetItem(identifier) != null;
        }

        public override string ToString()
        {
            return Document.ToString();
        }
    }
}
