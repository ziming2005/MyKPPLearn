using System;

namespace MyKPPLearn.Learner
{
    public partial class Practice : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ((LearnerMaster)Master).PageHeading = "Practice";
        }
    }
}
