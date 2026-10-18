<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="QuestionBank.aspx.cs" Inherits="MyKPPLearn.Admin.QuestionBank" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Question Bank | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="alert alert-warning">
        UI prototype. Questions shown here are frontend-only mock data; nothing is read from or written to the database.
    </div>

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
                <span id="qbNoSets" class="form-hint" hidden>This category has no practice sets yet. <a href="PracticeSets.aspx">Create one in Practice Sets</a> first.</span>
                <p id="qbSetError" class="field-error" role="alert" hidden></p>
            </div>

            <div class="form-group">
                <label class="form-label" for="qbFormText">Question</label>
                <textarea id="qbFormText" class="form-input" rows="3" maxlength="500"></textarea>
                <span class="char-counter"><span id="qbTextCount">0</span> / 500</span>
                <p id="qbTextError" class="field-error" role="alert" hidden></p>
            </div>

            <div class="form-group">
                <label class="form-label" for="qbFormImage">Image (optional)</label>
                <input type="file" id="qbFormImage" class="form-input" accept=".jpg,.jpeg,.png,.webp" />
                <span class="form-hint">JPG, PNG or WEBP, up to 2 MB. UI demo only: the image is previewed locally and is never uploaded.</span>
                <p id="qbImageError" class="field-error" role="alert" hidden></p>

                <div id="qbImagePreviewWrap" class="image-position-preview__image-slot" hidden>
                    <img id="qbImagePreview" alt="" />
                </div>
                <div id="qbImageTools" class="qb-image-tools" hidden>
                    <button type="button" id="qbRemoveImage" class="btn btn-secondary btn-small btn-danger">Remove image</button>
                </div>
            </div>

            <div id="qbAltGroup" class="form-group" hidden>
                <label class="form-label" for="qbFormImageAlt">Image description</label>
                <input id="qbFormImageAlt" type="text" class="form-input" maxlength="200" autocomplete="off" />
                <span class="form-hint">Describe the image for learners who use a screen reader.</span>
                <p id="qbAltError" class="field-error" role="alert" hidden></p>
            </div>

            <div class="form-group">
                <span class="form-label" id="qbOptionsLabel">Options</span>
                <span class="form-hint">Enter three different options (A, B and C) and choose the correct one.</span>

                <div class="qb-option">
                    <span class="qb-option__letter" aria-hidden="true">A</span>
                    <input id="qbFormOptionA" type="text" class="form-input" maxlength="200" aria-label="Option A" autocomplete="off" />
                    <label class="qb-option__correct"><input type="radio" name="qbCorrectAnswer" value="0" /> Correct</label>
                </div>
                <div class="qb-option">
                    <span class="qb-option__letter" aria-hidden="true">B</span>
                    <input id="qbFormOptionB" type="text" class="form-input" maxlength="200" aria-label="Option B" autocomplete="off" />
                    <label class="qb-option__correct"><input type="radio" name="qbCorrectAnswer" value="1" /> Correct</label>
                </div>
                <div class="qb-option">
                    <span class="qb-option__letter" aria-hidden="true">C</span>
                    <input id="qbFormOptionC" type="text" class="form-input" maxlength="200" aria-label="Option C" autocomplete="off" />
                    <label class="qb-option__correct"><input type="radio" name="qbCorrectAnswer" value="2" /> Correct</label>
                </div>

                <p id="qbOptionsError" class="field-error" role="alert" hidden></p>
                <p id="qbCorrectError" class="field-error" role="alert" hidden></p>
            </div>

            <div class="form-group">
                <label class="form-label" for="qbFormExplanation">Explanation</label>
                <textarea id="qbFormExplanation" class="form-input" rows="3" maxlength="500"></textarea>
                <span class="char-counter"><span id="qbExplanationCount">0</span> / 500</span>
                <span class="form-hint">Shown to learners when they answer incorrectly, or review an incorrect/unanswered question.</span>
                <p id="qbExplanationError" class="field-error" role="alert" hidden></p>
            </div>

            <p id="qbStatusHint" class="form-hint">A new question starts as Active. Use the Status button in the list to deactivate it.</p>

            <div class="form-group">
                <span class="form-label">Preview</span>
                <span class="form-hint">How this question appears to learners. The correct option is highlighted for you.</span>
                <div class="card qb-preview" aria-hidden="true">
                    <div id="pvImageWrap" class="qb-preview__image" hidden><img id="pvImage" alt="" /></div>
                    <h3 id="pvQuestion" class="qb-preview__question">Question text</h3>
                    <div id="pvOptions" class="qb-preview__options"></div>
                    <div id="pvExplanation" class="qb-preview__explanation" hidden>
                        <p class="qb-preview__explanation-label">Explanation <span>(shown after a wrong answer)</span></p>
                        <p id="pvExplanationText" class="qb-preview__explanation-text"></p>
                    </div>
                </div>
            </div>

            <div class="form-actions">
                <button type="button" id="qbSaveBtn" class="btn btn-primary">Save</button>
                <button type="button" id="qbCancelBtn" class="btn btn-secondary">Cancel</button>
            </div>
        </div>
    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/practice.js") + "?v=20261021-c" %>"></script>
    <script src="<%= ResolveUrl("~/Scripts/practice-admin.js") + "?v=20261021-c" %>"></script>
</asp:Content>
