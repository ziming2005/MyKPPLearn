using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using MyKPPLearn.Data;

namespace MyKPPLearn.Learner
{
    public partial class MockExam : System.Web.UI.Page
    {
        // As on the Practice page: one button per set, up to this many, then a "View all" link to the set list.
        private const int MaxSetButtons = 4;

        protected void Page_Load(object sender, EventArgs e)
        {
            ((LearnerMaster)Master).PageHeading = "Mock Exam";
        }

        /// <summary>
        /// The set buttons for one exam type: the sets a learner can start now (Active and complete), each opening the
        /// exam directly, and a "View all" link when there are more than <see cref="MaxSetButtons"/>.
        /// </summary>
        protected string SetButtons(string typeCode)
        {
            MockExamType type = MockExamRules.Find(typeCode);
            List<int> sets;

            try
            {
                sets = MockExamSetRepository.GetStartableSetNumbers(typeCode);
            }
            catch (Exception)
            {
                sets = new List<int>();
            }

            sets.Sort();
            StringBuilder html = new StringBuilder("<div class=\"practice-card__footer\"><div class=\"practice-card__sets\">");

            for (int i = 0; i < sets.Count && i < MaxSetButtons; i++)
            {
                string number = sets[i].ToString(CultureInfo.InvariantCulture);
                html.Append("<a class=\"btn btn-primary\" href=\"")
                    .Append(HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Learner/MockExamAttempt.aspx?type=" + typeCode + "&set=" + number)))
                    .Append("\" aria-label=\"").Append(HttpUtility.HtmlAttributeEncode("Start " + type.Name + " mock exam, set " + number))
                    .Append("\">Set ").Append(number).Append("</a>");
            }

            if (sets.Count == 0)
            {
                html.Append("<span class=\"btn btn-secondary practice-card__soon\" aria-disabled=\"true\">No sets yet</span>");
            }

            html.Append("</div>");

            if (sets.Count > MaxSetButtons)
            {
                html.Append("<a class=\"practice-card__all\" href=\"")
                    .Append(HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Learner/MockExamSets.aspx?type=" + typeCode)))
                    .Append("\">View all ").Append(sets.Count.ToString(CultureInfo.InvariantCulture)).Append(" sets</a>");
            }

            return html.Append("</div>").ToString();
        }
    }
}
