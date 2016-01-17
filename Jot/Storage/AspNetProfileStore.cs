using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Runtime.Serialization;
using System.Web.Profile;
using Jot.Storage.Serialization;

namespace Jot.Storage
{
    /// <summary>
    /// Since there is no option to add dynamic properties to an ASP.NET profile, 
    /// we can work arround that by creating a dictionary property in which we can 
    /// store all the data we want. We define this class only because it's
    /// not possible to use Dictionary<string, byte[]> directly because we can't specify 
    /// generic classes in the web.config profile section.
    /// </summary>
    [Serializable]
    public class TrackedData : Dictionary<string, StoreData>
    {
        public TrackedData(){}
        protected TrackedData(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    }

    public class AspNetProfileStore : PersistentStoreBase
    {
        const string DefaultProfileDataPropertyName = "StateTrackingData";

        string _profileDataPropertyName;
        /// <summary>
        /// The name of the property that will be used to store the data in the Profile object. The property must be of type <see cref="TrackedData"/>
        /// </summary>
        public string ProfileDataPropertyName
        {
            get { return _profileDataPropertyName; }
            set { _profileDataPropertyName = value; }
        }

        public AspNetProfileStore()
            : this(DefaultProfileDataPropertyName, new JsonSerializer())
        { }

        public AspNetProfileStore(ISerializer serializer)
            : this(DefaultProfileDataPropertyName, serializer)
        { }

        public AspNetProfileStore(string profileDataPropertyName, ISerializer serializer)
            : base(serializer)
        {
            _profileDataPropertyName = profileDataPropertyName;
        }

        private TrackedData GetDataObject()
        {
            return (TrackedData)HttpContext.Current.Profile.GetPropertyValue(_profileDataPropertyName);
        }

        #region IDataStore Members

        public override bool ContainsKey(string identifier)
        {
            return GetDataObject().ContainsKey(identifier);
        }

        protected override StoreData GetData(string identifier)
        {
            return GetDataObject()[identifier];
        }

        protected override void SetData(StoreData data, string identifier)
        {
            GetDataObject()[identifier] = data;
            HttpContext.Current.Profile.Save();
        }

        public override void Remove(string identifier)
        {
            GetDataObject().Remove(identifier);
        }

        #endregion
    }
}
