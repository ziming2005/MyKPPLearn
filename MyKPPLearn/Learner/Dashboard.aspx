<%@ Page Title="" Language="C#" MasterPageFile="~/Learner.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="MyKPPLearn.Learner.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Dashboard | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Panel ID="pnlSuccess" runat="server" CssClass="alert alert-success" Visible="false" EnableViewState="false" style="margin-bottom: 24px;">
        Account created successfully. You are now signed in.
    </asp:Panel>

    <div class="dashboard-greeting">
        <h2>Welcome back, <asp:Literal ID="litFullName" runat="server" /></h2>
        <p>Here is a snapshot of your driving theory preparation.</p>
    </div>

    <!-- ================= SUMMARY CARDS ================= -->
    <div class="stat-grid">
        <div class="stat-card">
            <div class="stat-card__label">Lessons Completed</div>
            <div class="stat-card__value">0</div>
        </div>
        <div class="stat-card">
            <div class="stat-card__label">Practice Average</div>
            <div class="stat-card__value">&mdash;</div>
        </div>
        <div class="stat-card">
            <div class="stat-card__label">Mock Exam Best Score</div>
            <div class="stat-card__value">&mdash;</div>
        </div>
        <div class="stat-card">
            <div class="stat-card__label">Learning Progress</div>
            <div class="stat-card__value">0%</div>
        </div>
    </div>

    <!-- ================= CONTINUE LEARNING ================= -->
    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Continue Learning</h2>
        </div>

        <div class="empty-state">
            <div class="empty-state__title">No learning activity yet.</div>
            <p class="empty-state__text">Start learning to begin tracking your lesson progress.</p>
            <a class="btn btn-primary" href="~/Learner/Learn.aspx" runat="server">Start Learning</a>
        </div>
    </div>

    <!-- ================= PRACTICE ================= -->
    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Practice</h2>
        </div>

        <div class="practice-grid--dashboard">
            <div class="card practice-card">
                <div class="card-icon">
                    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><circle cx="8" cy="8" r="4" fill="#DC2626" /><circle cx="16" cy="8" r="4" fill="#F59E0B" /><circle cx="12" cy="15" r="4" fill="#16A34A" /></svg>
                </div>
                <h3>Colour Recognition</h3>
                <p>Practice identifying the meaning of key road and signal colours.</p>
                <div class="practice-card__footer">
                    <a class="btn btn-secondary btn-block" href="~/Learner/Practice.aspx" runat="server">Start Practice</a>
                </div>
            </div>

            <div class="card practice-card">
                <div class="card-icon">
                    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M12 3 L20 8 L12 13 L4 8 Z" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" /><path d="M12 13v8" stroke="currentColor" stroke-width="1.8" /></svg>
                </div>
                <h3>Road Signs</h3>
                <p>Test your knowledge of warning, regulatory and information signs.</p>
                <div class="practice-card__footer">
                    <a class="btn btn-secondary btn-block" href="~/Learner/Practice.aspx" runat="server">Start Practice</a>
                </div>
            </div>

            <div class="card practice-card">
                <div class="card-icon">
                    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M4 17h16M6 17V9a6 6 0 0 1 12 0v8" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" /></svg>
                </div>
                <h3>Rules of the Road</h3>
                <p>Review common driving scenarios and correct road behaviour.</p>
                <div class="practice-card__footer">
                    <a class="btn btn-secondary btn-block" href="~/Learner/Practice.aspx" runat="server">Start Practice</a>
                </div>
            </div>

            <div class="card practice-card">
                <div class="card-icon">
                    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M12 3l7 3v6c0 4.5-3 7.5-7 9-4-1.5-7-4.5-7-9V6l7-3Z" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" /></svg>
                </div>
                <h3>KEJARA &amp; Safety</h3>
                <p>Practice questions on the demerit points system and road safety.</p>
                <div class="practice-card__footer">
                    <a class="btn btn-secondary btn-block" href="~/Learner/Practice.aspx" runat="server">Start Practice</a>
                </div>
            </div>

            <div class="card practice-card practice-card--featured">
                <div class="card-icon">
                    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M12 3 L20 8 L12 13 L4 8 Z" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" /><circle cx="12" cy="8.5" r="2" fill="currentColor" /></svg>
                </div>
                <div class="card-heading-row">
                    <h3>Hidden Road Sign</h3>
                    <span class="badge badge-accent">Interactive</span>
                </div>
                <p>Locate hidden road signs within a road scene and answer identification questions.</p>
                <div class="practice-card__footer">
                    <a class="btn btn-secondary btn-block" href="~/Learner/Practice.aspx" runat="server">Start Hidden Road Sign</a>
                </div>
            </div>
        </div>
    </div>

    <!-- ================= PROGRESS OVERVIEW ================= -->
    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Progress Overview</h2>
        </div>

        <div class="progress-overview-grid">
            <div class="progress-overview-card">
                <div class="progress-overview-card__label">Learning</div>
                <div class="progress-track"><div class="progress-track__fill" style="width: 0%;"></div></div>
                <p class="progress-overview-card__empty" style="margin-top: 10px;">0%</p>
            </div>
            <div class="progress-overview-card">
                <div class="progress-overview-card__label">Practice</div>
                <p class="progress-overview-card__empty">No attempts yet</p>
            </div>
            <div class="progress-overview-card">
                <div class="progress-overview-card__label">Mock Exam</div>
                <p class="progress-overview-card__empty">No attempts yet</p>
            </div>
        </div>
    </div>

    <!-- ================= MOCK EXAM ================= -->
    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Mock Exam</h2>
        </div>

        <div class="card mock-exam-cta">
            <div class="mock-exam-cta__text">
                <h3>Ready to test yourself?</h3>
                <p>Take a timed mock examination covering Colour Blind, Part A, Part B and Part C sections.</p>
            </div>
            <a class="btn btn-primary" href="~/Learner/MockExam.aspx" runat="server">Start Mock Exam</a>
        </div>
    </div>

    <!-- ================= RECENT ACTIVITY ================= -->
    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Recent Activity</h2>
        </div>

        <div class="empty-state">
            <div class="empty-state__title">No recent activity yet.</div>
        </div>
    </div>

</asp:Content>
