using System;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    /// <summary>Admin profile. Authorization runs first; the form itself is the shared Controls/ProfileEditor.ascx.</summary>
    public partial class Profile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireAdministrator(this);
            ((AdminMaster)Master).PageHeading = "Profile";
        }
    }
}
