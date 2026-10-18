using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using MyKPPLearn.Common;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    /// <summary>Result of checking one answer. Nothing is stored by this call.</summary>
    public sealed class HiddenRoadSignAnswerResponse
    {
        public bool ok;
        public bool needsLogin;
        public string error;
        public bool correct;
        public int correctChoiceId;
        public string explanation;
    }

    /// <summary>One answered hotspot sent by the browser when the whole game is finished.</summary>
    public sealed class HiddenRoadSignAnswerDto
    {
        public int itemId;
        public int choiceId;
        public int foundOrder;
    }

    public sealed class HiddenRoadSignCompleteResponse
    {
        public bool ok;
        public bool needsLogin;
        public string error;
        public string redirectUrl;
    }

    /// <summary>
    /// The Hidden Road Sign Game itself. The scene, the hotspots and the A/B/C choices are loaded from
    /// the database and handed to Scripts/hidden-road-sign-game.js WITHOUT any correct-answer or
    /// explanation data. While playing, everything stays in the browser and nothing is saved;
    /// answers are checked by CheckAnswer (read-only) and only the finished game is persisted, in one
    /// transaction, by Complete.
    /// </summary>
    public partial class HiddenRoadSignGame : System.Web.UI.Page
    {
        private const string SessionExpiredMessage = "Your session has expired. Please sign in again.";
        private const string GenericGameError = "Something went wrong. Please try again.";

        public string GameScriptVersion
        {
            get
            {
                string path = Server.MapPath("~/Scripts/hidden-road-sign-game.js");
                return File.GetLastWriteTimeUtc(path).Ticks.ToString(CultureInfo.InvariantCulture);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireLearner(this);
            ((LearnerMaster)Master).PageHeading = "Road Sign Game";

            // An unfinished game is never resumed: do not let the browser cache or restore this page.
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));

            int setId;

            if (!int.TryParse(Request.QueryString["set"], NumberStyles.None, CultureInfo.InvariantCulture, out setId))
            {
                Response.Redirect("~/Learner/Game.aspx", true);
                return;
            }

            DataRow set;
            DataTable items;
            DataTable choices;

            try
            {
                set = HiddenRoadSignPracticeRepository.GetPlayableById(setId);

                if (set == null)
                {
                    // Inactive, missing or empty Sets can't be started, not even by typing the URL.
                    Response.Redirect("~/Learner/Game.aspx", true);
                    return;
                }

                items = HiddenRoadSignItemRepository.GetItemsBySet(setId, true);   // Draft (incomplete) Road Signs are never sent to the learner
                choices = HiddenRoadSignItemRepository.GetChoicesBySet(setId);
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

            string setName = HiddenRoadSignPracticeRepository.FormatSetName(set["SetNumber"]);
            string token = HiddenRoadSignGameSession.Start(Session, setId);

            litSetName.Text = HttpUtility.HtmlEncode(setName);
            litTotal.Text = items.Rows.Count.ToString(CultureInfo.InvariantCulture);
            imgScene.Src = ResolveUrl("~/" + Convert.ToString(set["SceneImagePath"]));

            string gameUrl = ResolveUrl("~/Learner/HiddenRoadSignGame.aspx");
            Dictionary<string, object> game = new Dictionary<string, object>();
            game["setId"] = setId;
            game["token"] = token;
            game["items"] = BuildClientItems(items, choices);
            game["checkUrl"] = gameUrl + "/CheckAnswer";
            game["completeUrl"] = gameUrl + "/Complete";
            game["setsUrl"] = ResolveUrl("~/Learner/Game.aspx");

            // ASP.NET HTML-encodes the attribute on render; the script reads it back with JSON.parse and renders only textContent.
            hrsGame.Attributes["data-game"] = new JavaScriptSerializer().Serialize(game);
        }

        /// <summary>
        /// What the browser needs to play: hotspot geometry, question, sign image and the A/B/C
        /// choices. Deliberately NOT included: which choice is correct, and the explanation.
        /// </summary>
        private List<Dictionary<string, object>> BuildClientItems(DataTable items, DataTable choices)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

            foreach (DataRow item in items.Rows)
            {
                int itemId = Convert.ToInt32(item["HiddenRoadSignItemId"]);
                List<Dictionary<string, object>> choiceList = new List<Dictionary<string, object>>();

                foreach (DataRow choice in choices.Select("HiddenRoadSignItemId = " + itemId.ToString(CultureInfo.InvariantCulture), "DisplayOrder ASC"))
                {
                    Dictionary<string, object> c = new Dictionary<string, object>();
                    c["id"] = Convert.ToInt32(choice["HiddenRoadSignChoiceId"]);
                    c["letter"] = HiddenRoadSignItemRepository.ChoiceLetter(choice["DisplayOrder"]);
                    c["text"] = Convert.ToString(choice["ChoiceText"]);
                    choiceList.Add(c);
                }

                Dictionary<string, object> entry = new Dictionary<string, object>();
                entry["id"] = itemId;
                entry["x"] = Convert.ToDecimal(item["HotspotXPercent"]);
                entry["y"] = Convert.ToDecimal(item["HotspotYPercent"]);
                entry["w"] = Convert.ToDecimal(item["HotspotWidthPercent"]);
                entry["h"] = Convert.ToDecimal(item["HotspotHeightPercent"]);
                entry["question"] = Convert.ToString(item["QuestionText"]);
                entry["signUrl"] = ResolveUrl("~/" + Convert.ToString(item["AssetImagePath"]));
                entry["choices"] = choiceList;
                list.Add(entry);
            }

            return list;
        }

        // ------------------------------------------------------------------
        // Page methods (called by fetch from the game script; never touch Attempt tables unless Complete)
        // ------------------------------------------------------------------

        private static int? CurrentLearnerId()
        {
            System.Web.SessionState.HttpSessionState session = HttpContext.Current.Session;

            if (!SessionHelper.IsAuthenticated(session)
                || !string.Equals(SessionHelper.CurrentRole(session), SessionHelper.RoleLearner, StringComparison.Ordinal))
            {
                return null;
            }

            return SessionHelper.CurrentUserId(session);
        }

        /// <summary>
        /// Immediate feedback for one answer. Read-only: validates that the Set is active, the Item belongs
        /// to it and the choice belongs to the Item, then reports correct/incorrect (plus the correct choice
        /// and the explanation when incorrect). Nothing is inserted or updated.
        /// </summary>
        [WebMethod(EnableSession = true)]
        public static HiddenRoadSignAnswerResponse CheckAnswer(int setId, int itemId, int choiceId)
        {
            if (CurrentLearnerId() == null)
            {
                return new HiddenRoadSignAnswerResponse { needsLogin = true, error = SessionExpiredMessage };
            }

            try
            {
                HiddenRoadSignAnswerCheck check = HiddenRoadSignAttemptRepository.CheckAnswer(setId, itemId, choiceId);

                if (!check.IsValid)
                {
                    return new HiddenRoadSignAnswerResponse { error = "That answer could not be checked. Please reload the game." };
                }

                HiddenRoadSignAnswerResponse response = new HiddenRoadSignAnswerResponse { ok = true, correct = check.IsCorrect };

                if (!check.IsCorrect)
                {
                    response.correctChoiceId = check.CorrectChoiceId;
                    response.explanation = string.IsNullOrWhiteSpace(check.Explanation) ? null : check.Explanation;
                }

                return response;
            }
            catch (Exception)
            {
                return new HiddenRoadSignAnswerResponse { error = GenericGameError };
            }
        }

        /// <summary>
        /// Saves a COMPLETED game. The browser's own numbers are ignored: the server re-validates every
        /// item/choice relationship and recomputes correctness from the database, then writes the Attempt
        /// and all its Answers in one transaction.
        /// </summary>
        [WebMethod(EnableSession = true)]
        public static HiddenRoadSignCompleteResponse Complete(int setId, string token, HiddenRoadSignAnswerDto[] answers)
        {
            int? userId = CurrentLearnerId();

            if (userId == null)
            {
                return new HiddenRoadSignCompleteResponse { needsLogin = true, error = SessionExpiredMessage };
            }

            System.Web.SessionState.HttpSessionState session = HttpContext.Current.Session;

            try
            {
                DateTime startedAtUtc;
                int savedAttemptId;

                if (!HiddenRoadSignGameSession.TryGet(session, token, setId, out startedAtUtc, out savedAttemptId))
                {
                    return new HiddenRoadSignCompleteResponse { error = "This game session has expired. Please start the game again." };
                }

                // Submitting the same finished game twice (double click, retry) never creates a second attempt.
                if (savedAttemptId != 0)
                {
                    return new HiddenRoadSignCompleteResponse { ok = true, redirectUrl = ResultUrl(savedAttemptId) };
                }

                List<HiddenRoadSignAnswerInput> inputs = new List<HiddenRoadSignAnswerInput>();

                if (answers != null)
                {
                    foreach (HiddenRoadSignAnswerDto dto in answers)
                    {
                        if (dto == null)
                        {
                            return new HiddenRoadSignCompleteResponse { error = "This game could not be saved. Please try again." };
                        }

                        inputs.Add(new HiddenRoadSignAnswerInput { ItemId = dto.itemId, ChoiceId = dto.choiceId, FoundOrder = dto.foundOrder });
                    }
                }

                HiddenRoadSignSaveAttemptResult result = HiddenRoadSignAttemptRepository.SaveCompletedAttempt(userId.Value, setId, startedAtUtc, inputs);

                if (result.Error != null)
                {
                    return new HiddenRoadSignCompleteResponse { error = result.Error };
                }

                HiddenRoadSignGameSession.MarkSaved(session, token, result.AttemptId);
                return new HiddenRoadSignCompleteResponse { ok = true, redirectUrl = ResultUrl(result.AttemptId) };
            }
            catch (Exception)
            {
                return new HiddenRoadSignCompleteResponse { error = "Your result could not be saved. Please try again." };
            }
        }

        private static string ResultUrl(int attemptId)
        {
            return VirtualPathUtility.ToAbsolute("~/Learner/HiddenRoadSignResult.aspx") + "?attempt=" + attemptId.ToString(CultureInfo.InvariantCulture);
        }
    }
}
