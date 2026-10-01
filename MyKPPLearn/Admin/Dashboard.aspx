<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="MyKPPLearn.Admin.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Dashboard | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="dashboard-greeting">
        <h2>Welcome back, <asp:Literal ID="litFullName" runat="server" /></h2>
        <p>Here is an overview of MyKPP Learn.</p>
    </div>

    <!-- ================= SUMMARY CARDS ================= -->
    <div class="stat-grid">
        <div class="stat-card">
            <div class="stat-card__label">Total Learners</div>
            <div class="stat-card__value"><asp:Literal ID="litTotalLearners" runat="server" /></div>
        </div>
        <div class="stat-card">
            <div class="stat-card__label">Total Lessons</div>
            <div class="stat-card__value">0</div>
        </div>
        <div class="stat-card">
            <div class="stat-card__label">Total Questions</div>
            <div class="stat-card__value">0</div>
        </div>
        <div class="stat-card">
            <div class="stat-card__label">Active Mock Exams</div>
            <div class="stat-card__value">0</div>
        </div>
    </div>

    <!-- ================= QUICK ACTIONS ================= -->
    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Quick Actions</h2>
        </div>

        <div class="quick-actions-grid">
            <a class="btn btn-secondary btn-block" href="~/Admin/LearnerRecords.aspx" runat="server">Add Learner</a>
            <a class="btn btn-secondary btn-block" href="~/Admin/Learning.aspx" runat="server">Add Lesson</a>
            <a class="btn btn-secondary btn-block" href="~/Admin/QuestionBank.aspx" runat="server">Add Question</a>
            <a class="btn btn-secondary btn-block" href="~/Admin/MockExam.aspx" runat="server">Create Mock Exam</a>
        </div>
    </div>

    <!-- ================= MANAGEMENT OVERVIEW ================= -->
    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Management Overview</h2>
        </div>

        <div class="management-grid">
            <div class="card">
                <h3>Learner Records</h3>
                <p>View and manage registered learner accounts.</p>
                <a class="card-action" href="~/Admin/LearnerRecords.aspx" runat="server">Manage Learner Records</a>
            </div>

            <div class="card">
                <h3>Learning</h3>
                <p>Manage structured driving theory lessons.</p>
                <a class="card-action" href="~/Admin/Learning.aspx" runat="server">Manage Learning</a>
            </div>

            <div class="card">
                <h3>Practice Management</h3>
                <p>Practice Sets, Hidden Road Sign, Road Sign Assets.</p>
                <a class="card-action" href="~/Admin/PracticeManagement.aspx" runat="server">Manage Practice</a>
            </div>

            <div class="card">
                <h3>Mock Exam</h3>
                <p>Manage timed mock examination content.</p>
                <a class="card-action" href="~/Admin/MockExam.aspx" runat="server">Manage Mock Exam</a>
            </div>
        </div>
    </div>

    <!-- ================= CONTENT OVERVIEW ================= -->
    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Content Overview</h2>
        </div>

        <div class="content-overview-grid">
            <div class="stat-card">
                <div class="stat-card__label">Practice Sets</div>
                <div class="stat-card__value">0</div>
            </div>
            <div class="stat-card">
                <div class="stat-card__label">Hidden Road Sign Practices</div>
                <div class="stat-card__value">0</div>
            </div>
            <div class="stat-card">
                <div class="stat-card__label">Lessons</div>
                <div class="stat-card__value">0</div>
            </div>
        </div>
    </div>

    <!-- ================= LEARNER ACTIVITY ================= -->
    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Learner Activity</h2>
        </div>

        <div class="learner-activity-panel">
            <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M4 20V10M11 20V4M18 20v-7" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" /></svg>
            <p>Learner activity data will appear here once learning and assessment modules are in use.</p>
        </div>
    </div>

    <!-- ================= RECENT LEARNER ACTIVITY ================= -->
    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Recent Learner Activity</h2>
        </div>

        <div class="empty-state">
            <div class="empty-state__title">No learner activity available yet.</div>
        </div>
    </div>

</asp:Content>
