using System;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    /// <summary>
    /// Phase 5 UI prototype. Every row, filter and action on this page is driven entirely
    /// by frontend-only mock data (Scripts/practice-demo.js + Scripts/practice-admin-demo.js).
    /// There is no Question Bank database table or repository: this code-behind only
    /// performs the page's authorization check and sets the page heading.
    /// </summary>
    public partial class QuestionBank : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireAdministrator(this);
            ((AdminMaster)Master).PageHeading = "Question Bank";
        }
    }
}
