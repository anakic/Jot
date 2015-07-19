using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Thingie.Tracking.Description;

namespace TestWeb
{
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        [Trackable]
        public int Test { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            blabel.Text = (Test++).ToString();
        }
    }
}
