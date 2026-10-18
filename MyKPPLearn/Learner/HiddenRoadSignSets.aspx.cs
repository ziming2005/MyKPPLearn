using System;

namespace MyKPPLearn.Learner
{
    /// <summary>
    /// Former Set-selection page. The Game Sets are now listed directly on Game.aspx, so this address only
    /// redirects there (keeps old links and bookmarks working). Game.aspx enforces the learner login.
    /// </summary>
    public partial class HiddenRoadSignSets : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("~/Learner/Game.aspx", true);
        }
    }
}
