using System;
using System.Data;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls;
using MyKPPLearn.Common;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    public partial class LearningChapters : System.Web.UI.Page
    {
        private const string DuplicateMessage = "A chapter with this number already exists.";
        private const string GenericError = "Something went wrong. Please try again.";

        // 0 = creating a new chapter; otherwise the chapter being edited.
        private int EditingChapterId
        {
            get { return ViewState["EditingChapterId"] == null ? 0 : (int)ViewState["EditingChapterId"]; }
            set { ViewState["EditingChapterId"] = value; }
        }

        /// <summary>"Used Numbers: 1, 2, 4" for the Chapter field: every existing Chapter number, from the database.</summary>
        public string UsedNumbersText
        {
            get
            {
                try
                {
                    return HttpUtility.HtmlEncode(UsedNumbers.Format(LearningChapterRepository.GetUsedNumbers()));
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireAdministrator(this);
            ((AdminMaster)Master).PageHeading = "Chapters";

            if (!IsPostBack)
            {
                FlashMessage.ShowPending(Session, lblMessage);
                ShowList();
            }
        }

        private void ShowList()
        {
            pnlList.Visible = true;
            pnlForm.Visible = false;

            try
            {
                DataTable chapters = LearningChapterRepository.GetAllChaptersForAdmin();
                rptChapters.DataSource = chapters;
                rptChapters.DataBind();
                pnlTable.Visible = chapters.Rows.Count > 0;
                pnlEmpty.Visible = chapters.Rows.Count == 0;
            }
            catch (Exception)
            {
                pnlTable.Visible = false;
                pnlEmpty.Visible = false;
                FlashMessage.Show(lblMessage, GenericError, true);
            }
        }

        private void ShowForm(DataRow chapter)
        {
            pnlList.Visible = false;
            pnlForm.Visible = true;

            if (chapter == null)
            {
                EditingChapterId = 0;
                litFormHeading.Text = "Add Chapter";
                litFormCrumb.Text = "Add Chapter";
                txtChapterNumber.Text = NextFreeNumber();
                txtTitle.Text = string.Empty;
                txtDescription.Text = string.Empty;
            }
            else
            {
                EditingChapterId = Convert.ToInt32(chapter["ChapterId"]);
                litFormHeading.Text = "Edit Chapter";
                litFormCrumb.Text = "Edit Chapter";
                txtChapterNumber.Text = Convert.ToString(chapter["DisplayOrder"], CultureInfo.InvariantCulture);
                txtTitle.Text = Convert.ToString(chapter["Title"]);
                txtDescription.Text = Convert.ToString(chapter["Description"]);
            }
        }

        /// <summary>The number after the highest existing chapter (1 for the first chapter); blank if the numbers cannot be read.</summary>
        private static string NextFreeNumber()
        {
            try
            {
                int max = 0;

                foreach (int number in LearningChapterRepository.GetUsedNumbers())
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

        protected void btnAddChapter_Click(object sender, EventArgs e)
        {
            ShowForm(null);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ShowList();
        }

        protected void cvDescriptionLength_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = (args.Value ?? string.Empty).Trim().Length <= 1000;
        }

        protected void rptChapters_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            DataRowView row = (DataRowView)e.Item.DataItem;
            bool isActive = (bool)row["IsActive"];
            string name = "Chapter " + Convert.ToInt32(row["DisplayOrder"]);
            StatusToggle.Apply((LinkButton)e.Item.FindControl("btnStatus"), isActive, name);

            // A chapter that still has lessons cannot be deleted (the server checks too), so don't offer the action.
            if (Convert.ToInt32(row["LessonCount"]) > 0)
            {
                LinkButton delete = (LinkButton)e.Item.FindControl("btnDelete");
                delete.Enabled = false;
                delete.ToolTip = "Remove this chapter's lessons first";
                delete.Attributes.Remove("data-confirm");     // otherwise confirm-modal.js still opens "Delete this item?"
                delete.Attributes["aria-disabled"] = "true";
            }
        }

        protected void rptChapters_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int chapterId;

            if (!int.TryParse(Convert.ToString(e.CommandArgument), NumberStyles.None, CultureInfo.InvariantCulture, out chapterId))
            {
                return;
            }

            try
            {
                DataRow chapter = LearningChapterRepository.GetChapterById(chapterId);

                if (chapter == null)
                {
                    FlashMessage.Show(lblMessage, "That chapter no longer exists.", true);
                    ShowList();
                    return;
                }

                switch (e.CommandName)
                {
                    case "edit":
                        ShowForm(chapter);
                        break;

                    case "toggle":
                        bool activate = !(bool)chapter["IsActive"];
                        LearningChapterRepository.SetChapterActive(chapterId, activate);
                        FlashMessage.Set(Session, activate ? "Chapter activated." : "Chapter deactivated.");
                        Response.Redirect(Request.RawUrl, true);
                        break;

                    case "delete":
                        DeleteChapter(chapterId, chapter);
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

        private void DeleteChapter(int chapterId, DataRow chapter)
        {
            if (Convert.ToInt32(chapter["LessonCount"]) > 0)
            {
                FlashMessage.Show(lblMessage, "This chapter cannot be deleted because it contains lessons.", true);
                ShowList();
                return;
            }

            if (LearningChapterRepository.DeleteChapter(chapterId))
            {
                FlashMessage.Set(Session, "Chapter deleted.");
                Response.Redirect(Request.RawUrl, true);
            }
            else
            {
                FlashMessage.Show(lblMessage, "This chapter cannot be deleted because it contains lessons.", true);
                ShowList();
            }
        }

        // The duplicate-number error sits under the Chapter Number field, where the admin is looking.
        private void ShowNumberError()
        {
            lblNumberError.Text = DuplicateMessage;
            lblNumberError.Visible = true;
            txtChapterNumber.Focus();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            lblNumberError.Visible = false;
            Page.Validate("ChapterForm");

            if (!Page.IsValid)
            {
                return;
            }

            // Server-side re-validation: never trust client-side checks.
            int displayOrder;
            string title = (txtTitle.Text ?? string.Empty).Trim();
            string description = (txtDescription.Text ?? string.Empty).Trim();

            if (!int.TryParse((txtChapterNumber.Text ?? string.Empty).Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out displayOrder)
                || displayOrder < 1)
            {
                FlashMessage.Show(lblMessage, "Chapter number must be a whole number greater than 0.", true);
                return;
            }

            if (title.Length == 0 || title.Length > 150 || description.Length == 0 || description.Length > 1000)
            {
                FlashMessage.Show(lblMessage, "Please provide a title (up to 150 characters) and a description (up to 1000 characters).", true);
                return;
            }

            try
            {
                int editingId = EditingChapterId;

                if (LearningChapterRepository.ChapterDisplayOrderExists(displayOrder, editingId))
                {
                    ShowNumberError();
                    return;
                }

                if (editingId == 0)
                {
                    if (LearningChapterRepository.CreateChapter(displayOrder, title, description) == 0)
                    {
                        ShowNumberError();
                        return;
                    }

                    FlashMessage.Set(Session, "Chapter created.");
                }
                else
                {
                    if (!LearningChapterRepository.UpdateChapter(editingId, displayOrder, title, description))
                    {
                        ShowNumberError();
                        return;
                    }

                    FlashMessage.Set(Session, "Chapter updated.");
                }

                Response.Redirect(Request.RawUrl, true);
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception)
            {
                FlashMessage.Show(lblMessage, GenericError, true);
            }
        }
    }
}
