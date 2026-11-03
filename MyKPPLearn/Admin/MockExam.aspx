<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="MockExam.aspx.cs" Inherits="MyKPPLearn.Admin.MockExam" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Mock Exam | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div id="mxToast" class="alert alert-success" hidden role="status"></div>

    <!-- ================= LIST ================= -->
    <div id="mxListView">
        <div class="page-toolbar">
            <div>
                <h2>Mock Exam</h2>
                <p>Manage timed mock examination sets for each exam type.</p>
            </div>
            <button type="button" id="mxAddBtn" class="btn btn-primary">Add Mock Exam Set</button>
        </div>

        <div class="filter-bar">
            <label class="form-label" for="mxTypeFilter">Exam Type</label>
            <select id="mxTypeFilter" class="form-input form-input--auto"></select>

            <label class="form-label" for="mxStatusFilter">Status</label>
            <select id="mxStatusFilter" class="form-input form-input--auto">
                <option value="">All</option>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
            </select>

            <button type="button" id="mxClearFilters" class="btn btn-secondary" hidden>Clear Filters</button>
        </div>

        <div id="mxEmpty" class="empty-state" hidden>
            <div id="mxEmptyTitle" class="empty-state__title">No mock exam sets have been created yet.</div>
            <button type="button" id="mxEmptyAdd" class="btn btn-primary">Add Mock Exam Set</button>
            <button type="button" id="mxEmptyClear" class="btn btn-secondary" hidden>Clear Filters</button>
        </div>

        <div id="mxTableWrap" class="card table-card">
            <div class="table-wrap">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th scope="col">Set</th>
                            <th scope="col">Exam Type</th>
                            <th scope="col">Questions</th>
                            <th scope="col">Time</th>
                            <th scope="col">Status</th>
                            <th scope="col">Actions</th>
                        </tr>
                    </thead>
                    <tbody id="mxTableBody"></tbody>
                </table>
            </div>
        </div>
    </div>

    <!-- ================= ADD / EDIT: THE SET AND ITS QUESTIONS ================= -->
    <div id="mxFormView" hidden>
        <nav class="breadcrumb" aria-label="Breadcrumb">
            <a href="~/Admin/MockExam.aspx" runat="server">Mock Exam</a>
            <span aria-hidden="true">/</span>
            <span id="mxFormCrumb" aria-current="page">Add Mock Exam Set</span>
        </nav>

        <div class="page-toolbar">
            <div>
                <h2 id="mxFormHeading">Add Mock Exam Set</h2>
            </div>
        </div>

        <div class="card form-card mx-questions">

            <div class="form-group">
                <label class="form-label" for="mxFormType">Exam Type</label>
                <select id="mxFormType" class="form-input"></select>
                <span id="mxTypeHint" class="form-hint" hidden></span>
                <p id="mxTypeError" class="field-error" role="alert" hidden></p>
            </div>

            <p id="mxNumberInfo" class="form-hint" hidden></p>

            <dl class="detail-list mx-rules" id="mxFormRules" aria-label="Exam rules (set automatically from Exam Type)"></dl>

            <h3 class="mx-questions__title">Questions</h3>

            <div class="filter-bar">
                <label class="form-label" for="mxQSection">Section</label>
                <select id="mxQSection" class="form-input form-input--auto"></select>
                <label class="form-label" for="mxQSearch">Search</label>
                <input id="mxQSearch" type="search" class="form-input form-input--auto" placeholder="Search questions" autocomplete="off" />
            </div>

            <div id="mxQGroups"></div>

            <p id="mxQMessage" class="field-error" hidden role="alert"></p>

            <div class="form-actions">
                <button type="button" id="mxSaveBtn" class="btn btn-primary">Save</button>
                <button type="button" id="mxCancelBtn" class="btn btn-secondary">Cancel</button>
            </div>
        </div>
    </div>

    <!-- Picture of a question, opened from its Image badge -->
    <div id="mxImageOverlay" class="modal-overlay" hidden>
        <div class="modal-panel card mx-image-modal" role="dialog" aria-modal="true" aria-labelledby="mxImageTitle">
            <h3 id="mxImageTitle"></h3>
            <img id="mxImagePicture" class="mx-image-modal__picture" alt="" />
            <div class="form-actions">
                <button type="button" id="mxImageCloseBtn" class="btn btn-secondary">Close</button>
            </div>
        </div>
    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/mock-exam.js") + "?v=" + MockExamScriptVersion %>"></script>
    <script src="<%= ResolveUrl("~/Scripts/mock-exam-admin.js") + "?v=" + MockExamAdminScriptVersion %>"></script>
</asp:Content>
