<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" ValidateRequest="false" CodeBehind="LearningChapters.aspx.cs" Inherits="MyKPPLearn.Admin.LearningChapters" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Chapters | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Label ID="lblMessage" runat="server" Visible="false" EnableViewState="false" role="status" />

    <!-- ================= LIST ================= -->
    <asp:Panel ID="pnlList" runat="server">
        <div class="page-toolbar">
            <div>
                <h2>Chapters</h2>
                <p>Organise lessons into chapters for learners.</p>
            </div>
            <asp:Button ID="btnAddChapter" runat="server" Text="Add Chapter" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnAddChapter_Click" />
        </div>

        <asp:Panel ID="pnlEmpty" runat="server" CssClass="empty-state" Visible="false">
            <div class="empty-state__title">No learning chapters have been created yet.</div>
            <asp:Button ID="btnAddFirst" runat="server" Text="Add Chapter" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnAddChapter_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlTable" runat="server" CssClass="card table-card">
            <div class="table-wrap">
                <table class="data-table chapters-table">
                    <colgroup>
                        <col style="width: 9%;" />
                        <col style="width: 19%;" />
                        <col style="width: 28%;" />
                        <col style="width: 9%;" />
                        <col style="width: 12%;" />
                        <col style="width: 23%;" />
                    </colgroup>
                    <thead>
                        <tr>
                            <th scope="col">Chapter</th>
                            <th scope="col">Title</th>
                            <th scope="col">Description</th>
                            <th scope="col">Lessons</th>
                            <th scope="col">Status</th>
                            <th scope="col">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptChapters" runat="server" OnItemCommand="rptChapters_ItemCommand" OnItemDataBound="rptChapters_ItemDataBound">
                            <ItemTemplate>
                                <tr class='<%# (bool)Eval("IsActive") ? "" : "row--inactive" %>'>
                                    <td class="data-table__nowrap"><strong><%# Eval("DisplayOrder") %></strong></td>
                                    <td><strong><%# Server.HtmlEncode(Convert.ToString(Eval("Title"))) %></strong></td>
                                    <td class="data-table__muted"><%# Server.HtmlEncode(MyKPPLearn.Common.LearningText.Truncate(Convert.ToString(Eval("Description")), 120)) %></td>
                                    <td class="data-table__nowrap"><%# Eval("LessonCount") %></td>
                                    <td><asp:LinkButton runat="server" ID="btnStatus" CommandName="toggle" CommandArgument='<%# Eval("ChapterId") %>' CausesValidation="false" /></td>
                                    <td>
                                        <div class="row-actions">
                                            <a class="btn btn-secondary btn-small" href='<%# ResolveUrl("~/Admin/Lessons.aspx?chapter=" + Eval("ChapterId")) %>' aria-label='View lessons in Chapter <%# Eval("DisplayOrder") %>'>View</a>
                                            <asp:LinkButton runat="server" CommandName="edit" CommandArgument='<%# Eval("ChapterId") %>' CssClass="btn btn-secondary btn-small" CausesValidation="false">Edit</asp:LinkButton>
                                            <asp:LinkButton runat="server" ID="btnDelete" CommandName="delete" CommandArgument='<%# Eval("ChapterId") %>' CssClass="btn btn-secondary btn-small btn-danger" CausesValidation="false" data-confirm="Are you sure you want to delete this chapter?">Delete</asp:LinkButton>
                                        </div>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </asp:Panel>
    </asp:Panel>

    <!-- ================= ADD / EDIT ================= -->
    <asp:Panel ID="pnlForm" runat="server" Visible="false">
        <nav class="breadcrumb" aria-label="Breadcrumb">
            <a href="~/Admin/LearningChapters.aspx" runat="server">Chapters</a>
            <span aria-hidden="true">/</span>
            <span aria-current="page"><asp:Literal ID="litFormCrumb" runat="server" /></span>
        </nav>

        <div class="page-toolbar">
            <h2><asp:Literal ID="litFormHeading" runat="server" /></h2>
        </div>

        <div class="card form-card">
            <asp:ValidationSummary ID="valSummary" runat="server" CssClass="validation-summary" HeaderText="Please fix the following:" DisplayMode="BulletList" ValidationGroup="ChapterForm" />

            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtChapterNumber" CssClass="form-label">Chapter Number</asp:Label>
                <asp:TextBox ID="txtChapterNumber" runat="server" CssClass="form-input form-input--short" MaxLength="9" />
                <span class="used-numbers"><%= UsedNumbersText %></span>
                <span class="form-hint">The chapter number shown to learners (1, 2, 3...). Each chapter number must be unique.</span>
                <asp:Label ID="lblNumberError" runat="server" CssClass="field-error" Visible="false" EnableViewState="false" role="alert" />
                <asp:RequiredFieldValidator ID="rfvChapterNumber" runat="server" ControlToValidate="txtChapterNumber" CssClass="field-error" Display="Dynamic" ErrorMessage="Chapter number is required." ValidationGroup="ChapterForm" />
                <asp:RegularExpressionValidator ID="revChapterNumber" runat="server" ControlToValidate="txtChapterNumber" CssClass="field-error" Display="Dynamic" ValidationExpression="^\s*[1-9][0-9]{0,8}\s*$" ErrorMessage="Chapter number must be a whole number greater than 0." ValidationGroup="ChapterForm" />
            </div>

            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtTitle" CssClass="form-label">Title</asp:Label>
                <asp:TextBox ID="txtTitle" runat="server" CssClass="form-input" MaxLength="150" />
                <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtTitle" CssClass="field-error" Display="Dynamic" ErrorMessage="Title is required." ValidationGroup="ChapterForm" />
            </div>

            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtDescription" CssClass="form-label">Description</asp:Label>
                <asp:TextBox ID="txtDescription" runat="server" CssClass="form-input" TextMode="MultiLine" Rows="4" MaxLength="1000" />
                <span id="descCounter" class="char-counter" aria-live="off"><span id="descCount">0</span> / 1000</span>
                <asp:RequiredFieldValidator ID="rfvDescription" runat="server" ControlToValidate="txtDescription" CssClass="field-error" Display="Dynamic" ErrorMessage="Description is required." ValidationGroup="ChapterForm" />
                <asp:CustomValidator ID="cvDescriptionLength" runat="server" ControlToValidate="txtDescription" CssClass="field-error" Display="Dynamic" ErrorMessage="Description must be 1000 characters or fewer." ValidationGroup="ChapterForm" OnServerValidate="cvDescriptionLength_ServerValidate" />
            </div>

            <div class="form-group">
                <span class="form-label">Preview</span>
                <span class="form-hint">How this chapter appears to learners on the Learning page.</span>
                <div class="card chapter-preview" aria-hidden="true">
                    <div class="chapter-preview__top">
                        <span class="dash-icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" focusable="false"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20V3H6.5A2.5 2.5 0 0 0 4 5.5v14z"/><path d="M20 17v4H6.5A2.5 2.5 0 0 1 4 19.5"/></svg></span>
                        <span class="badge">Chapter <span id="pvNumber">&mdash;</span></span>
                    </div>
                    <h3 id="pvTitle" class="chapter-preview__title">Chapter title</h3>
                    <p id="pvDescription" class="chapter-preview__text">Chapter description</p>
                    <div class="progress-track"><div class="progress-track__fill" style="width: 0%;"></div></div>
                </div>
            </div>

            <div class="form-actions">
                <asp:Button ID="btnSave" runat="server" Text="Save Chapter" CssClass="btn btn-primary" ValidationGroup="ChapterForm" OnClick="btnSave_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>
        </div>
        <script>
            (function () {
                var number = document.getElementById("<%= txtChapterNumber.ClientID %>");
                var title = document.getElementById("<%= txtTitle.ClientID %>");
                var desc = document.getElementById("<%= txtDescription.ClientID %>");
                var pvNumber = document.getElementById("pvNumber");
                var pvTitle = document.getElementById("pvTitle");
                var pvDesc = document.getElementById("pvDescription");
                var count = document.getElementById("descCount");
                var counter = document.getElementById("descCounter");

                if (!number || !title || !desc) { return; }

                // textContent only: typed text is never interpreted as HTML.
                function update() {
                    var n = number.value.trim();
                    var t = title.value.trim();
                    var d = desc.value.trim();
                    pvNumber.textContent = /^[1-9][0-9]{0,8}$/.test(n) ? n : "—";
                    pvTitle.textContent = t || "Chapter title";
                    pvDesc.textContent = d || "Chapter description";
                    count.textContent = String(d.length);
                    counter.classList.toggle("char-counter--over", d.length > 1000);
                }

                [number, title, desc].forEach(function (input) { input.addEventListener("input", update); });
                update();
            })();
        </script>
    </asp:Panel>

</asp:Content>
