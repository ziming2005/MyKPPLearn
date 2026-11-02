using System;
using System.Collections.Generic;
using System.Web;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    /// <summary>
    /// Learner Practice Sets for one category. The sets themselves are rendered by Scripts/practice.js from
    /// window.PracticeDatabaseData (written by Learner.Master from the database). This code-behind only validates the
    /// "category" query string against the four fixed categories, the same validate-and-redirect pattern used by the
    /// Learning module.
    /// </summary>
    public partial class PracticeSets : System.Web.UI.Page
    {
        // The four fixed QuestionCategories codes (seeded by DatabaseInitializer). Kept here only for
        // query-string validation; the category list and metadata themselves are not duplicated.
        private static readonly Dictionary<string, string> KnownCategories = new Dictionary<string, string>
        {
            { "colour-recognition", "Colour Recognition" },
            { "road-signs", "Road Signs" },
            { "rules-of-the-road", "Rules of the Road" },
            { "kejara-safety", "KEJARA & Safety" }
        };

        public string CategoryId { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireLearner(this);
            ((LearnerMaster)Master).PageHeading = "Practice";

            string requestedCategory = Request.QueryString["category"];
            string categoryTitle;

            if (string.IsNullOrEmpty(requestedCategory) || !KnownCategories.TryGetValue(requestedCategory, out categoryTitle))
            {
                Response.Redirect("~/Learner/Practice.aspx", true);
                return;
            }

            CategoryId = requestedCategory;
            litCategoryTitle.Text = HttpUtility.HtmlEncode(categoryTitle);
            litCategoryHeading.Text = HttpUtility.HtmlEncode(categoryTitle);
        }
    }
}
