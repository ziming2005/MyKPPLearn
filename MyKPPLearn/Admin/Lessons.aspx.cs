using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls;
using MyKPPLearn.Common;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    public partial class Lessons : System.Web.UI.Page
    {
        private const string DuplicateMessage = "This lesson number already exists in the selected chapter.";
        private const string OrderMessage = "Lesson number must be a whole number greater than 0.";
        private const string GenericError = "Something went wrong. Please try again.";

        // 0 = creating a new lesson; otherwise the lesson being edited.
        protected int EditingLessonId
        {
            get { return ViewState["EditingLessonId"] == null ? 0 : (int)ViewState["EditingLessonId"]; }
            set { ViewState["EditingLessonId"] = value; }
        }

        // The lesson shown on the View page (so "Edit Lesson" there knows which one to open).
        private int ViewingLessonId
        {
            get { return ViewState["ViewingLessonId"] == null ? 0 : (int)ViewState["ViewingLessonId"]; }
            set { ViewState["ViewingLessonId"] = value; }
        }

        /// <summary>Lesson numbers already used, per Chapter id, for the "Used Numbers" helper (HTML-attribute-safe JSON).</summary>
        public string UsedNumbersMap
        {
            get
            {
                try
                {
                    return UsedNumbers.MapAttribute(LessonRepository.GetUsedNumbersByChapter());
                }
                catch (Exception)
                {
                    return "{}";
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireAdministrator(this);
            ((AdminMaster)Master).PageHeading = "Lessons";

            if (!IsPostBack)
            {
                FlashMessage.ShowPending(Session, lblMessage);

                try
                {
                    BindChapterDropdowns();

                    // "View" on the Chapters page links here with ?chapter=ID to show only that chapter's lessons.
                    ListItem fromLink = ddlFilterChapter.Items.FindByValue(Request.QueryString["chapter"] ?? string.Empty);

                    if (fromLink != null && fromLink.Value.Length > 0)
                    {
                        ddlFilterChapter.SelectedValue = fromLink.Value;
                    }

                    ShowList();
                }
                catch (Exception)
                {
                    FlashMessage.Show(lblMessage, GenericError, true);
                }
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!pnlForm.Visible)
            {
                return;
            }

            // ListItem attributes are not kept in ViewState, so re-apply data-order on every render.
            // The client-side script reads it to keep the lesson number preview live while typing.
            Dictionary<string, int> chapterOrders = new Dictionary<string, int>();

            try
            {
                foreach (DataRow row in LearningChapterRepository.GetAllChaptersForAdmin().Rows)
                {
                    chapterOrders[Convert.ToString(row["ChapterId"], CultureInfo.InvariantCulture)] = Convert.ToInt32(row["DisplayOrder"]);
                }
            }
            catch (Exception)
            {
            }

            foreach (ListItem item in ddlChapter.Items)
            {
                int order;

                if (chapterOrders.TryGetValue(item.Value, out order))
                {
                    item.Attributes["data-order"] = order.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        private void BindChapterDropdowns()
        {
            DataTable chapters = LearningChapterRepository.GetAllChaptersForAdmin();

            ddlChapter.Items.Clear();
            ddlChapter.Items.Add(new ListItem("Select a chapter", string.Empty));
            ddlFilterChapter.Items.Clear();
            ddlFilterChapter.Items.Add(new ListItem("All chapters", string.Empty));

            foreach (DataRow row in chapters.Rows)
            {
                string value = Convert.ToString(row["ChapterId"], CultureInfo.InvariantCulture);
                string text = "Chapter " + row["DisplayOrder"] + " - " + row["Title"];

                ddlChapter.Items.Add(new ListItem(text, value));
                ddlFilterChapter.Items.Add(new ListItem(text, value));
            }
        }

        private void ShowList()
        {
            pnlList.Visible = true;
            pnlForm.Visible = false;
            pnlView.Visible = false;

            int chapterId;
            DataTable lessons = int.TryParse(ddlFilterChapter.SelectedValue, NumberStyles.None, CultureInfo.InvariantCulture, out chapterId)
                ? LessonRepository.GetLessonsByChapterForAdmin(chapterId)
                : LessonRepository.GetAllLessonsForAdmin();

            string term = txtSearch.Text.Trim();
            bool filtering = term.Length > 0 || ddlFilterChapter.SelectedValue.Length > 0;

            if (term.Length > 0)
            {
                DataTable matches = lessons.Clone();

                foreach (DataRow row in lessons.Rows)
                {
                    string title = Convert.ToString(row["Title"], CultureInfo.InvariantCulture);

                    if (title.IndexOf(term, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        matches.ImportRow(row);
                    }
                }

                lessons = matches;
            }

            litEmptyText.Text = filtering && lessons.Rows.Count == 0
                ? "No lessons match your filters."
                : "No lessons have been created yet.";

            rptLessons.DataSource = lessons;
            rptLessons.DataBind();
            pnlTable.Visible = lessons.Rows.Count > 0;
            pnlEmpty.Visible = lessons.Rows.Count == 0;

            btnClearFilters.Visible = filtering;
            btnEmptyClear.Visible = filtering;
            btnEmptyAdd.Visible = !filtering;
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            ddlFilterChapter.SelectedIndex = 0;
            txtSearch.Text = string.Empty;
            ShowList();
        }

        /// <summary>The number after the highest lesson already in the chapter (1 for an empty chapter); blank if it cannot be worked out.</summary>
        private static string NextFreeLessonNumber(string chapterId)
        {
            int id;

            if (!int.TryParse(chapterId, NumberStyles.None, CultureInfo.InvariantCulture, out id))
            {
                return string.Empty;
            }

            try
            {
                List<int> used;
                int max = 0;

                if (LessonRepository.GetUsedNumbersByChapter().TryGetValue(id, out used))
                {
                    foreach (int number in used)
                    {
                        if (number > max) { max = number; }
                    }
                }

                return (max + 1).ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private void ShowForm(DataRow lesson)
        {
            pnlList.Visible = false;
            pnlForm.Visible = true;
            pnlView.Visible = false;
            // Always rendered (never Control.Visible = false): the client script needs the
            // element present in the DOM so it can reveal it when a new file is chosen. Hidden
            // by default so "no image" never shows an empty image box in the combined preview.
            previewImageSlot.Style["display"] = "none";
            litKeepImageHint.Text = string.Empty;
            pnlImagePreview.CssClass = "image-position-preview";
            pnlRemoveImage.Visible = false;
            chkRemoveImage.Checked = false;

            if (lesson == null)
            {
                EditingLessonId = 0;
                litFormHeading.Text = "Add Lesson";
                litFormCrumb.Text = "Add Lesson";
                ddlChapter.SelectedIndex = 0;
                txtLessonNumber.Text = string.Empty;

                // Coming from a filtered list: start in that chapter, with its next free lesson number.
                string filtered = ddlFilterChapter.SelectedValue;

                if (filtered.Length > 0 && ddlChapter.Items.FindByValue(filtered) != null)
                {
                    ddlChapter.SelectedValue = filtered;
                    txtLessonNumber.Text = NextFreeLessonNumber(filtered);
                }

                txtTitle.Text = string.Empty;
                txtContent.Text = string.Empty;
                rblImagePosition.SelectedValue = LessonRepository.ImagePositionAbove;
                return;
            }

            EditingLessonId = Convert.ToInt32(lesson["LessonId"]);
            litFormHeading.Text = "Edit Lesson";
            litFormCrumb.Text = "Edit Lesson";
            ddlChapter.SelectedValue = Convert.ToString(lesson["ChapterId"], CultureInfo.InvariantCulture);
            txtLessonNumber.Text = Convert.ToString(lesson["DisplayOrder"], CultureInfo.InvariantCulture);
            txtTitle.Text = Convert.ToString(lesson["Title"]);
            txtContent.Text = Convert.ToString(lesson["Content"]);

            string imagePosition = LessonRepository.NormalizeImagePosition(Convert.ToString(lesson["ImagePosition"]));
            rblImagePosition.SelectedValue = imagePosition;

            if (imagePosition == LessonRepository.ImagePositionBelow)
            {
                pnlImagePreview.CssClass = "image-position-preview image-position-preview--below";
            }

            string imagePath = lesson["ImagePath"] as string;

            if (!string.IsNullOrEmpty(imagePath))
            {
                previewImg.Src = ResolveUrl("~/" + imagePath);
                previewImg.Alt = "Current image for " + Convert.ToString(lesson["Title"]);
                previewImageSlot.Style["display"] = "block";
                litKeepImageHint.Text = " The current image is shown in the preview below; choose a new file to replace it.";
                pnlRemoveImage.Visible = true;
            }
        }

        private void ShowView(DataRow lesson)
        {
            pnlList.Visible = false;
            pnlForm.Visible = false;
            pnlView.Visible = true;

            bool isActive = (bool)lesson["IsActive"];
            string title = Convert.ToString(lesson["Title"]);

            ViewingLessonId = Convert.ToInt32(lesson["LessonId"]);
            litViewLesson.Text = LessonRepository.FormatLessonNumber(lesson["ChapterOrder"], lesson["DisplayOrder"]);
            litViewChapter.Text = HttpUtility.HtmlEncode("Chapter " + lesson["ChapterOrder"] + " - " + lesson["ChapterTitle"]);
            litViewTitle.Text = HttpUtility.HtmlEncode(title);
            litViewStatus.Text = isActive
                ? "<span class=\"status-badge status-badge--active\">Active</span>"
                : "<span class=\"status-badge status-badge--inactive\">Inactive</span>";
            litViewContent.Text = LearningText.ToSafeHtml(Convert.ToString(lesson["Content"]));

            string imagePath = lesson["ImagePath"] as string;
            pnlViewAbove.Visible = false;
            pnlViewBelow.Visible = false;

            if (!string.IsNullOrEmpty(imagePath))
            {
                bool isAbove = LessonRepository.NormalizeImagePosition(Convert.ToString(lesson["ImagePosition"])) == LessonRepository.ImagePositionAbove;
                string imageUrl = ResolveUrl("~/" + imagePath);

                if (isAbove)
                {
                    imgViewAbove.Src = imageUrl;
                    imgViewAbove.Alt = title;
                    pnlViewAbove.Visible = true;
                }
                else
                {
                    imgViewBelow.Src = imageUrl;
                    imgViewBelow.Alt = title;
                    pnlViewBelow.Visible = true;
                }
            }
        }

        protected void btnAddLesson_Click(object sender, EventArgs e)
        {
            ShowForm(null);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ShowList();
        }

        protected void btnViewEdit_Click(object sender, EventArgs e)
        {
            try
            {
                DataRow lesson = LessonRepository.GetLessonById(ViewingLessonId);

                if (lesson == null)
                {
                    FlashMessage.Show(lblMessage, "That lesson no longer exists.", true);
                    ShowList();
                    return;
                }

                ShowForm(lesson);
            }
            catch (Exception)
            {
                FlashMessage.Show(lblMessage, GenericError, true);
                ShowList();
            }
        }

        protected void ddlFilterChapter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowList();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            ShowList();
        }

        protected void rptLessons_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            DataRowView row = (DataRowView)e.Item.DataItem;
            bool isActive = (bool)row["IsActive"];
            string name = "Lesson " + LessonRepository.FormatLessonNumber(row["ChapterOrder"], row["DisplayOrder"]);
            StatusToggle.Apply((LinkButton)e.Item.FindControl("btnStatus"), isActive, name);
        }

        protected void rptLessons_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int lessonId;

            if (!int.TryParse(Convert.ToString(e.CommandArgument), NumberStyles.None, CultureInfo.InvariantCulture, out lessonId))
            {
                return;
            }

            try
            {
                DataRow lesson = LessonRepository.GetLessonById(lessonId);

                if (lesson == null)
                {
                    FlashMessage.Show(lblMessage, "That lesson no longer exists.", true);
                    ShowList();
                    return;
                }

                switch (e.CommandName)
                {
                    case "view":
                        ShowView(lesson);
                        break;

                    case "edit":
                        ShowForm(lesson);
                        break;

                    case "toggle":
                        bool activate = !(bool)lesson["IsActive"];
                        LessonRepository.SetLessonActive(lessonId, activate);
                        FlashMessage.Show(lblMessage, activate ? "Lesson activated." : "Lesson deactivated.", false);
                        ShowList();
                        break;

                    case "delete":
                        DeleteLesson(lessonId, lesson);
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

        private void DeleteLesson(int lessonId, DataRow lesson)
        {
            const string blocked = "This lesson cannot be deleted because learner progress has already been recorded. Deactivate the lesson instead.";

            if (LearningProgressRepository.LessonHasProgress(lessonId) || !LessonRepository.DeleteLesson(lessonId))
            {
                FlashMessage.Show(lblMessage, blocked, true);
                ShowList();
                return;
            }

            LessonImageStorage.Delete(lesson["ImagePath"] as string);
            FlashMessage.Set(Session, "Lesson deleted.");
            Response.Redirect(Request.RawUrl, true);
        }

        // The duplicate-number error sits under the Lesson Number field, where the admin is looking.
        private void ShowNumberError()
        {
            lblNumberError.Text = DuplicateMessage;
            lblNumberError.Visible = true;
            txtLessonNumber.Focus();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            lblNumberError.Visible = false;
            Page.Validate("LessonForm");

            if (!Page.IsValid)
            {
                return;
            }

            // Server-side re-validation: never trust client-side checks or the preview.
            int chapterId;
            int displayOrder;
            string title = (txtTitle.Text ?? string.Empty).Trim();
            string content = (txtContent.Text ?? string.Empty).Trim();
            // Never trust the posted value directly, even though it comes from a RadioButtonList:
            // anything other than "Above"/"Below" safely falls back to "Above".
            string imagePosition = LessonRepository.NormalizeImagePosition(rblImagePosition.SelectedValue);

            if (!int.TryParse(ddlChapter.SelectedValue, NumberStyles.None, CultureInfo.InvariantCulture, out chapterId))
            {
                FlashMessage.Show(lblMessage, "Please select a chapter.", true);
                return;
            }

            if (!int.TryParse((txtLessonNumber.Text ?? string.Empty).Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out displayOrder)
                || displayOrder < 1)
            {
                FlashMessage.Show(lblMessage, OrderMessage, true);
                return;
            }

            if (title.Length == 0 || title.Length > 200 || content.Length == 0)
            {
                FlashMessage.Show(lblMessage, "Please provide a title (up to 200 characters) and the lesson content.", true);
                return;
            }

            string newImagePath = null;
            bool saved = false;

            try
            {
                if (LearningChapterRepository.GetChapterById(chapterId) == null)
                {
                    FlashMessage.Show(lblMessage, "The selected chapter no longer exists.", true);
                    return;
                }

                int editingId = EditingLessonId;

                if (LessonRepository.LessonDisplayOrderExists(chapterId, displayOrder, editingId))
                {
                    ShowNumberError();
                    return;
                }

                if (fuImage.HasFile)
                {
                    string imageError = LessonImageStorage.Validate(fuImage.PostedFile);

                    if (imageError != null)
                    {
                        FlashMessage.Show(lblMessage, imageError, true);
                        return;
                    }

                    newImagePath = LessonImageStorage.Save(fuImage.PostedFile);
                }

                if (editingId == 0)
                {
                    if (LessonRepository.CreateLesson(chapterId, displayOrder, title, content, newImagePath, imagePosition) == 0)
                    {
                        LessonImageStorage.Delete(newImagePath);
                        ShowNumberError();
                        return;
                    }

                    saved = true;
                    FlashMessage.Set(Session, "Lesson created.");
                }
                else
                {
                    DataRow existing = LessonRepository.GetLessonById(editingId);
                    string oldImagePath = existing == null ? null : existing["ImagePath"] as string;

                    bool removeImage = pnlRemoveImage.Visible && chkRemoveImage.Checked;

                    if (!LessonRepository.UpdateLesson(editingId, chapterId, displayOrder, title, content, newImagePath, imagePosition, removeImage))
                    {
                        LessonImageStorage.Delete(newImagePath);
                        ShowNumberError();
                        return;
                    }

                    saved = true;

                    // The old file goes when it was replaced, or when the admin asked to remove it.
                    if (newImagePath != null || removeImage)
                    {
                        LessonImageStorage.Delete(oldImagePath);
                    }

                    FlashMessage.Set(Session, "Lesson updated.");
                }

                Response.Redirect(Request.RawUrl, true);
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception)
            {
                // Don't leave an uploaded file behind when the lesson itself was not saved.
                if (!saved)
                {
                    LessonImageStorage.Delete(newImagePath);
                }

                FlashMessage.Show(lblMessage, GenericError, true);
            }
        }
    }
}
