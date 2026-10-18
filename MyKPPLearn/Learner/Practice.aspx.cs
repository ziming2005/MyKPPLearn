using System;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    public partial class Practice : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireLearner(this);
            ((LearnerMaster)Master).PageHeading = "Practice";
        }
    }
}
