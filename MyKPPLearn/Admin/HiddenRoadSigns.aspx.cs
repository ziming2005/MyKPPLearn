using System;

namespace MyKPPLearn.Admin
{
    public partial class HiddenRoadSigns : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ((AdminMaster)Master).PageHeading = "Hidden Road Sign";
        }
    }
}
