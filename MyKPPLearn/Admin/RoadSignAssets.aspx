<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" ValidateRequest="false" CodeBehind="RoadSignAssets.aspx.cs" Inherits="MyKPPLearn.Admin.RoadSignAssets" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Road Sign Assets | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Label ID="lblMessage" runat="server" Visible="false" EnableViewState="false" role="status" />

    <!-- ================= LIST ================= -->
    <asp:Panel ID="pnlList" runat="server">
        <div class="page-toolbar">
            <div>
                <h2>Road Sign Assets</h2>
                <p>Isolated road sign images used by the Road Sign Game hotspots.</p>
            </div>
            <asp:Button ID="btnAddAsset" runat="server" Text="Add Road Sign" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnAddAsset_Click" />
        </div>

        <asp:Panel ID="pnlFilters" runat="server" CssClass="filter-bar" DefaultButton="btnSearch">
            <asp:Label runat="server" AssociatedControlID="txtSearch" CssClass="form-label">Search</asp:Label>
            <asp:TextBox ID="txtSearch" runat="server" CssClass="form-input form-input--auto" placeholder="Search road sign names" MaxLength="100" autocomplete="off" />

            <asp:Label runat="server" AssociatedControlID="ddlStatusFilter" CssClass="form-label">Status</asp:Label>
            <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="form-input form-input--auto" AutoPostBack="true" CausesValidation="false" OnSelectedIndexChanged="ddlStatusFilter_SelectedIndexChanged">
                <asp:ListItem Text="All" Value="" />
                <asp:ListItem Text="Active" Value="Active" />
                <asp:ListItem Text="Inactive" Value="Inactive" />
            </asp:DropDownList>

            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnSearch_Click" />
            <asp:Button ID="btnClearFilters" runat="server" Text="Clear Filters" CssClass="btn btn-secondary" CausesValidation="false" Visible="false" OnClick="btnClearFilters_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlEmpty" runat="server" CssClass="empty-state" Visible="false">
            <div class="empty-state__title"><asp:Literal ID="litEmptyText" runat="server" Text="No Road Sign Assets yet." /></div>
            <asp:Button ID="btnEmptyAdd" runat="server" Text="Add Road Sign" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnAddAsset_Click" />
            <asp:Button ID="btnEmptyClear" runat="server" Text="Clear Filters" CssClass="btn btn-secondary" CausesValidation="false" Visible="false" OnClick="btnClearFilters_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlTable" runat="server" CssClass="card table-card">
            <div class="table-wrap">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th scope="col">Road Sign</th>
                            <th scope="col">Image</th>
                            <th scope="col">Used</th>
                            <th scope="col">Status</th>
                            <th scope="col">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptAssets" runat="server" OnItemCommand="rptAssets_ItemCommand" OnItemDataBound="rptAssets_ItemDataBound">
                            <ItemTemplate>
                                <tr class='<%# (bool)Eval("IsActive") ? "" : "row--inactive" %>'>
                                    <td><strong><%# Server.HtmlEncode(Convert.ToString(Eval("Name"))) %></strong></td>
                                    <td><img class="asset-thumb" src='<%# ResolveUrl("~/" + Eval("ImagePath")) %>' alt='<%# Server.HtmlEncode(Convert.ToString(Eval("Name"))) %>' /></td>
                                    <td class="data-table__nowrap"><%# UsedText(Eval("UsageCount")) %></td>
                                    <td><asp:LinkButton runat="server" ID="btnStatus" CommandName="toggle" CommandArgument='<%# Eval("RoadSignAssetId") %>' CausesValidation="false" /></td>
                                    <td>
                                        <div class="row-actions">
                                            <asp:LinkButton runat="server" CommandName="edit" CommandArgument='<%# Eval("RoadSignAssetId") %>' CssClass="btn btn-secondary btn-small" CausesValidation="false">Edit</asp:LinkButton>
                                            <asp:LinkButton runat="server" ID="btnDelete" CommandName="delete" CommandArgument='<%# Eval("RoadSignAssetId") %>' CssClass="btn btn-secondary btn-small btn-danger" CausesValidation="false" data-confirm="Are you sure you want to delete this road sign?">Delete</asp:LinkButton>
                                        </div>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlPager" runat="server" CssClass="lr-footer" Visible="false">
            <p class="lr-footer__summary"><asp:Literal ID="litPageInfo" runat="server" /></p>
            <div class="lr-pager">
                <asp:Button ID="btnPrev" runat="server" Text="Previous" CssClass="lr-pager__btn" CausesValidation="false" OnClick="btnPrev_Click" />
                <asp:Button ID="btnNext" runat="server" Text="Next" CssClass="lr-pager__btn" CausesValidation="false" OnClick="btnNext_Click" />
            </div>
        </asp:Panel>
    </asp:Panel>

    <!-- ================= ADD / EDIT ================= -->
    <asp:Panel ID="pnlForm" runat="server" Visible="false">
        <nav class="breadcrumb" aria-label="Breadcrumb">
            <a href="~/Admin/RoadSignAssets.aspx" runat="server">Road Sign Assets</a>
            <span aria-hidden="true">/</span>
            <span aria-current="page"><asp:Literal ID="litFormCrumb" runat="server" /></span>
        </nav>

        <div class="page-toolbar">
            <h2><asp:Literal ID="litFormHeading" runat="server" /></h2>
        </div>

        <div class="card form-card">
            <asp:ValidationSummary ID="valSummary" runat="server" CssClass="validation-summary" HeaderText="Please fix the following:" DisplayMode="BulletList" ValidationGroup="AssetForm" />

            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtName" CssClass="form-label">Road Sign Name</asp:Label>
                <asp:TextBox ID="txtName" runat="server" CssClass="form-input" MaxLength="150" />
                <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName" CssClass="field-error" Display="Dynamic" ErrorMessage="Road Sign Name is required." ValidationGroup="AssetForm" />
                <asp:Label ID="lblNameError" runat="server" CssClass="field-error" Visible="false" EnableViewState="false" role="alert" />
            </div>

            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="fuImage" CssClass="form-label">Image</asp:Label>
                <asp:FileUpload ID="fuImage" runat="server" CssClass="form-input" accept=".jpg,.jpeg,.png,.webp" data-max-bytes="2097152" data-error-el="assetImageError" />
                <span class="form-hint">JPG, PNG or WEBP, up to 2 MB.</span>
                <asp:Literal ID="litKeepImageHint" runat="server" />
                <p id="assetImageError" class="field-error" role="alert" hidden></p>
            </div>

            <div class="form-group">
                <span class="form-label">Image Preview</span>
                <div id="previewWrap" runat="server" class="image-preview-box">
                    <img id="previewImg" runat="server" alt="" />
                </div>
            </div>

            <div class="form-group">
                <span class="form-label">Status</span>
                <asp:RadioButtonList ID="rblStatus" runat="server" CssClass="radio-list" RepeatDirection="Vertical">
                    <asp:ListItem Text="Active" Value="Active" Selected="True" />
                    <asp:ListItem Text="Inactive" Value="Inactive" />
                </asp:RadioButtonList>
                <span class="form-hint">An Inactive road sign stays in the game sets already using it, but can not be added to new ones.</span>
            </div>

            <div class="form-actions">
                <asp:Button ID="btnSave" runat="server" Text="Save Road Sign" CssClass="btn btn-primary" ValidationGroup="AssetForm" OnClick="btnSave_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>
        </div>
    </asp:Panel>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/image-preview.js") + "?v=20261019" %>"></script>
</asp:Content>
