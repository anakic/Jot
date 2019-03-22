using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Jot.Storage
{
    /// <summary>
    /// An implementation of IStore that saves data to a json file.
    /// </summary>
    public class JsonFileStore : IStore
    {
        #region custom serialization (for object type handling)
        private class IPAddressConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(IPAddress);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var value = reader.Value;
                if (value != null)
                    return IPAddress.Parse((string)reader.Value);
                else
                    return null;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }
        }

        private class StoreItemConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(StoreItem);
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                reader.Read();//read "Type" attribute name
                reader.Read();//read "Type" attribute value
                Type t = serializer.Deserialize<Type>(reader);

                var x = reader.Read();//read "Name" attribute name
                var name = reader.ReadAsString();//read "Name" attribute value

                reader.Read();//read "Value" attribute name
                reader.Read();//read "value" attribute value
                var res = serializer.Deserialize(reader, t);

                reader.Read();//position to next item

                return new StoreItem() { Name = name, Type = t, Value = res };
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                //nothing fancy, standard serialization
                var converters = serializer.Converters.ToArray();
                var jObject = JObject.FromObject(value);
                jObject.WriteTo(writer, converters);
            }
        }

        private class StoreItem
        {
            [JsonProperty(Order = 1)]
            public Type Type { get; set; }
            [JsonProperty(Order = 2)]
            public string Name { get; set; }
            [JsonProperty(Order = 3)]
            public object Value { get; set; }
        }
        #endregion

        /// <summary>
        /// The folder in which the store files will be located.
        /// </summary>
        public string FolderPath { get; set; }

        /// <summary>
        /// Creates a JsonFileStore that will store files in a per-user folder (%appdata%\[companyname]\[productname]). 
        /// </summary>
        /// <remarks>
        /// CompanyName and ProductName are read from the entry assembly's attributes.
        /// </remarks>
        public JsonFileStore()
            : this(true)
        {
        }

        /// <summary>
        /// Creates a JsonFileStore that will store files in a per-user or per-machine folder. (%appdata% or %allusersprofile%  + \[companyname]\[productname]). 
        /// </summary>
        /// <param name="perUser">Specified if a per-user or per-machine folder will be used for storing the data.</param>
        /// <remarks>
        /// CompanyName and ProductName are read from the entry assembly's attributes.
        /// </remarks>
        public JsonFileStore(bool perUser)
            : this(ConstructPath(perUser ? Environment.SpecialFolder.ApplicationData : Environment.SpecialFolder.CommonApplicationData))
        {
        }

        /// <summary>
        /// Creates a JsonFileStore that will store files in the specified folder.
        /// </summary>
        /// <param name="folder">The folder inside which the json files for tracked objects will be stored.</param>
        public JsonFileStore(Environment.SpecialFolder folder)
            : this(ConstructPath(folder))
        {
        }

        /// <summary>
        /// Creates a JsonFileStore that will store files in the specified folder.
        /// </summary>
        /// <param name="storeFolderPath">The folder inside which the json files for tracked objects will be stored.</param>
        public JsonFileStore(string storeFolderPath)
        {
            FolderPath = storeFolderPath;
        }

        /// <summary>
        /// Loads values from the json file into a dictionary.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> GetData(string id)
        {
            string filePath = GetfilePath(id);
            List<StoreItem> storeItems = null;
            if (File.Exists(filePath))
            {
                try
                {
                    var fileContents = File.ReadAllText(filePath);
                    storeItems = JsonConvert.DeserializeObject<List<StoreItem>>(fileContents, new StoreItemConverter(), new IPAddressConverter());
                }
                catch { }
            }

            if (storeItems == null)
                storeItems = new List<StoreItem>();

            return storeItems.ToDictionary(item => item.Name, item => item.Value);
        }

		/// <summary>
		/// Stores the values as a json file.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="values"></param>
		public void SetData(string id, IDictionary<string, object> values)
        {
            string filePath = GetfilePath(id);
            var list = values.Select(kvp => new StoreItem() { Name = kvp.Key, Value = kvp.Value, Type = kvp.Value?.GetType() });
            string serialized = JsonConvert.SerializeObject(list, new JsonSerializerSettings() { Formatting = Formatting.Indented, TypeNameHandling=TypeNameHandling.None, Converters = new JsonConverter[] { new IPAddressConverter() } });

            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(filePath, serialized);
        }

        private string GetfilePath(string id)
        {
            return Path.Combine(FolderPath, $"{id}.json");
        }

        private static string ConstructPath(Environment.SpecialFolder baseFolder)
        {
            string companyPart = string.Empty;
            string appNamePart = string.Empty;

            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)//for unit tests entryAssembly == null
            {
                AssemblyCompanyAttribute companyAttribute = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyCompanyAttribute));
                if (!string.IsNullOrEmpty(companyAttribute.Company))
                    companyPart = $"{companyAttribute.Company}\\";
                AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyTitleAttribute));
                if (!string.IsNullOrEmpty(titleAttribute.Title))
                    appNamePart = $"{titleAttribute.Title}\\";
            }

            return Path.Combine(Environment.GetFolderPath(baseFolder), $@"{companyPart}{appNamePart}");
        }
    }
}
