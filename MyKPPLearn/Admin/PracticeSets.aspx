<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="PracticeSets.aspx.cs" Inherits="MyKPPLearn.Admin.PracticeSets" %>
<%@ Register Src="~/Controls/QuestionFields.ascx" TagPrefix="uc" TagName="QuestionFields" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Practice Sets | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">


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

    <!-- ================= ADD / EDIT: THE SET AND ITS QUESTIONS ================= -->
    <div id="psFormView" data-app-root='<%= ResolveUrl("~/") %>' hidden>
        <nav class="breadcrumb" aria-label="Breadcrumb">
            <a href="~/Admin/PracticeSets.aspx" runat="server">Practice Sets</a>
            <span aria-hidden="true">/</span>
            <span id="psFormCrumb" aria-current="page">Add Practice Set</span>
        </nav>

        <div class="page-toolbar">
            <div>
                <h2 id="psFormHeading">Add Practice Set</h2>
                <p id="psFormSubtitle" hidden></p>
            </div>
        </div>

        <div id="psSetCard" class="card form-card lesson-form-card">
            <div class="form-group">
                <label class="form-label" for="psFormCategory">Category</label>
                <select id="psFormCategory" class="form-input"></select>
                <p id="psCategoryError" class="field-error" role="alert" hidden></p>
            </div>

            <p id="psNumberInfo" class="form-hint" hidden></p>
        </div>

        <div id="psQuestionsCard" class="card table-card ps-questions">
            <div class="ps-questions__head">
                <h3 id="psQuestionsTitle">Questions</h3>
                <button type="button" id="psAddQuestionBtn" class="btn btn-primary btn-small">Add New Question</button>
            </div>
            <p id="psQuestionsEmpty" class="form-hint ps-questions__empty" hidden>This set has no questions yet. Press Add New Question to write the first one.</p>
            <div id="psQuestionsTableWrap" class="table-wrap">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th scope="col">Question</th>
                            <th scope="col">Status</th>
                            <th scope="col">Actions</th>
                        </tr>
                    </thead>
                    <tbody id="psQuestionsBody"></tbody>
                </table>
            </div>
        </div>

        <div id="psQuestionToast" class="alert alert-success" hidden role="status"></div>

        <div id="psQuestionForm" class="card form-card lesson-form-card ps-question-form" hidden>
            <h3 id="psQuestionHeading">New Question</h3>

            <uc:QuestionFields ID="QuestionFields1" runat="server" />

            <p id="psQuestionHint" class="form-hint">The question is saved to the Question Bank as part of this Practice Set. A new question starts as Active.</p>

            <div class="form-actions">
                <button type="button" id="psQSaveBtn" class="btn btn-primary">Save Question</button>
                <button type="button" id="psQCancelBtn" class="btn btn-secondary">Cancel</button>
            </div>
        </div>

        <div class="form-actions">
            <button type="button" id="psBackBtn" class="btn btn-secondary">Back to Practice Sets</button>
        </div>
    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/practice-admin-db.js") %>?v=20261104-keepresults2"></script>
</asp:Content>
