using System;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ((AdminMaster)Master).PageHeading = "Dashboard";

            litFullName.Text = SessionHelper.CurrentFullName(Session);

            try
            {
                litTotalLearners.Text = UserRepository.GetLearnerCount().ToString();
            }
            catch (Exception)
            {
                litTotalLearners.Text = "—";
            }
        }
    }
}
