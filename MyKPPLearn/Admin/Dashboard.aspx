<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="MyKPPLearn.Admin.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Dashboard | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="dashboard-greeting">
        <h2>Welcome back, <asp:Literal ID="litFullName" runat="server" /></h2>
        <p>Here is an overview of MyKPP Learn.</p>
    </div>

    <asp:Panel ID="pnlPartial" runat="server" CssClass="alert alert-warning" Visible="false" role="status">
        Some figures could not be loaded and are shown as a dash. Please refresh the page to try again.
    </asp:Panel>

    <!-- ================= SUMMARY CARDS (each one opens the page that manages it) ================= -->
    <div class="stat-grid">
        <a class="stat-card stat-card--blue stat-card--link" href="~/Admin/LearnerRecords.aspx" runat="server">
            <span class="dash-icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8"/></svg></span>
            <div class="stat-card__label">Total Learners</div>
            <div class="stat-card__value"><asp:Literal ID="litTotalLearners" runat="server" /></div>
        </a>
        <a class="stat-card stat-card--green stat-card--link" href="~/Admin/Lessons.aspx" runat="server">
            <span class="dash-icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20V3H6.5A2.5 2.5 0 0 0 4 5.5v14z"/><path d="M20 17v4H6.5A2.5 2.5 0 0 1 4 19.5"/></svg></span>
            <div class="stat-card__label">Active Lessons</div>
            <div class="stat-card__value"><asp:Literal ID="litTotalLessons" runat="server" Text="0" /></div>
        </a>
        <a class="stat-card stat-card--amber stat-card--link" href="~/Admin/HiddenRoadSigns.aspx" runat="server">
            <span class="dash-icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M6 12h4M8 10v4"/><path d="M15 13h.01M18 11h.01"/><path d="M17.3 5H6.7a4 4 0 0 0-3.9 3.2l-1.5 7A3 3 0 0 0 4.2 19a3 3 0 0 0 2.4-1.2l1.4-2h8l1.4 2a3 3 0 0 0 2.4 1.2 3 3 0 0 0 2.9-3.8l-1.5-7A4 4 0 0 0 17.3 5z"/></svg></span>
            <div class="stat-card__label">Active Game Sets</div>
            <div class="stat-card__value"><asp:Literal ID="litOverviewHiddenRoadSigns" runat="server" Text="0" /></div>
        </a>
        <a class="stat-card stat-card--purple stat-card--link" href="~/Admin/LearnerRecords.aspx" runat="server">
            <span class="dash-icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M19 8v6M16 11h6"/></svg></span>
            <div class="stat-card__label">New Learners (7 days)</div>
            <div class="stat-card__value"><asp:Literal ID="litNewLearners" runat="server" Text="0" /></div>
        </a>
    </div>

    <!-- ================= MANAGEMENT OVERVIEW ================= -->
    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Management Overview</h2>
        </div>

        <div class="management-grid">
            <div class="card management-card stat-card--blue">
                <span class="dash-icon dash-icon--lg"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8"/></svg></span>
                <h3>Learner Records</h3>
                <p>View and manage registered learner accounts.</p>
                <p class="management-card__meta"><asp:Literal ID="litLearnersMeta" runat="server" /></p>
                <div class="management-card__footer">
                    <a class="btn btn-secondary btn-block" href="~/Admin/LearnerRecords.aspx" runat="server">Manage Learner Records</a>
                </div>
            </div>

            <div class="card management-card stat-card--green">
                <span class="dash-icon dash-icon--lg"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20V3H6.5A2.5 2.5 0 0 0 4 5.5v14z"/><path d="M20 17v4H6.5A2.5 2.5 0 0 1 4 19.5"/></svg></span>
                <h3>Learning</h3>
                <p>Manage structured driving theory lessons.</p>
                <p class="management-card__meta"><asp:Literal ID="litLearningMeta" runat="server" /></p>
                <p class="management-card__links">
                    <a href="~/Admin/LearningChapters.aspx" runat="server">Chapters</a>
                    <a href="~/Admin/Lessons.aspx" runat="server">Lessons</a>
                </p>
                <div class="management-card__footer">
                    <a class="btn btn-secondary btn-block" href="~/Admin/LearningChapters.aspx" runat="server">Manage Learning</a>
                </div>
            </div>

            <div class="card management-card stat-card--amber">
                <span class="dash-icon dash-icon--lg"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><rect x="8" y="2" width="8" height="4" rx="1"/><path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2"/><path d="M9 14l2 2 4-4"/></svg></span>
                <h3>Practice Management</h3>
                <p>Manage practice sets, the Road Sign Game and road sign assets.</p>
                <p class="management-card__meta"><asp:Literal ID="litGamesMeta" runat="server" /></p>
                <p class="management-card__links">
                    <a href="~/Admin/PracticeSets.aspx" runat="server">Practice Sets</a>
                    <a href="~/Admin/QuestionBank.aspx" runat="server">Question Bank</a>
                    <a href="~/Admin/HiddenRoadSigns.aspx" runat="server">Road Sign Game</a>
                    <a href="~/Admin/RoadSignAssets.aspx" runat="server">Assets</a>
                </p>
                <div class="management-card__footer">
                    <a class="btn btn-secondary btn-block" href="~/Admin/PracticeSets.aspx" runat="server">Manage Practice</a>
                </div>
            </div>

            <div class="card management-card stat-card--purple">
                <span class="dash-icon dash-icon--lg"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><circle cx="12" cy="13" r="8"/><path d="M12 9v4l2.5 2.5"/><path d="M9 2h6"/></svg></span>
                <h3>Mock Exam</h3>
                <p>Manage mock exam types, sets and questions.</p>
                <div class="management-card__footer">
                    <a class="btn btn-secondary btn-block" href="~/Admin/MockExam.aspx" runat="server">Manage Mock Exam</a>
                </div>
            </div>
        </div>
    </div>

    <!-- ================= LEARNER ACTIVITY ================= -->
    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Learner Activity</h2>
        </div>

        <asp:Panel ID="pnlNoActivity" runat="server" CssClass="empty-state empty-state--activity" Visible="false">
            <span class="dash-icon dash-icon--lg"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg></span>
            <div class="empty-state__title">No learner activity yet.</div>
            <p>New sign-ups, completed lessons and finished Road Sign Games will show up here.</p>
        </asp:Panel>

        <asp:Panel ID="pnlActivity" runat="server" CssClass="card activity-card" Visible="false">
            <ul class="activity-list">
                <asp:Repeater ID="rptActivity" runat="server">
                    <ItemTemplate>
                        <li class='<%# "activity-item activity-item--" + Eval("Kind") %>'>
                            <span class="activity-item__dot" aria-hidden="true"></span>
                            <span class="activity-item__text"><%# ActivityText(Eval("Kind"), Eval("Who"), Eval("Detail")) %></span>
                            <span class="activity-item__time" title='<%# ExactTime(Eval("At")) %>'><%# TimeAgo(Eval("At")) %></span>
                        </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </asp:Panel>
    </div>

</asp:Content>
