using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using MyKPPLearn.Common;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    public partial class RoadSignAssets : System.Web.UI.Page
    {
        private const string GenericError = "Something went wrong. Please try again.";
        private const int PageSize = 10;
        private const string InUseMessage = "This road sign is currently used in a Road Sign Game and cannot be deleted. Deactivate it instead.";

        // 0 = creating a new asset; otherwise the asset being edited.
        private int EditingAssetId
        {
            get { return ViewState["EditingAssetId"] == null ? 0 : (int)ViewState["EditingAssetId"]; }
            set { ViewState["EditingAssetId"] = value; }
        }

        private int CurrentPage
        {
            get { return ViewState["CurrentPage"] == null ? 1 : (int)ViewState["CurrentPage"]; }
            set { ViewState["CurrentPage"] = value; }
        }

        protected string UsedText(object usageCount)
        {
            int count = Convert.ToInt32(usageCount);
            return count == 0 ? "Not used" : (count == 1 ? "1 time" : count + " times");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireAdministrator(this);
            ((AdminMaster)Master).PageHeading = "Road Sign Assets";

            if (!IsPostBack)
            {
                FlashMessage.ShowPending(Session, lblMessage);

                try
                {
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
            // Tells Scripts/image-preview.js which <img>/wrapper to update when a file is chosen.
            fuImage.Attributes["data-preview-img"] = previewImg.ClientID;
            fuImage.Attributes["data-preview-wrap"] = previewWrap.ClientID;
        }

        private void ShowList()
        {
            pnlList.Visible = true;
            pnlForm.Visible = false;

            DataTable all = HiddenRoadSignAssetRepository.GetAllForAdmin();
            string term = txtSearch.Text.Trim();
            string status = ddlStatusFilter.SelectedValue;
            bool filtering = term.Length > 0 || status.Length > 0;

            System.Collections.Generic.List<DataRow> matches = all.AsEnumerable()
                .Where(r => (term.Length == 0 || Convert.ToString(r["Name"]).IndexOf(term, StringComparison.CurrentCultureIgnoreCase) >= 0)
                         && (status.Length == 0 || (bool)r["IsActive"] == string.Equals(status, "Active", StringComparison.Ordinal)))
                .ToList();

            int pages = Math.Max(1, (int)Math.Ceiling(matches.Count / (double)PageSize));
            CurrentPage = Math.Min(Math.Max(1, CurrentPage), pages);

            DataTable pageRows = all.Clone();

            foreach (DataRow row in matches.Skip((CurrentPage - 1) * PageSize).Take(PageSize))
            {
                pageRows.ImportRow(row);
            }

            rptAssets.DataSource = pageRows;
            rptAssets.DataBind();

            pnlTable.Visible = matches.Count > 0;
            pnlEmpty.Visible = matches.Count == 0;
            litEmptyText.Text = filtering && matches.Count == 0 ? "No road signs match your filters." : "No Road Sign Assets yet.";

            // Nothing created yet: offer "Add". Nothing matches the filters: offer "Clear Filters".
            btnEmptyAdd.Visible = !filtering;
            btnEmptyClear.Visible = filtering;
            btnClearFilters.Visible = filtering;

            pnlPager.Visible = matches.Count > PageSize;
            btnPrev.Enabled = CurrentPage > 1;
            btnNext.Enabled = CurrentPage < pages;
            int first = matches.Count == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
            litPageInfo.Text = "Showing " + first + "–" + Math.Min(CurrentPage * PageSize, matches.Count) + " of " + matches.Count
                + (matches.Count == 1 ? " road sign" : " road signs");
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            CurrentPage = 1;
            ShowList();
        }

        protected void ddlStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentPage = 1;
            ShowList();
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            ddlStatusFilter.SelectedIndex = 0;
            CurrentPage = 1;
            ShowList();
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            CurrentPage--;
            ShowList();
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            CurrentPage++;
            ShowList();
        }

        private void ShowForm(DataRow asset)
        {
            pnlList.Visible = false;
            pnlForm.Visible = true;

            // Always rendered (never Control.Visible = false) so the preview script can reveal it
            // when a file is chosen; hidden until there is something to show.
            previewWrap.Style["display"] = "none";
            previewImg.Src = string.Empty;
            litKeepImageHint.Text = string.Empty;

            if (asset == null)
            {
                EditingAssetId = 0;
                litFormHeading.Text = "Add Road Sign";
                litFormCrumb.Text = "Add Road Sign";
                txtName.Text = string.Empty;
                rblStatus.SelectedValue = "Active";
                return;
            }

            EditingAssetId = Convert.ToInt32(asset["RoadSignAssetId"]);
            litFormHeading.Text = "Edit Road Sign";
            litFormCrumb.Text = "Edit Road Sign";
            txtName.Text = Convert.ToString(asset["Name"]);
            rblStatus.SelectedValue = (bool)asset["IsActive"] ? "Active" : "Inactive";
            previewImg.Src = ResolveUrl("~/" + Convert.ToString(asset["ImagePath"]));
            previewImg.Alt = "Current image for " + Convert.ToString(asset["Name"]);
            previewWrap.Style["display"] = "block";
            litKeepImageHint.Text = " The current image is shown below; choose a new file to replace it.";
        }

        protected void btnAddAsset_Click(object sender, EventArgs e)
        {
            ShowForm(null);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ShowList();
        }

        protected void rptAssets_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            DataRowView row = (DataRowView)e.Item.DataItem;
            StatusToggle.Apply((LinkButton)e.Item.FindControl("btnStatus"), (bool)row["IsActive"], Convert.ToString(row["Name"]));

            // A road sign placed in a game set can not be deleted (Deactivate it instead), so the action is not offered.
            if (Convert.ToInt32(row["UsageCount"]) > 0)
            {
                LinkButton delete = (LinkButton)e.Item.FindControl("btnDelete");
                delete.Enabled = false;
                delete.ToolTip = "This road sign is used in a game set. Deactivate it instead.";
                delete.Attributes.Remove("data-confirm");
                delete.Attributes["aria-disabled"] = "true";
            }
        }

        protected void rptAssets_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int assetId;

            if (!int.TryParse(Convert.ToString(e.CommandArgument), NumberStyles.None, CultureInfo.InvariantCulture, out assetId))
            {
                return;
            }

            try
            {
                DataRow asset = HiddenRoadSignAssetRepository.GetById(assetId);

                if (asset == null)
                {
                    FlashMessage.Show(lblMessage, "That road sign no longer exists.", true);
                    ShowList();
                    return;
                }

                switch (e.CommandName)
                {
                    case "edit":
                        ShowForm(asset);
                        break;

                    case "toggle":
                        bool activate = !(bool)asset["IsActive"];
                        HiddenRoadSignAssetRepository.SetActive(assetId, activate);
                        FlashMessage.Show(lblMessage, activate
                            ? "Road sign activated."
                            : "Road sign deactivated. It stays in the game sets already using it, but can not be added to new ones.", false);
                        ShowList();
                        break;

                    case "delete":
                        DeleteAsset(assetId, asset);
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

        private void DeleteAsset(int assetId, DataRow asset)
        {
            // Blocked while any hotspot uses the asset: configured Games are never cascade-deleted.
            if (Convert.ToInt32(asset["UsageCount"]) > 0 || !HiddenRoadSignAssetRepository.DeleteIfUnused(assetId))
            {
                FlashMessage.Show(lblMessage, InUseMessage, true);
                ShowList();
                return;
            }

            // The row is gone (committed); only now is the image file removed.
            GameImageStorage.Delete(Convert.ToString(asset["ImagePath"]));
            FlashMessage.Set(Session, "Road sign deleted.");
            Response.Redirect(Request.RawUrl, true);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Page.Validate("AssetForm");

            if (!Page.IsValid)
            {
                return;
            }

            lblNameError.Visible = false;

            // Server-side re-validation: never rely on client-side checks.
            string name = (txtName.Text ?? string.Empty).Trim();
            bool isActive = string.Equals(rblStatus.SelectedValue, "Active", StringComparison.Ordinal);
            int editingId = EditingAssetId;

            if (name.Length == 0 || name.Length > 150)
            {
                FlashMessage.Show(lblMessage, "Road Sign Name is required (up to 150 characters).", true);
                return;
            }

            string newImagePath = null;
            bool committed = false;

            try
            {
                // Two road signs with the same name would look identical in the Road Sign Library.
                if (HiddenRoadSignAssetRepository.NameExists(name, editingId))
                {
                    lblNameError.Text = "A road sign with this name already exists.";
                    lblNameError.Visible = true;
                    txtName.Focus();
                    return;
                }

                DataRow existing = null;

                if (editingId != 0)
                {
                    existing = HiddenRoadSignAssetRepository.GetById(editingId);

                    if (existing == null)
                    {
                        FlashMessage.Show(lblMessage, "That road sign no longer exists.", true);
                        ShowList();
                        return;
                    }
                }

                if (!fuImage.HasFile && editingId == 0)
                {
                    FlashMessage.Show(lblMessage, "Please choose an image for the road sign.", true);
                    return;
                }

                if (fuImage.HasFile)
                {
                    string imageError = GameImageStorage.Validate(fuImage.PostedFile);

                    if (imageError != null)
                    {
                        FlashMessage.Show(lblMessage, imageError, true);
                        return;
                    }

                    newImagePath = GameImageStorage.SaveRoadSign(fuImage.PostedFile);
                }

                if (editingId == 0)
                {
                    HiddenRoadSignAssetRepository.Create(name, newImagePath, isActive);
                    committed = true;
                    FlashMessage.Set(Session, "Road sign created.");
                }
                else
                {
                    string oldImagePath = Convert.ToString(existing["ImagePath"]);
                    HiddenRoadSignAssetRepository.Update(editingId, name, newImagePath, isActive);
                    committed = true;

                    // The replacement is saved; only now is the old (replaced) image file removed.
                    if (newImagePath != null)
                    {
                        GameImageStorage.Delete(oldImagePath);
                    }

                    FlashMessage.Set(Session, "Road sign updated.");
                }

                Response.Redirect(Request.RawUrl, true);
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

                FlashMessage.Show(lblMessage, GenericError, true);
            }
        }
    }
}
