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
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireLearner(this);
            ((LearnerMaster)Master).PageHeading = "Road Sign Game";

            try
            {
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

        protected string SignCountText(object configuredCount)
        {
            int count = Convert.ToInt32(configuredCount);
            return count + (count == 1 ? " Road Sign" : " Road Signs");
        }
    }
}
