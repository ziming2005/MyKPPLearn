<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="MockExam.aspx.cs" Inherits="MyKPPLearn.Admin.MockExam" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Mock Exam | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="alert alert-warning">
        UI prototype. Mock Exam Sets shown here are frontend-only mock data; nothing is read from or written to the database.
    </div>

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

    <!-- ================= ADD / EDIT ================= -->
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

        <div class="card form-card">

            <div class="form-group">
                <label class="form-label" for="mxFormType">Exam Type</label>
                <select id="mxFormType" class="form-input"></select>
                <span id="mxTypeChangeHint" class="form-hint" hidden>Changing the exam type changes the required questions. Questions that no longer fit are removed, and the set becomes Inactive until its selection is complete again.</span>
                <p id="mxTypeError" class="field-error" role="alert" hidden></p>
            </div>

            <div class="form-group">
                <label class="form-label" for="mxFormNumber">Mock Exam Set Number</label>
                <input id="mxFormNumber" type="text" inputmode="numeric" maxlength="9" autocomplete="off" class="form-input form-input--short" />
                <span id="mxUsedNumbers" class="used-numbers"></span>
                <span class="form-hint">The Mock Exam Set number (1, 2, 3...). Each number must be unique within its exam type.</span>
                <p id="mxNumberError" class="field-error" role="alert" hidden></p>
            </div>

            <dl class="detail-list mx-rules" id="mxFormRules" aria-label="Exam rules (set automatically from Exam Type)"></dl>

            <p id="mxStatusHint" class="form-hint">A new set starts as Inactive. Select its questions with Manage Questions, then activate it from the list.</p>

            <div class="form-group">
                <span class="form-label">Preview</span>
                <span class="form-hint">How this set appears to learners.</span>
                <div class="card mx-preview" aria-hidden="true">
                    <span id="pvType" class="mx-preview__type">Exam type</span>
                    <h3 id="pvSetTitle" class="mx-preview__title">Mock Exam Set</h3>
                    <p id="pvMeta" class="mx-preview__meta"></p>
                    <span class="btn btn-primary btn-block">Start Mock Exam</span>
                </div>
            </div>

            <div class="form-actions">
                <button type="button" id="mxSaveBtn" class="btn btn-primary">Save</button>
                <button type="button" id="mxCancelBtn" class="btn btn-secondary">Cancel</button>
            </div>
        </div>
    </div>

    <!-- ================= MANAGE QUESTIONS ================= -->
    <div id="mxQuestionsView" hidden>
        <nav class="breadcrumb" aria-label="Breadcrumb">
            <a href="~/Admin/MockExam.aspx" runat="server">Mock Exam</a>
            <span aria-hidden="true">/</span>
            <span aria-current="page">Manage Questions</span>
        </nav>

        <div class="page-toolbar">
            <div>
                <h2>Manage Questions</h2>
                <p id="mxQuestionsSubtitle" class="mx-questions__subtitle"></p>
            </div>
        </div>

        <div class="card form-card mx-questions">

            <div class="filter-bar">
                <label class="form-label" for="mxQSection">Section</label>
                <select id="mxQSection" class="form-input form-input--auto"></select>
                <label class="form-label" for="mxQSearch">Search</label>
                <input id="mxQSearch" type="text" class="form-input form-input--auto" placeholder="Search questions" autocomplete="off" />
            </div>

            <p class="form-hint">Select exactly the required number of questions in every section. A section that is full cannot take more until you untick one.</p>

            <div id="mxQGroups"></div>

            <p id="mxQMessage" class="field-error" hidden role="alert"></p>

            <div class="form-actions">
                <button type="button" id="mxQSaveBtn" class="btn btn-primary">Save Selection</button>
                <button type="button" id="mxQBackBtn" class="btn btn-secondary">Back to Mock Exam</button>
            </div>
        </div>
    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/used-numbers.js") %>?v=20261010"></script>
    <script src="<%= ResolveUrl("~/Scripts/mock-exam-demo.js") + "?v=" + MockExamScriptVersion %>"></script>
    <script src="<%= ResolveUrl("~/Scripts/mock-exam-admin-demo.js") + "?v=" + MockExamAdminScriptVersion %>"></script>
</asp:Content>
