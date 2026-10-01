using System;

namespace MyKPPLearn.Admin
{
    public partial class PracticeManagement : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ((AdminMaster)Master).PageHeading = "Practice Management";
        }
    }
}
