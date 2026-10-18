using System;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    /// <summary>
    /// UI prototype. Every learner, count and date on this page is mock data from Scripts/learner-records-demo.js
    /// (searched and paged in the browser). Nothing is read from the Users table or any attempt table: this code-behind
    /// only performs the Administrator authorization check and sets the page heading.
    /// </summary>
    public partial class LearnerRecords : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireAdministrator(this);
            ((AdminMaster)Master).PageHeading = "Learner Records";
        }
    }
}
