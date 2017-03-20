using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Jot.Storage.Stores
{
    /// <summary>
    /// An implementation of IStore that saves data to a json file.
    /// </summary>
    public class JsonFileStore : PersistentStoreBase
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
        /// The file that will store the target object's data.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Creates a new instance of a JsonFileStore.
        /// </summary>
        /// <param name="filePath"></param>
        public JsonFileStore(string filePath)
        {
            FilePath = filePath;
        }

        /// <summary>
        /// Loads values from the json file into the dictionary cache.
        /// </summary>
        /// <returns></returns>
        protected override Dictionary<string, object> LoadValues()
        {
            List<StoreItem> storeItems = null;
            if (File.Exists(FilePath))
            {
                try
                {
                    storeItems = JsonConvert.DeserializeObject<List<StoreItem>>(File.ReadAllText(FilePath), new StoreItemConverter(), new IPAddressConverter());
                }
                catch { }
            }

            if (storeItems == null)
                storeItems = new List<StoreItem>();

            return storeItems.ToDictionary(item => item.Name, item => item.Value);
        }

        /// <summary>
        /// Stores the values from the dictioanry cache into the json file.
        /// </summary>
        /// <param name="values"></param>
        protected override void SaveValues(Dictionary<string, object> values)
        {
            var list = values.Select(kvp => new StoreItem() { Name = kvp.Key, Value = kvp.Value, Type = kvp.Value?.GetType() });
            string serialized = JsonConvert.SerializeObject(list, new JsonSerializerSettings() { Formatting = Formatting.Indented, TypeNameHandling=TypeNameHandling.None, Converters = new JsonConverter[] { new IPAddressConverter() } });

            string directory = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(FilePath, serialized);
        }
    }
}
