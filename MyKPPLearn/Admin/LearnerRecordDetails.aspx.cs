using System;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    /// <summary>
    /// UI prototype (read-only). The selected learner comes from the same mock data as the list page
    /// (Scripts/learner-records-demo.js); the id in the address refers only to that local mock collection, never to Users.
    /// An unknown or missing id sends the browser back to Learner Records. This code-behind only performs the
    /// Administrator authorization check and sets the page heading.
    /// </summary>
    public partial class LearnerRecordDetails : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireAdministrator(this);
            ((AdminMaster)Master).PageHeading = "Learner Record Details";
        }
    }
}
