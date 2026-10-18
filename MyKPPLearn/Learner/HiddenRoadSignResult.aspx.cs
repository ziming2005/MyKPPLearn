using System;
using System.Data;
using System.Globalization;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    /// <summary>
    /// Result of a COMPLETED Hidden Road Sign Game. A learner can only open their own attempt, even if
    /// the Set has since been deactivated. There is no pass/fail.
    /// </summary>
    public partial class HiddenRoadSignResult : System.Web.UI.Page
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

            try
            {
                // Ownership is part of the query: someone else's attempt id simply isn't found.
                attempt = HiddenRoadSignAttemptRepository.GetAttemptForUser(attemptId, userId);
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

            int total = Convert.ToInt32(attempt["TotalItems"]);
            int correct = Convert.ToInt32(attempt["CorrectCount"]);

            litSetName.Text = HiddenRoadSignPracticeRepository.FormatSetName(attempt["SetNumber"]);

            int percent = total <= 0 ? 0 : (int)Math.Round(correct * 100.0 / total, MidpointRounding.AwayFromZero);
            int practiceId = Convert.ToInt32(attempt["HiddenRoadSignPracticeId"]);

            litScore.Text = correct + " / " + total;
            litPercent.Text = percent + "%";
            litMessage.Text = percent >= 90 ? "Excellent work!"
                : (percent >= 70 ? "Good job. Keep playing to get even sharper."
                : (percent >= 50 ? "Getting there. Review your mistakes to improve."
                : "Keep practising. Review the answers to learn each sign."));

            int setNumber = Convert.ToInt32(attempt["SetNumber"]);
            bool hasNext = false;

            try
            {
                foreach (DataRow row in HiddenRoadSignAttemptRepository.GetBestByPracticeForUser(userId).Rows)
                {
                    if (Convert.ToInt32(row["HiddenRoadSignPracticeId"]) == practiceId)
                    {
                        litBest.Text = "Your best on this set: " + Convert.ToInt32(row["BestCorrect"]) + " / " + Convert.ToInt32(row["BestTotal"]);
                    }
                }

                foreach (DataRow row in HiddenRoadSignPracticeRepository.GetPlayableForLearner().Rows)
                {
                    if (Convert.ToInt32(row["SetNumber"]) > setNumber)
                    {
                        lnkNextSet.NavigateUrl = "~/Learner/HiddenRoadSignGame.aspx?set=" + Convert.ToInt32(row["HiddenRoadSignPracticeId"]).ToString(CultureInfo.InvariantCulture);
                        lnkNextSet.Visible = true;
                        hasNext = true;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                // The best score and the next set are extras; the result itself still shows.
            }

            lnkReview.CssClass = hasNext ? "btn btn-secondary" : "btn btn-primary";

            lnkReview.NavigateUrl = "~/Learner/HiddenRoadSignReview.aspx?attempt=" + attemptId.ToString(CultureInfo.InvariantCulture);
            lnkTryAgain.NavigateUrl = "~/Learner/HiddenRoadSignGame.aspx?set="
                + Convert.ToInt32(attempt["HiddenRoadSignPracticeId"]).ToString(CultureInfo.InvariantCulture);
        }
    }
}
