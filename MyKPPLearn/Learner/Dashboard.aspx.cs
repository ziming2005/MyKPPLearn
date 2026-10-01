using System;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ((LearnerMaster)Master).PageHeading = "Dashboard";

            litFullName.Text = SessionHelper.CurrentFullName(Session);

            if (Session["RegistrationSuccess"] != null)
            {
                pnlSuccess.Visible = true;
                Session.Remove("RegistrationSuccess");
            }
        }
    }
}
