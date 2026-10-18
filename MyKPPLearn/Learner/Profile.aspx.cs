using System;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    /// <summary>Learner profile. Authorization runs first; the form itself is the shared Controls/ProfileEditor.ascx.</summary>
    public partial class Profile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireLearner(this);
            ((LearnerMaster)Master).PageHeading = "Profile";
        }
    }
}
