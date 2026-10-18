<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="LearnerRecordDetails.aspx.cs" Inherits="MyKPPLearn.Admin.LearnerRecordDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Learner Record Details | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="alert alert-warning">
        UI prototype. This learner record is mock data; nothing is read from or written to the database.
    </div>

    <div id="lrNotFound" hidden>
        <div class="alert alert-error">Learner not found. The learner may have been removed, or the link is not valid.</div>
        <div class="form-actions lr-back">
            <a class="btn btn-secondary" href="~/Admin/LearnerRecords.aspx" runat="server">Back to Learner Records</a>
        </div>
    </div>

    <div id="lrDetail">

        <div class="lr-topback">
            <a class="btn btn-secondary btn-small" href="~/Admin/LearnerRecords.aspx" runat="server">&larr; Back to Learner Records</a>
        </div>

        <!-- ================= LEARNER INFORMATION ================= -->
        <section class="panel-section lr-first-section" aria-labelledby="lrInfoTitle">
            <div class="panel-section__header"><h2 id="lrInfoTitle">Learner Information</h2></div>
            <div class="card lr-info">
                <span id="lrInfoAvatar" class="lr-avatar lr-avatar--lg" aria-hidden="true"></span>
                <dl class="lr-info__list">
                    <div><dt>Full Name</dt><dd id="lrInfoName"></dd></div>
                    <div><dt>Email</dt><dd id="lrInfoEmail"></dd></div>
                    <div><dt>Status</dt><dd><span id="lrInfoStatus" class="status-badge"></span></dd></div>
                    <div><dt>Account Created</dt><dd id="lrInfoCreated"></dd></div>
                </dl>
            </div>
        </section>

        <!-- ================= OVERVIEW ================= -->
        <section class="panel-section" aria-labelledby="lrOverviewTitle">
            <div class="panel-section__header"><h2 id="lrOverviewTitle">Overview</h2></div>
            <div class="stat-grid lr-stats">
                <div class="stat-card stat-card--blue">
                    <span class="dash-icon"><svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20V3H6.5A2.5 2.5 0 0 0 4 5.5v14z"/><path d="M20 17v4H6.5A2.5 2.5 0 0 1 4 19.5"/></svg></span>
                    <div class="stat-card__label">Lessons Completed</div><div class="stat-card__value" id="lrOvLessons"></div>
                </div>
                <div class="stat-card stat-card--green">
                    <span class="dash-icon"><svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><rect x="8" y="2" width="8" height="4" rx="1"/><path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2"/><path d="M9 14l2 2 4-4"/></svg></span>
                    <div class="stat-card__label">Practice Attempts</div><div class="stat-card__value" id="lrOvPractice"></div>
                </div>
                <div class="stat-card stat-card--amber">
                    <span class="dash-icon"><svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><circle cx="12" cy="13" r="8"/><path d="M12 9v4l2.5 2.5"/><path d="M9 2h6"/></svg></span>
                    <div class="stat-card__label">Mock Exams Completed</div><div class="stat-card__value" id="lrOvMock"></div>
                </div>
                <div class="stat-card stat-card--purple">
                    <span class="dash-icon"><svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M6 12h4M8 10v4"/><path d="M15 13h.01M18 11h.01"/><path d="M17.3 5H6.7a4 4 0 0 0-3.9 3.2l-1.5 7A3 3 0 0 0 4.2 19a3 3 0 0 0 2.4-1.2l1.4-2h8l1.4 2a3 3 0 0 0 2.4 1.2 3 3 0 0 0 2.9-3.8l-1.5-7A4 4 0 0 0 17.3 5z"/></svg></span>
                    <div class="stat-card__label">Games Completed</div><div class="stat-card__value" id="lrOvGames"></div>
                </div>
            </div>
        </section>

        <!-- ================= LEARNING PROGRESS ================= -->
        <section class="panel-section" aria-labelledby="lrLearningTitle">
            <div class="panel-section__header"><h2 id="lrLearningTitle">Learning Progress</h2></div>
            <div class="card lr-card" id="lrLearning"></div>
        </section>

        <!-- ================= PRACTICE HISTORY ================= -->
        <section class="panel-section" aria-labelledby="lrPracticeTitle">
            <div class="panel-section__header"><h2 id="lrPracticeTitle">Practice History</h2></div>
            <div class="card lr-card" id="lrPractice"></div>
        </section>

        <!-- ================= MOCK EXAM HISTORY ================= -->
        <section class="panel-section" aria-labelledby="lrMockTitle">
            <div class="panel-section__header"><h2 id="lrMockTitle">Mock Exam History</h2></div>
            <div class="card lr-card" id="lrMock"></div>
        </section>

        <!-- ================= ROAD SIGN GAME HISTORY ================= -->
        <section class="panel-section" aria-labelledby="lrGamesTitle">
            <div class="panel-section__header"><h2 id="lrGamesTitle">Road Sign Game History</h2></div>
            <div class="card lr-card" id="lrGames"></div>
        </section>

        <div class="form-actions lr-back">
            <a class="btn btn-secondary" href="~/Admin/LearnerRecords.aspx" runat="server">Back to Learner Records</a>
        </div>
    </div>

    <noscript><p class="alert alert-error">JavaScript is required to view this prototype.</p></noscript>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/learner-records-data.js") + "?v=20261020-b" %>"></script>
    <script src="<%= ResolveUrl("~/Scripts/learner-records-admin.js") + "?v=20261019t" %>"></script>
</asp:Content>
