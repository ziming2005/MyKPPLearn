<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="QuestionBank.aspx.cs" Inherits="MyKPPLearn.Admin.QuestionBank" %>
<%@ Register Src="~/Controls/QuestionFields.ascx" TagPrefix="uc" TagName="QuestionFields" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Question Bank | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">


    <div id="qbToast" class="alert alert-success" hidden role="status"></div>

    <!-- ================= LIST ================= -->
    <div id="qbListView" data-app-root='<%= ResolveUrl("~/") %>'>
        <div class="page-toolbar">
            <div>
                <h2>Question Bank</h2>
                <p>Manage practice questions, answers and explanations.</p>
            </div>
            <button type="button" id="qbAddBtn" class="btn btn-primary">Add Question</button>
        </div>

        <div class="filter-bar">
            <label class="form-label" for="qbCategoryFilter">Category</label>
            <select id="qbCategoryFilter" class="form-input form-input--auto"></select>

            <label class="form-label" for="qbSetFilter">Practice Set</label>
            <select id="qbSetFilter" class="form-input form-input--auto"></select>

            <label class="form-label" for="qbStatusFilter">Status</label>
            <select id="qbStatusFilter" class="form-input form-input--auto">
                <option value="">All</option>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
            </select>

            <label class="form-label" for="qbSearch">Search</label>
            <input id="qbSearch" type="search" class="form-input form-input--auto" placeholder="Search questions" autocomplete="off" />

            <button type="button" id="qbClearFilters" class="btn btn-secondary" hidden>Clear Filters</button>
        </div>

        <div id="qbEmpty" class="empty-state" hidden>
            <div id="qbEmptyTitle" class="empty-state__title">No questions have been created yet.</div>
            <button type="button" id="qbEmptyAdd" class="btn btn-primary">Add Question</button>
            <button type="button" id="qbEmptyClear" class="btn btn-secondary" hidden>Clear Filters</button>
        </div>

        <div id="qbTableWrap" class="card table-card">
            <div class="table-wrap">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th scope="col">Question</th>
                            <th scope="col">Category</th>
                            <th scope="col">Practice Set</th>
                            <th scope="col">Status</th>
                            <th scope="col">Actions</th>
                        </tr>
                    </thead>
                    <tbody id="qbTableBody"></tbody>
                </table>
            </div>
        </div>

        <div id="qbFooter" class="lr-footer" hidden>
            <p id="qbSummary" class="lr-footer__summary" aria-live="polite"></p>
            <nav id="qbPager" class="lr-pager" aria-label="Question pages"></nav>
        </div>
    </div>

    <!-- ================= ADD / EDIT ================= -->
    <div id="qbFormView" hidden>
        <nav class="breadcrumb" aria-label="Breadcrumb">
            <a href="~/Admin/QuestionBank.aspx" runat="server">Question Bank</a>
            <span aria-hidden="true">/</span>
            <span id="qbFormCrumb" aria-current="page">Add Question</span>
        </nav>

        <div class="page-toolbar">
            <div>
                <h2 id="qbFormHeading">Add Question</h2>
            </div>
        </div>

        <div class="card form-card lesson-form-card">

            <div class="form-group">
                <label class="form-label" for="qbFormCategory">Category</label>
                <select id="qbFormCategory" class="form-input"></select>
                <p id="qbCategoryError" class="field-error" role="alert" hidden></p>
            </div>

            <div class="form-group">
                <label class="form-label" for="qbFormSet">Practice Set</label>
                <select id="qbFormSet" class="form-input"></select>
                <span id="qbNoSets" class="form-hint" hidden>This category has no practice sets yet, so the question will be used in mock exams only. <a href="PracticeSets.aspx">Create a practice set</a> to use it in Practice too.</span>
                <p id="qbSetError" class="field-error" role="alert" hidden></p>
            </div>

            <uc:QuestionFields ID="QuestionFields1" runat="server" />

            <p id="qbStatusHint" class="form-hint">A new question starts as Active. Use the Status button in the list to deactivate it.</p>

            <div class="form-actions">
                <button type="button" id="qbSaveBtn" class="btn btn-primary">Save</button>
                <button type="button" id="qbCancelBtn" class="btn btn-secondary">Cancel</button>
            </div>
        </div>
    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/practice-admin-db.js") %>?v=20261108"></script>
</asp:Content>
