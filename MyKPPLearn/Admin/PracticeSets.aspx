<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="PracticeSets.aspx.cs" Inherits="MyKPPLearn.Admin.PracticeSets" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Practice Sets | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="alert alert-warning">
        UI prototype. Practice Sets shown here are frontend-only mock data; nothing is read from or written to the database.
    </div>

    <div id="psToast" class="alert alert-success" hidden role="status"></div>

    <!-- ================= LIST ================= -->
    <div id="psListView">
        <div class="page-toolbar">
            <div>
                <h2>Practice Sets</h2>
                <p>Manage the practice sets learners can attempt in each category.</p>
            </div>
            <button type="button" id="psAddBtn" class="btn btn-primary">Add Practice Set</button>
        </div>

        <div class="filter-bar">
            <label class="form-label" for="psCategoryFilter">Category</label>
            <select id="psCategoryFilter" class="form-input form-input--auto"></select>
            <button type="button" id="psClearFilter" class="btn btn-secondary" hidden>Clear Filter</button>
        </div>

        <div id="psEmpty" class="empty-state" hidden>
            <div id="psEmptyTitle" class="empty-state__title">No practice sets have been created yet.</div>
            <button type="button" id="psEmptyAdd" class="btn btn-primary">Add Practice Set</button>
            <button type="button" id="psEmptyClear" class="btn btn-secondary" hidden>Clear Filter</button>
        </div>

        <div id="psTableWrap" class="card table-card">
            <div class="table-wrap">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th scope="col">Practice Set</th>
                            <th scope="col">Category</th>
                            <th scope="col">Questions</th>
                            <th scope="col">Status</th>
                            <th scope="col">Actions</th>
                        </tr>
                    </thead>
                    <tbody id="psTableBody"></tbody>
                </table>
            </div>
        </div>
    </div>

    <!-- ================= ADD / EDIT ================= -->
    <div id="psFormView" hidden>
        <nav class="breadcrumb" aria-label="Breadcrumb">
            <a href="~/Admin/PracticeSets.aspx" runat="server">Practice Sets</a>
            <span aria-hidden="true">/</span>
            <span id="psFormCrumb" aria-current="page">Add Practice Set</span>
        </nav>

        <div class="page-toolbar">
            <div>
                <h2 id="psFormHeading">Add Practice Set</h2>
            </div>
        </div>

        <div class="card form-card">

            <div class="form-group">
                <label class="form-label" for="psFormCategory">Category</label>
                <select id="psFormCategory" class="form-input"></select>
                <span id="psCategoryHint" class="form-hint" hidden>The category can't be changed because this set has questions.</span>
                <p id="psCategoryError" class="field-error" role="alert" hidden></p>
            </div>

            <div class="form-group">
                <label class="form-label" for="psFormNumber">Practice Set Number</label>
                <input id="psFormNumber" type="text" inputmode="numeric" maxlength="9" autocomplete="off" class="form-input form-input--short" />
                <span id="psUsedNumbers" class="used-numbers"></span>
                <span class="form-hint">The Practice Set number shown to learners (1, 2, 3...). Each number must be unique within its category.</span>
                <p id="psNumberError" class="field-error" role="alert" hidden></p>
            </div>

            <p id="psStatusHint" class="form-hint">A new set starts as Inactive. Add its questions in the Question Bank, then activate it from the list.</p>

            <div class="form-group">
                <span class="form-label">Preview</span>
                <span class="form-hint">How this set appears to learners.</span>
                <div class="card ps-preview" aria-hidden="true">
                    <span class="ps-preview__category"><span id="pvCategoryDot" class="ps-cat__dot"></span><span id="pvCategory">Category</span></span>
                    <h3 id="pvSetTitle" class="ps-preview__title">Set</h3>
                    <p id="pvQuestionCount" class="ps-preview__count">0 Questions</p>
                    <span class="btn btn-primary btn-block">Start Practice</span>
                </div>
            </div>

            <div class="form-actions">
                <button type="button" id="psSaveBtn" class="btn btn-primary">Save</button>
                <button type="button" id="psCancelBtn" class="btn btn-secondary">Cancel</button>
            </div>
        </div>
    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/used-numbers.js") %>?v=20261010"></script>
    <script src="<%= ResolveUrl("~/Scripts/practice-demo.js") + "?v=20261019-ps" %>"></script>
    <script src="<%= ResolveUrl("~/Scripts/practice-admin-demo.js") + "?v=20261019-qb2" %>"></script>
</asp:Content>
