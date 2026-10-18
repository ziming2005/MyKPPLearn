using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    /// <summary>
    /// Game page: lists the playable Hidden Road Sign Game Sets directly (no separate Set-selection page).
    /// A Set is offered only when it is Active and has at least one CONFIGURED road sign; the count shown is
    /// the configured count. Sets are in ascending Set Number order.
    /// </summary>
    public partial class Game : System.Web.UI.Page
    {
        // Best finished game per Set for this learner (empty if none, or if it could not be loaded).
        private readonly Dictionary<int, DataRow> _best = new Dictionary<int, DataRow>();

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireLearner(this);
            ((LearnerMaster)Master).PageHeading = "Road Sign Game";

            try
            {
                try
                {
                    int userId = SessionHelper.CurrentUserId(Session).Value;

                    foreach (DataRow row in HiddenRoadSignAttemptRepository.GetBestByPracticeForUser(userId).Rows)
                    {
                        _best[Convert.ToInt32(row["HiddenRoadSignPracticeId"])] = row;
                    }
                }
                catch (Exception)
                {
                    // The best scores are a nicety; the sets still list without them.
                }

                DataTable sets = HiddenRoadSignPracticeRepository.GetPlayableForLearner();
                rptSets.DataSource = sets;
                rptSets.DataBind();
                pnlEmpty.Visible = sets.Rows.Count == 0;
            }
            catch (Exception)
            {
                pnlEmpty.Visible = false;
                lblError.Text = "Game sets could not be loaded. Please try again.";
                lblError.Visible = true;
            }
        }

        protected string BestText(object practiceId)
        {
            DataRow row;

            if (!_best.TryGetValue(Convert.ToInt32(practiceId), out row))
            {
                return "Not played yet";
            }

            return "&#10003; Best: " + Convert.ToInt32(row["BestCorrect"]).ToString(CultureInfo.InvariantCulture)
                + " / " + Convert.ToInt32(row["BestTotal"]).ToString(CultureInfo.InvariantCulture);
        }

        protected string PlayLabel(object practiceId)
        {
            return _best.ContainsKey(Convert.ToInt32(practiceId)) ? "Play Again" : "Start Game";
        }

        protected string SignCountText(object configuredCount)
        {
            int count = Convert.ToInt32(configuredCount);
            return count + (count == 1 ? " Road Sign" : " Road Signs");
        }
    }
}
