using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using MyKPPLearn.Common;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    /// <summary>The editor state posted by the final Save (see Scripts/hidden-road-sign-editor.js). Display fields sent along are ignored.</summary>
    public sealed class HiddenRoadSignStateDto
    {
        public List<HiddenRoadSignStateItem> items;
        /// <summary>Database ids of existing road signs the administrator removed.</summary>
        public List<int> removed;
    }

    public sealed class HiddenRoadSignStateItem
    {
        /// <summary>Database id of an existing road sign, 0 for a new one.</summary>
        public int id;
        public int assetId;
        public decimal x;
        public decimal y;
        public decimal w;
        public decimal h;
        public string question;
        public string a;
        public string b;
        public string c;
        public int correct;
        public string explanation;
    }

    /// <summary>
    /// Admin management of Hidden Road Sign Game Sets. Add and Edit are ONE page-state editor with ONE final
    /// Save: the Game Set details, the Scene Image and the complete Road Sign Scene Editor state (placements,
    /// positions, sizes, questions, removals) are posted together and written in a single transaction by
    /// HiddenRoadSignGameSetRepository.Save. Nothing is written while the administrator is merely editing, and
    /// a Game Set with any incomplete road sign is never saved. Scripts/hidden-road-sign-editor.js drives the editor;
    /// every rule is validated again here.
    /// </summary>
    public partial class HiddenRoadSigns : System.Web.UI.Page
    {
        private const string GenericError = "Something went wrong. Please try again.";
        private const string DuplicateMessage = "This set number already exists.";
        private const string OrderMessage = "Set number must be a whole number greater than 0.";
        private const string AttemptHistoryMessage = "This Road Sign Game has learner attempt history and cannot be deleted. Deactivate it instead.";
        private const string StateMessage = "The editor content could not be read. Please reload the page and try again.";
        private const int MaxRoadSigns = 300;

        // True when the final Save was rejected and the page is shown again with the editor state restored.
        private bool _failedSave;

        // Set when the rejected Save was about the Game Set Number (the page then scrolls to that field instead of the Save button).
        private bool _numberErrorShown;

        // 0 = creating a new Set; otherwise the Set being edited.
        private int EditingPracticeId
        {
            get { return ViewState["EditingPracticeId"] == null ? 0 : (int)ViewState["EditingPracticeId"]; }
            set { ViewState["EditingPracticeId"] = value; }
        }

        private int ViewingPracticeId
        {
            get { return ViewState["ViewingPracticeId"] == null ? 0 : (int)ViewState["ViewingPracticeId"]; }
            set { ViewState["ViewingPracticeId"] = value; }
        }

        /// <summary>"Used Numbers: 1, 2, 4" for the Set field: every existing Game Set number, from the database.</summary>
        public string UsedNumbersText
        {
            get
            {
                try
                {
                    return HttpUtility.HtmlEncode(UsedNumbers.Format(HiddenRoadSignPracticeRepository.GetUsedNumbers()));
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        public string EditorScriptVersion
        {
            get
            {
                string path = Server.MapPath("~/Scripts/hidden-road-sign-editor.js");
                return File.GetLastWriteTimeUtc(path).Ticks.ToString(CultureInfo.InvariantCulture);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireAdministrator(this);
            ((AdminMaster)Master).PageHeading = "Road Sign Game";

            if (!IsPostBack)
            {
                FlashMessage.ShowPending(Session, lblMessage);

                try
                {
                    RouteFromQuery();
                }
                catch (System.Threading.ThreadAbortException)
                {
                    throw;
                }
                catch (Exception)
                {
                    FlashMessage.Show(lblMessage, GenericError, true);
                    ShowList();
                }
            }
        }

        /// <summary>?edit=ID opens that Game Set in the Edit experience (used after a save, and by the old Manage Road Signs URL).</summary>
        private void RouteFromQuery()
        {
            int setId;

            if (!int.TryParse(Request.QueryString["edit"], NumberStyles.None, CultureInfo.InvariantCulture, out setId))
            {
                ShowList();
                return;
            }

            DataRow set = HiddenRoadSignPracticeRepository.GetById(setId);

            if (set == null)
            {
                FlashMessage.Show(lblMessage, "That game set no longer exists.", true);
                ShowList();
                return;
            }

            ShowForm(set);
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!pnlForm.Visible)
            {
                return;
            }

            int editingId = EditingPracticeId;
            DataRow set = null;

            if (editingId != 0)
            {
                try
                {
                    set = HiddenRoadSignPracticeRepository.GetById(editingId);
                }
                catch (Exception)
                {
                    set = null;
                }
            }

            if (_failedSave)
            {
                // The page is long: bring the reason into view instead of leaving it at the top or the bottom.
                string targetId = _numberErrorShown ? lblNumberError.ClientID : (lblSaveError.Visible ? lblSaveError.ClientID : null);

                if (targetId != null)
                {
                    ClientScript.RegisterStartupScript(GetType(), "focusSaveError",
                        "(function(){var e=document.getElementById('" + targetId + "');if(e){e.setAttribute('tabindex','-1');e.scrollIntoView({block:'center'});e.focus({preventScroll:true});}})();", true);
                }
            }

            hrsEditor.Attributes["data-mode"] = editingId == 0 ? "add" : "edit";
            hrsEditor.Attributes["data-dirty"] = _failedSave ? "1" : "0";

            // The editor state is handed to the script as JSON in data attributes. The attribute value is HTML-encoded by
            // ASP.NET when rendered, and the script only ever uses textContent / .value, so administrator-entered text
            // can never become markup.
            string itemsJson = "[]";

            if (set != null)
            {
                imgScene.Src = ResolveUrl("~/" + Convert.ToString(set["SceneImagePath"]));
                imgScene.Alt = "Scene Image for " + HiddenRoadSignPracticeRepository.FormatSetName(set["SetNumber"]);

                try
                {
                    itemsJson = BuildItemsJson(editingId, HiddenRoadSignItemRepository.GetItemsBySet(editingId));
                }
                catch (Exception)
                {
                    itemsJson = "[]";
                }
            }

            hrsEditor.Attributes["data-items"] = itemsJson;

            // Road Sign Library: ACTIVE assets only (new placements). Signs already placed keep
            // rendering from their own item data, even if their asset was deactivated later.
            try
            {
                string libraryJson = BuildLibraryJson(HiddenRoadSignAssetRepository.GetSelectableForSet(editingId));
                hrsEditor.Attributes["data-assets"] = libraryJson;
                bool hasAssets = libraryJson != "[]";
                pnlNoAssets.Visible = !hasAssets;
                hrsLibraryBody.Visible = hasAssets;
            }
            catch (Exception)
            {
                hrsEditor.Attributes["data-assets"] = "[]";
                pnlNoAssets.Visible = true;
                hrsLibraryBody.Visible = false;
            }
        }

        // ------------------------------------------------------------------
        // Panels
        // ------------------------------------------------------------------

        private void ShowList()
        {
            pnlList.Visible = true;
            pnlForm.Visible = false;
            pnlView.Visible = false;

            DataTable sets = HiddenRoadSignPracticeRepository.GetAllForAdmin();
            rptSets.DataSource = sets;
            rptSets.DataBind();
            pnlTable.Visible = sets.Rows.Count > 0;
            pnlEmpty.Visible = sets.Rows.Count == 0;
        }

        private void ShowForm(DataRow set)
        {
            pnlList.Visible = false;
            pnlForm.Visible = true;
            pnlView.Visible = false;

            hfState.Value = string.Empty;
            litKeepImageHint.Text = string.Empty;

            if (set == null)
            {
                EditingPracticeId = 0;
                litFormHeading.Text = "Add Game Set";
                litFormCrumb.Text = "Add Game Set";
                btnSave.Text = "Save Game Set";
                txtSetNumber.Text = NextFreeSetNumber();
                rblStatus.SelectedValue = "Inactive";      // activate once the scene has its road signs
                return;
            }

            EditingPracticeId = Convert.ToInt32(set["HiddenRoadSignPracticeId"]);
            litFormHeading.Text = "Edit Game Set";
            litFormCrumb.Text = "Edit Game Set";
            btnSave.Text = "Save Changes";
            txtSetNumber.Text = Convert.ToString(set["SetNumber"], CultureInfo.InvariantCulture);
            rblStatus.SelectedValue = (bool)set["IsActive"] ? "Active" : "Inactive";

            // One Scene Image only: the Scene Editor below is the preview.
            litKeepImageHint.Text = " Choose a new file to replace the current Scene Image shown in the editor below.";
        }

        /// <summary>The number after the highest existing Game Set (1 for the first); blank if the numbers cannot be read.</summary>
        private static string NextFreeSetNumber()
        {
            try
            {
                int max = 0;

                foreach (int number in HiddenRoadSignPracticeRepository.GetUsedNumbers())
                {
                    if (number > max) { max = number; }
                }

                return (max + 1).ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private void ShowView(DataRow set)
        {
            pnlList.Visible = false;
            pnlForm.Visible = false;
            pnlView.Visible = true;

            ViewingPracticeId = Convert.ToInt32(set["HiddenRoadSignPracticeId"]);

            bool isActive = (bool)set["IsActive"];
            int total = Convert.ToInt32(set["ItemCount"]);
            int configured = Convert.ToInt32(set["ConfiguredCount"]);
            int attemptCount = Convert.ToInt32(set["AttemptCount"]);
            string name = HiddenRoadSignPracticeRepository.FormatSetName(set["SetNumber"]);

            litViewSet.Text = HttpUtility.HtmlEncode(name);
            litViewStatus.Text = isActive
                ? "<span class=\"status-badge status-badge--active\">Active</span>"
                : "<span class=\"status-badge status-badge--inactive\">Inactive</span>";
            litViewItems.Text = string.Format(CultureInfo.InvariantCulture, "{0} road signs · {1} configured · {2} incomplete",
                total, configured, total - configured);
            litViewAttempts.Text = attemptCount.ToString(CultureInfo.InvariantCulture) + (attemptCount == 1 ? " completed attempt" : " completed attempts");
            imgView.Src = ResolveUrl("~/" + Convert.ToString(set["SceneImagePath"]));
            imgView.Alt = "Scene Image for " + name;
            litViewSigns.Text = BuildViewSignsHtml(ViewingPracticeId);
            litViewQuestions.Text = BuildViewQuestionsHtml(ViewingPracticeId);
        }

        /// <summary>A read-only table of every road sign in the set: its question, the correct answer and whether it is complete.</summary>
        private string BuildViewQuestionsHtml(int practiceId)
        {
            DataTable items = HiddenRoadSignItemRepository.GetItemsBySet(practiceId);

            if (items.Rows.Count == 0)
            {
                return "<p class=\"form-hint\">No road signs have been placed in this scene yet.</p>";
            }

            DataTable choices = HiddenRoadSignItemRepository.GetChoicesBySet(practiceId);
            StringBuilder html = new StringBuilder();

            html.Append("<div class=\"table-wrap\"><table class=\"data-table\"><thead><tr>")
                .Append("<th scope=\"col\">#</th><th scope=\"col\">Road Sign</th><th scope=\"col\">Question</th>")
                .Append("<th scope=\"col\">Correct Answer</th><th scope=\"col\">Status</th></tr></thead><tbody>");

            int number = 0;

            foreach (DataRow item in items.Rows)
            {
                number++;
                int itemId = Convert.ToInt32(item["HiddenRoadSignItemId"]);
                bool configured = (bool)item["IsConfigured"];
                string correct = "\u2014";

                foreach (DataRow choice in choices.Select("HiddenRoadSignItemId = " + itemId.ToString(CultureInfo.InvariantCulture) + " AND IsCorrect = true"))
                {
                    correct = HiddenRoadSignItemRepository.ChoiceLetter(choice["DisplayOrder"]) + ". " + Convert.ToString(choice["ChoiceText"]);
                }

                html.Append("<tr><td class=\"data-table__nowrap\"><strong>").Append(number.ToString(CultureInfo.InvariantCulture)).Append("</strong></td>")
                    .Append("<td>").Append(HttpUtility.HtmlEncode(Convert.ToString(item["AssetName"]))).Append("</td>")
                    .Append("<td class=\"data-table__muted\">").Append(HttpUtility.HtmlEncode(item["QuestionText"] as string ?? string.Empty)).Append("</td>")
                    .Append("<td>").Append(HttpUtility.HtmlEncode(correct)).Append("</td>")
                    .Append("<td>").Append(configured
                        ? "<span class=\"status-badge status-badge--active\">Configured</span>"
                        : "<span class=\"status-badge status-badge--inactive\">Incomplete</span>").Append("</td></tr>");
            }

            html.Append("</tbody></table></div>");
            return html.ToString();
        }

        /// <summary>
        /// The CONFIGURED road signs as plain images over the Scene, at their stored percentage geometry
        /// (the same values the learner game uses); Scripts/hidden-road-sign-view.js then fits each image inside its box with the
        /// shared learner formula. Incomplete drafts are not rendered, exactly as for learners.
        /// </summary>
        private string BuildViewSignsHtml(int practiceId)
        {
            StringBuilder html = new StringBuilder();

            foreach (DataRow item in HiddenRoadSignItemRepository.GetItemsBySet(practiceId, true).Rows)
            {
                html.Append("<img class=\"hrs-view-sign\" alt=\"\" src=\"")
                    .Append(HttpUtility.HtmlAttributeEncode(ResolveUrl("~/" + Convert.ToString(item["AssetImagePath"]))))
                    .Append("\" data-x=\"").Append(Percent(item["HotspotXPercent"]))
                    .Append("\" data-y=\"").Append(Percent(item["HotspotYPercent"]))
                    .Append("\" data-w=\"").Append(Percent(item["HotspotWidthPercent"]))
                    .Append("\" data-h=\"").Append(Percent(item["HotspotHeightPercent"]))
                    .Append("\" style=\"left:").Append(Percent(item["HotspotXPercent"]))
                    .Append("%;top:").Append(Percent(item["HotspotYPercent"]))
                    .Append("%;width:").Append(Percent(item["HotspotWidthPercent"]))
                    .Append("%;height:").Append(Percent(item["HotspotHeightPercent"]))
                    .Append("%\" />");
            }

            return html.ToString();
        }

        private static string Percent(object value)
        {
            return Convert.ToDecimal(value, CultureInfo.InvariantCulture).ToString("0.##", CultureInfo.InvariantCulture);
        }

        // ------------------------------------------------------------------
        // Game Set list / details
        // ------------------------------------------------------------------

        protected void btnAddSet_Click(object sender, EventArgs e)
        {
            ShowForm(null);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Admin/HiddenRoadSigns.aspx", true);
        }

        protected void btnViewEdit_Click(object sender, EventArgs e)
        {
            try
            {
                DataRow set = HiddenRoadSignPracticeRepository.GetById(ViewingPracticeId);

                if (set == null)
                {
                    FlashMessage.Show(lblMessage, "That game set no longer exists.", true);
                    ShowList();
                    return;
                }

                ShowForm(set);
            }
            catch (Exception)
            {
                FlashMessage.Show(lblMessage, GenericError, true);
                ShowList();
            }
        }

        /// <summary>Green "x / y configured" pill when every road sign is complete (and there is at least one), amber otherwise.</summary>
        protected string SignsBadgeClass(object configured, object total)
        {
            int all = Convert.ToInt32(total);
            return "mx-qcount" + (all > 0 && Convert.ToInt32(configured) == all ? " mx-qcount--ok" : string.Empty);
        }

        protected string SignsBadgeTitle(object configured, object total)
        {
            int all = Convert.ToInt32(total);

            if (all == 0) { return "No road signs placed yet. Edit the set to add them."; }
            return Convert.ToInt32(configured) == all ? "Every road sign has its question." : "Some road signs are incomplete. Edit the set to finish them.";
        }

        protected void rptSets_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            DataRowView row = (DataRowView)e.Item.DataItem;
            StatusToggle.Apply((LinkButton)e.Item.FindControl("btnStatus"), (bool)row["IsActive"],
                HiddenRoadSignPracticeRepository.FormatSetName(row["SetNumber"]));

            // A set with completed learner attempts can not be deleted (history is kept), so the action is not offered.
            if (Convert.ToInt32(row["AttemptCount"]) > 0)
            {
                LinkButton delete = (LinkButton)e.Item.FindControl("btnDelete");
                delete.Enabled = false;
                delete.ToolTip = "This set has learner attempts. Deactivate it instead.";
                delete.Attributes.Remove("data-confirm");
                delete.Attributes["aria-disabled"] = "true";
            }
        }

        protected void rptSets_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int practiceId;

            if (!int.TryParse(Convert.ToString(e.CommandArgument), NumberStyles.None, CultureInfo.InvariantCulture, out practiceId))
            {
                return;
            }

            try
            {
                DataRow set = HiddenRoadSignPracticeRepository.GetById(practiceId);

                if (set == null)
                {
                    FlashMessage.Show(lblMessage, "That game set no longer exists.", true);
                    ShowList();
                    return;
                }

                switch (e.CommandName)
                {
                    case "view":
                        ShowView(set);
                        break;

                    case "edit":
                        ShowForm(set);
                        break;

                    case "toggle":
                        bool activate = !(bool)set["IsActive"];
                        string setName = HiddenRoadSignPracticeRepository.FormatSetName(set["SetNumber"]);

                        // A set without road signs, or with incomplete ones, would give learners an empty or broken game.
                        if (activate && Convert.ToInt32(set["ItemCount"]) == 0)
                        {
                            FlashMessage.Show(lblMessage, setName + " can not be activated until it has at least one road sign. Edit the set and place road signs in the scene.", true);
                            ShowList();
                            break;
                        }

                        if (activate && Convert.ToInt32(set["ConfiguredCount"]) < Convert.ToInt32(set["ItemCount"]))
                        {
                            FlashMessage.Show(lblMessage, setName + " can not be activated while some road signs are incomplete. Edit the set and finish their questions.", true);
                            ShowList();
                            break;
                        }

                        bool othersActive = HiddenRoadSignPracticeRepository.GetAllForAdmin()
                            .Select("IsActive = true AND HiddenRoadSignPracticeId <> " + practiceId.ToString(CultureInfo.InvariantCulture)).Length > 0;

                        HiddenRoadSignPracticeRepository.SetActive(practiceId, activate);

                        if (activate)
                        {
                            FlashMessage.Show(lblMessage, "Game set activated.", false);
                        }
                        else if (!othersActive)
                        {
                            FlashMessage.ShowWarning(lblMessage, "Game set deactivated. There are no active game sets now, so learners can not play the Road Sign Game.");
                        }
                        else
                        {
                            FlashMessage.Show(lblMessage, "Game set deactivated.", false);
                        }

                        ShowList();
                        break;

                    case "delete":
                        DeleteSet(practiceId, set);
                        break;
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception)
            {
                FlashMessage.Show(lblMessage, GenericError, true);
                ShowList();
            }
        }

        private void DeleteSet(int practiceId, DataRow set)
        {
            HiddenRoadSignDeleteResult result = HiddenRoadSignPracticeRepository.Delete(practiceId);

            if (result == HiddenRoadSignDeleteResult.HasAttempts)
            {
                FlashMessage.Show(lblMessage, AttemptHistoryMessage, true);
                ShowList();
                return;
            }

            if (result == HiddenRoadSignDeleteResult.NotFound)
            {
                FlashMessage.Show(lblMessage, "That game set no longer exists.", true);
                ShowList();
                return;
            }

            // Committed: the Scene Image is no longer referenced by anything, so it can go.
            GameImageStorage.Delete(Convert.ToString(set["SceneImagePath"]));

            string deleted = "Game set deleted.";

            if ((bool)set["IsActive"] && HiddenRoadSignPracticeRepository.GetAllForAdmin().Select("IsActive = true").Length == 0)
            {
                deleted += " There are no active game sets now, so learners can not play the Road Sign Game.";
            }

            FlashMessage.Set(Session, deleted);
            Response.Redirect(Request.RawUrl, true);
        }

        // ------------------------------------------------------------------
        // Editor data for the script
        // ------------------------------------------------------------------

        private string BuildLibraryJson(DataTable assets)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

            foreach (DataRow row in assets.Rows)
            {
                if (!(bool)row["IsActive"])
                {
                    continue;
                }

                Dictionary<string, object> entry = new Dictionary<string, object>();
                entry["id"] = Convert.ToInt32(row["RoadSignAssetId"]);
                entry["name"] = Convert.ToString(row["Name"]);
                entry["img"] = ResolveUrl("~/" + Convert.ToString(row["ImagePath"]));
                list.Add(entry);
            }

            return new JavaScriptSerializer().Serialize(list);
        }

        private string BuildItemsJson(int practiceId, DataTable items)
        {
            DataTable choices = HiddenRoadSignItemRepository.GetChoicesBySet(practiceId);
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

            foreach (DataRow item in items.Rows)
            {
                int itemId = Convert.ToInt32(item["HiddenRoadSignItemId"]);
                string[] texts = new string[HiddenRoadSignItemRepository.ChoiceCount];
                int correct = 0;

                foreach (DataRow choice in choices.Select("HiddenRoadSignItemId = " + itemId.ToString(CultureInfo.InvariantCulture)))
                {
                    int order = Convert.ToInt32(choice["DisplayOrder"]);

                    if (order >= 1 && order <= texts.Length)
                    {
                        texts[order - 1] = Convert.ToString(choice["ChoiceText"]);

                        if ((bool)choice["IsCorrect"])
                        {
                            correct = order;
                        }
                    }
                }

                Dictionary<string, object> entry = new Dictionary<string, object>();
                entry["id"] = itemId;
                entry["assetId"] = Convert.ToInt32(item["RoadSignAssetId"]);
                entry["assetName"] = Convert.ToString(item["AssetName"]);
                entry["img"] = ResolveUrl("~/" + Convert.ToString(item["AssetImagePath"]));
                entry["question"] = item["QuestionText"] as string ?? string.Empty;
                entry["explanation"] = item["Explanation"] as string ?? string.Empty;
                entry["x"] = Convert.ToDecimal(item["HotspotXPercent"]);
                entry["y"] = Convert.ToDecimal(item["HotspotYPercent"]);
                entry["w"] = Convert.ToDecimal(item["HotspotWidthPercent"]);
                entry["h"] = Convert.ToDecimal(item["HotspotHeightPercent"]);
                entry["choices"] = texts;
                // Only a COMPLETE road sign is loaded as configured; a legacy Draft (or malformed row) loads as incomplete.
                entry["correct"] = correct;
                entry["hasHistory"] = Convert.ToInt32(item["AnswerCount"]) > 0;
                list.Add(entry);
            }

            return new JavaScriptSerializer().Serialize(list);
        }

        // ------------------------------------------------------------------
        // THE ONE SAVE: Save Game Set (Add) / Save Changes (Edit)
        // ------------------------------------------------------------------

        // The reason is shown next to the Save button (the page is long), not up at the top.
        private void RejectSave(string message)
        {
            _failedSave = true;
            FlashMessage.Show(lblSaveError, message, true);
        }

        // A duplicate Game Set Number is shown under the Game Set Number field.
        private void RejectNumber(string message)
        {
            _failedSave = true;
            _numberErrorShown = true;
            lblNumberError.Text = HttpUtility.HtmlEncode(message);
            lblNumberError.Visible = true;
            txtSetNumber.Focus();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            lblNumberError.Visible = false;
            lblSaveError.Visible = false;
            Page.Validate("SetForm");

            if (!Page.IsValid)
            {
                _failedSave = true;
                return;
            }

            int setNumber;

            if (!int.TryParse((txtSetNumber.Text ?? string.Empty).Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out setNumber)
                || setNumber < 1)
            {
                RejectSave(OrderMessage);
                return;
            }

            bool isActive = string.Equals(rblStatus.SelectedValue, "Active", StringComparison.Ordinal);
            int editingId = EditingPracticeId;
            string newImagePath = null;
            bool committed = false;

            try
            {
                // ---- the submitted editor state ----
                HiddenRoadSignStateDto state = ReadState();

                if (state == null)
                {
                    RejectSave(StateMessage);
                    return;
                }

                // ---- Game Set details ----
                DataRow existingSet = null;

                if (editingId != 0)
                {
                    existingSet = HiddenRoadSignPracticeRepository.GetById(editingId);

                    if (existingSet == null)
                    {
                        FlashMessage.Show(lblMessage, "That game set no longer exists.", true);
                        ShowList();
                        return;
                    }
                }

                if (HiddenRoadSignPracticeRepository.SetNumberExists(setNumber, editingId))
                {
                    RejectNumber(DuplicateMessage);
                    return;
                }

                if (editingId == 0 && !fuScene.HasFile)
                {
                    RejectSave("Please choose a Scene Image for the game set.");
                    return;
                }

                if (fuScene.HasFile)
                {
                    string imageError = GameImageStorage.ValidateScene(fuScene.PostedFile);

                    if (imageError != null)
                    {
                        RejectSave(imageError);
                        return;
                    }
                }

                // ---- every road sign (incomplete ones block the whole save) ----
                List<HiddenRoadSignEditorItem> items;
                string itemsError = BuildEditorItems(state, editingId, out items);

                if (itemsError != null)
                {
                    RejectSave(itemsError);
                    return;
                }

                if (isActive && items.Count == 0)
                {
                    RejectSave("An Active game set needs at least one road sign. Place a road sign in the scene, or set the status to Inactive.");
                    return;
                }

                // ---- Scene Image: staged only now that everything else is valid; the replaced file is removed after the commit ----
                if (fuScene.HasFile)
                {
                    newImagePath = GameImageStorage.SaveScene(fuScene.PostedFile);
                }

                HiddenRoadSignSaveSetResult result = HiddenRoadSignGameSetRepository.Save(
                    editingId, setNumber, isActive, newImagePath, items, state.removed ?? new List<int>());

                if (result.Error != null)
                {
                    GameImageStorage.Delete(newImagePath);
                    RejectSave(result.Error);
                    return;
                }

                committed = true;

                if (result.ReplacedScenePath != null)
                {
                    GameImageStorage.Delete(result.ReplacedScenePath);
                }

                if (editingId == 0)
                {
                    FlashMessage.Set(Session, "Game set created successfully.");
                    Response.Redirect("~/Admin/HiddenRoadSigns.aspx", true);
                    return;
                }

                string message = "Changes saved successfully.";

                if (newImagePath != null && items.Count > 0)
                {
                    message += " The Scene Image was replaced, so please review the road sign positions in the scene.";
                }

                FlashMessage.Set(Session, message);
                Response.Redirect("~/Admin/HiddenRoadSigns.aspx", true);
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception)
            {
                if (!committed)
                {
                    GameImageStorage.Delete(newImagePath);
                }

                RejectSave(GenericError);
            }
        }

        private HiddenRoadSignStateDto ReadState()
        {
            string json = (hfState.Value ?? string.Empty).Trim();

            if (json.Length == 0)
            {
                return null;
            }

            try
            {
                HiddenRoadSignStateDto state = new JavaScriptSerializer { MaxJsonLength = 4 * 1024 * 1024 }.Deserialize<HiddenRoadSignStateDto>(json);
                return state != null && state.items != null ? state : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Validates the submitted road signs against the database (never trusting the browser) and converts them for saving.
        /// Returns an error message, or null when every road sign is valid and complete.
        /// </summary>
        private string BuildEditorItems(HiddenRoadSignStateDto state, int practiceId, out List<HiddenRoadSignEditorItem> result)
        {
            result = new List<HiddenRoadSignEditorItem>();

            if (state.items.Count > MaxRoadSigns)
            {
                return "A Game Set can hold at most " + MaxRoadSigns + " road signs.";
            }

            // Existing road signs of this Set, by id (their asset is fixed and their name is shown in messages).
            Dictionary<int, DataRow> stored = new Dictionary<int, DataRow>();

            if (practiceId != 0)
            {
                foreach (DataRow row in HiddenRoadSignItemRepository.GetItemsBySet(practiceId).Rows)
                {
                    stored[Convert.ToInt32(row["HiddenRoadSignItemId"])] = row;
                }
            }

            List<string> incompleteNames = new List<string>();
            HashSet<int> seenIds = new HashSet<int>();

            foreach (HiddenRoadSignStateItem submitted in state.items)
            {
                if (submitted == null)
                {
                    return StateMessage;
                }

                HiddenRoadSignEditorItem item = new HiddenRoadSignEditorItem();
                item.ItemId = submitted.id;
                item.X = Math.Round(submitted.x, 2, MidpointRounding.AwayFromZero);
                item.Y = Math.Round(submitted.y, 2, MidpointRounding.AwayFromZero);
                item.Width = Math.Round(submitted.w, 2, MidpointRounding.AwayFromZero);
                item.Height = Math.Round(submitted.h, 2, MidpointRounding.AwayFromZero);
                item.QuestionText = (submitted.question ?? string.Empty).Trim();
                item.Explanation = (submitted.explanation ?? string.Empty).Trim();
                item.ChoiceTexts = new[]
                {
                    (submitted.a ?? string.Empty).Trim(),
                    (submitted.b ?? string.Empty).Trim(),
                    (submitted.c ?? string.Empty).Trim()
                };
                item.CorrectChoice = submitted.correct;

                string name;

                if (submitted.id > 0)
                {
                    DataRow row;

                    if (!stored.TryGetValue(submitted.id, out row) || !seenIds.Add(submitted.id))
                    {
                        return HiddenRoadSignGameSetRepository.ChangedMessage;
                    }

                    item.RoadSignAssetId = Convert.ToInt32(row["RoadSignAssetId"]);   // the stored asset is kept
                    name = Convert.ToString(row["AssetName"]);
                }
                else if (submitted.id == 0)
                {
                    DataRow asset = HiddenRoadSignAssetRepository.GetById(submitted.assetId);

                    if (asset == null)
                    {
                        return "A road sign in the scene no longer exists in the library. Remove it and choose another one.";
                    }

                    name = Convert.ToString(asset["Name"]);

                    if (!(bool)asset["IsActive"])
                    {
                        return "\"" + name + "\" is inactive and can't be added to a scene. Remove it and choose an active road sign.";
                    }

                    item.RoadSignAssetId = submitted.assetId;
                }
                else
                {
                    return StateMessage;
                }

                string geometryError = HiddenRoadSignItemRepository.ValidateGeometry(item.X, item.Y, item.Width, item.Height);

                if (geometryError != null)
                {
                    return "\"" + name + "\": " + geometryError;
                }

                if (HiddenRoadSignItemRepository.ValidateConfiguration(new HiddenRoadSignItemInput
                {
                    QuestionText = item.QuestionText,
                    Explanation = item.Explanation,
                    ChoiceTexts = item.ChoiceTexts,
                    CorrectChoice = item.CorrectChoice
                }) != null)
                {
                    incompleteNames.Add(name);
                }

                result.Add(item);
            }

            if (incompleteNames.Count > 0)
            {
                return "Complete all Road Sign questions before saving the Game Set. "
                    + incompleteNames.Count + (incompleteNames.Count == 1 ? " road sign still has" : " road signs still have")
                    + " an incomplete question: " + string.Join(", ", incompleteNames) + ".";
            }

            return null;
        }
    }
}
