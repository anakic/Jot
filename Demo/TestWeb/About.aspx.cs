using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Practices.Unity;
using System.Diagnostics;
using Thingie.Tracking;
using Thingie.Tracking.Configuration;

namespace TestWeb
{
    public partial class About : System.Web.UI.Page
    {
        //[Dependency]
        //public ISerializer Serializer { get; set; }
        //[Dependency]
        //public SettingsTracker Tracker { get; set; }

        [Trackable]
        public int NekiBroj { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            //StoredData sd = (StoredData)HttpContext.Current.Profile.GetPropertyValue("TrackingData");
            //object res = null;
            //if(sd.Count>0)
            //    res = Serializer.Deserialize(sd.Values.First());
            //Debug.WriteLine(res);

            //Tracker.ApplyState(this);
            Label1.Text = (++NekiBroj).ToString();
            //Tracker.PersistState(this);

            //NekiBroj = (int)HttpContext.Current.Profile.GetPropertyValue("NekiBroj");
            //Label1.Text = (++NekiBroj).ToString();
            //HttpContext.Current.Profile.SetPropertyValue("NekiBroj", NekiBroj);
            //HttpContext.Current.Profile.Save();
        }
    }
}
