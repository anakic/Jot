using Jot.Storage.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Jot.Storage
{
    public abstract class XmlStoreBase : PersistentStoreBase
    {
        const string ROOT_TAG = "Data";
        const string ITEM_TAG = "Item";
        const string ID_ATTRIBUTE = "Id";
        const string TYPE_ATTRIBUTE = "Type";

        protected abstract string GetXml();
        protected abstract void SaveXML(string contents);

        XDocument _document;
        private XDocument Document
        {
            get
            {
                if (_document == null)
                {
                    try
                    {
                        _document = XDocument.Parse(GetXml());
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

        public XmlStoreBase(ISerializer serializer)
            : base(serializer)
        {
        }

        protected override StoreData GetData(string identifier)
        {
            XElement itemElement = GetItem(identifier);
            if (itemElement == null)
                return null;
            else
                return new StoreData((string)itemElement.Value, Type.GetType(itemElement.Attribute(TYPE_ATTRIBUTE).Value));
        }

        protected override void SetData(StoreData data, string identifier)
        {
            XElement itemElement = GetItem(identifier);
            if (itemElement == null)
            {
                itemElement = new XElement(ITEM_TAG, new XAttribute(ID_ATTRIBUTE, identifier));
                Document.Root.Add(itemElement);
            }

            itemElement.Value = data.Serialized;
            itemElement.SetAttributeValue(TYPE_ATTRIBUTE, data.OriginalType.AssemblyQualifiedName);

            SaveXML(Document.ToString());
        }

        public override void Remove(string identifier)
        {
            XElement itemElement = GetItem(identifier);
            if (itemElement != null)
                itemElement.Remove();
        }

        private XElement GetItem(string identifier)
        {
            return Document.Root.Elements(ITEM_TAG).SingleOrDefault(el => (string)el.Attribute(ID_ATTRIBUTE) == identifier);
        }

        public override bool ContainsKey(string identifier)
        {
            return GetItem(identifier) != null;
        }

        public override string ToString()
        {
            return Document.ToString();
        }
    }
}
