using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    /// <summary>
    /// Answer review of the learner's OWN completed Hidden Road Sign Game attempt: found order, road sign,
    /// question, own answer and status; for incorrect answers also the correct answer and the explanation.
    /// Hotspot coordinates are never shown.
    /// </summary>
    public partial class HiddenRoadSignReview : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireLearner(this);
            ((LearnerMaster)Master).PageHeading = "Road Sign Game";

            int attemptId;

            if (!int.TryParse(Request.QueryString["attempt"], NumberStyles.None, CultureInfo.InvariantCulture, out attemptId))
            {
                Response.Redirect("~/Learner/Game.aspx", true);
                return;
            }

            int userId = SessionHelper.CurrentUserId(Session).Value;
            DataRow attempt;
            DataTable rows;

            try
            {
                // Both queries are scoped to the signed-in learner: another learner's id is never returned.
                attempt = HiddenRoadSignAttemptRepository.GetAttemptForUser(attemptId, userId);
                rows = attempt == null ? null : HiddenRoadSignAttemptRepository.GetReviewRows(attemptId, userId);
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception)
            {
                Response.Redirect("~/Learner/Game.aspx", true);
                return;
            }

            if (attempt == null)
            {
                Response.Redirect("~/Learner/Game.aspx", true);
                return;
            }

            string attemptKey = attemptId.ToString(CultureInfo.InvariantCulture);

            litSetName.Text = HttpUtility.HtmlEncode(HiddenRoadSignPracticeRepository.FormatSetName(attempt["SetNumber"])
                + " · " + Convert.ToInt32(attempt["CorrectCount"]).ToString(CultureInfo.InvariantCulture)
                + " / " + Convert.ToInt32(attempt["TotalItems"]).ToString(CultureInfo.InvariantCulture) + " correct");
            lnkBackTop.NavigateUrl = "~/Learner/HiddenRoadSignResult.aspx?attempt=" + attemptKey;
            lnkBackResult.NavigateUrl = "~/Learner/HiddenRoadSignResult.aspx?attempt=" + attemptKey;

            rptReview.DataSource = rows;
            rptReview.DataBind();
        }

        protected string AnswerText(object order, object text)
        {
            return HttpUtility.HtmlEncode(HiddenRoadSignItemRepository.ChoiceLetter(order) + ". " + Convert.ToString(text));
        }

        /// <summary>
        /// Only an incorrect answer shows the correct answer and (when configured) the explanation;
        /// a correct answer shows neither. All text is HTML-encoded.
        /// </summary>
        protected string IncorrectDetails(object isCorrect, object correctOrder, object correctText, object explanation)
        {
            if ((bool)isCorrect)
            {
                return string.Empty;
            }

            StringBuilder html = new StringBuilder();
            html.Append("<p class=\"practice-review__label\">Correct Answer:</p>");
            html.Append("<p class=\"practice-review__answer practice-review__answer--correct\">")
                .Append(HttpUtility.HtmlEncode(HiddenRoadSignItemRepository.ChoiceLetter(correctOrder) + ". " + Convert.ToString(correctText)))
                .Append("</p>");

            string explanationText = explanation as string;

            if (!string.IsNullOrWhiteSpace(explanationText))
            {
                html.Append("<p class=\"practice-review__label\">Explanation:</p>");
                html.Append("<p class=\"practice-review__explanation\">")
                    .Append(HttpUtility.HtmlEncode(explanationText))
                    .Append("</p>");
            }

            return html.ToString();
        }
    }
}
