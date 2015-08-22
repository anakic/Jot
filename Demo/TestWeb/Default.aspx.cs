using System;
using Jot;
using Jot.Configuration;
using Jot.Unity.Web;

namespace TestWeb
{
    public partial class _Default : System.Web.UI.Page
    {
        [Trackable(TrackerName = AspNetTrackerNames.SESSION)]
        public int Counter { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Counter++;
        }

        protected override void OnPreRender(EventArgs e)
        {
            lblCounter.Text = Counter.ToString();
            base.OnPreRender(e);
        }
    }
}
