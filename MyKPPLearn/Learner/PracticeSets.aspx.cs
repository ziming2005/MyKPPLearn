using System;
using System.Collections.Generic;
using System.Web;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    /// <summary>
    /// Phase 5 UI prototype. The Practice Sets, their questions and options shown on this
    /// page all come from the frontend-only mock data in Scripts/practice-demo.js — there is
    /// no Practice database table or repository behind this page. This code-behind only
    /// validates the "category" query string against the four known Standard Practice
    /// categories (the same validate-and-redirect pattern used by the Learning module),
    /// nothing else.
    /// </summary>
    public partial class PracticeSets : System.Web.UI.Page
    {
        // Mirrors the four category ids defined in Scripts/practice-demo.js. Kept here only
        // for query-string validation; the category list/metadata itself is not duplicated.
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
