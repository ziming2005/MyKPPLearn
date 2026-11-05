<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="LearnerRecords.aspx.cs" Inherits="MyKPPLearn.Admin.LearnerRecords" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Learner Records | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div id="lrToast" class="alert alert-success" role="status" hidden></div>

    <div id="lrList">
        <div class="page-toolbar">
            <div>
                <h2>Learner Records</h2>
                <p>View learner accounts and learning activity.</p>
            </div>
            <button type="button" id="lrAddBtn" class="btn btn-primary">Add Learner</button>
        </div>

        <div class="lr-filters">
            <div class="lr-search">
                <input id="lrSearch" type="search" class="form-input" placeholder="Search by name or email..." aria-label="Search learners by name or email" autocomplete="off" />
            </div>
            <select id="lrStatusFilter" class="form-input form-input--auto" aria-label="Filter learners by status">
                <option value="">All Statuses</option>
                <option value="Active">Active</option>
                <option value="Disabled">Disabled</option>
            </select>
        </div>

        <div id="lrEmpty" class="empty-state" hidden>
            <div id="lrEmptyTitle" class="empty-state__title">No learners found.</div>
            <button type="button" id="lrEmptyClear" class="btn btn-secondary" hidden>Clear Search</button>
        </div>

        <div id="lrTableWrap" class="card table-card">
            <div class="table-wrap">
                <table class="data-table lr-table">
                    <thead>
                        <tr>
                            <th scope="col" class="lr-col-learner">Learner</th>
                            <th scope="col" class="lr-col-email">Email</th>
                            <th scope="col" class="lr-col-status">Status</th>
                            <th scope="col" class="lr-col-joined">Joined</th>
                            <th scope="col" class="lr-col-actions">Actions</th>
                        </tr>
                    </thead>
                    <tbody id="lrTableBody"></tbody>
                </table>
            </div>
        </div>

        <div id="lrFooter" class="lr-footer" hidden>
            <p id="lrSummary" class="lr-footer__summary" aria-live="polite"></p>
            <nav id="lrPager" class="lr-pager" aria-label="Learner pages"></nav>
        </div>
    </div>

    <!-- ================= ADD / EDIT LEARNER ================= -->
    <div id="lrEditModal" class="modal-overlay" hidden>
        <div class="modal-panel card" role="dialog" aria-modal="true" aria-labelledby="lrEditTitle">
            <h3 id="lrEditTitle">Edit Learner</h3>

            <div class="form-group">
                <label class="form-label" for="lrEditName">Full Name</label>
                <input id="lrEditName" type="text" class="form-input" maxlength="150" autocomplete="off" />
                <p id="lrNameError" class="field-error" role="alert" hidden></p>
            </div>

            <div class="form-group">
                <label class="form-label" for="lrEditEmail">Email</label>
                <input id="lrEditEmail" type="email" class="form-input" maxlength="254" autocomplete="off" />
                <span id="lrEmailHint" class="form-hint" hidden>The email is the learner's sign-in and can't be changed.</span>
                <p id="lrEmailError" class="field-error" role="alert" hidden></p>
            </div>

            <div id="lrPasswordGroup" class="form-group" hidden>
                <label class="form-label" for="lrEditPassword">Password</label>
                <input id="lrEditPassword" type="password" class="form-input" maxlength="100" autocomplete="new-password" />
                <span class="form-hint">At least 8 characters, including an uppercase letter, a lowercase letter, and a number.</span>
                <p id="lrPasswordError" class="field-error" role="alert" hidden></p>
            </div>

            <p id="lrEditError" class="field-error" role="alert" hidden></p>

            <div class="form-actions lr-modal-actions">
                <button type="button" class="btn btn-secondary" data-close>Cancel</button>
                <button type="button" id="lrEditSave" class="btn btn-primary">Save Changes</button>
            </div>
        </div>
    </div>

    <!-- ================= CONFIRM (Disable / Enable / Delete) ================= -->
    <div id="lrConfirmModal" class="modal-overlay" hidden>
        <div class="modal-panel card" role="alertdialog" aria-modal="true" aria-labelledby="lrConfirmTitle" aria-describedby="lrConfirmText">
            <h3 id="lrConfirmTitle"></h3>
            <p id="lrConfirmText"></p>
            <div class="form-actions lr-modal-actions">
                <button type="button" class="btn btn-secondary" data-close>Cancel</button>
                <button type="button" id="lrConfirmOk" class="btn btn-primary"></button>
            </div>
        </div>
    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/learner-records-data.js") + "?v=20261103" %>"></script>
    <script src="<%= ResolveUrl("~/Scripts/learner-records-admin.js") + "?v=20261103-msg5" %>"></script>
</asp:Content>
