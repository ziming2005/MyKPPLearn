using System;
using MyKPPLearn.Security;

namespace MyKPPLearn
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.SignOut(Session);
            Response.Redirect("~/Login.aspx", true);
        }
    }
}
