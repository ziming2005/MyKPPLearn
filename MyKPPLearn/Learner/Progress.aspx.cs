using System;

namespace MyKPPLearn.Learner
{
    public partial class Progress : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ((LearnerMaster)Master).PageHeading = "Progress";
        }
    }
}
