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
        <div class="stat-card stat-card--blue">
            <span class="dash-icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20V3H6.5A2.5 2.5 0 0 0 4 5.5v14z"/><path d="M20 17v4H6.5A2.5 2.5 0 0 1 4 19.5"/></svg></span>
            <div class="stat-card__label">Lessons Completed</div>
            <div class="stat-card__value"><asp:Literal ID="litLessonsCompleted" runat="server" Text="0" /></div>
        </div>
        <div class="stat-card stat-card--green">
            <span class="dash-icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><rect x="8" y="2" width="8" height="4" rx="1"/><path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2"/><path d="M9 14l2 2 4-4"/></svg></span>
            <div class="stat-card__label">Practice Average</div>
            <div class="stat-card__value">&mdash;</div>
        </div>
        <div class="stat-card stat-card--amber">
            <span class="dash-icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M8 21h8M12 17v4M7 4h10v5a5 5 0 0 1-10 0V4z"/><path d="M17 5h3v2a3 3 0 0 1-3 3M7 5H4v2a3 3 0 0 0 3 3"/></svg></span>
            <div class="stat-card__label">Mock Exam Best Score</div>
            <div class="stat-card__value">&mdash;</div>
        </div>
        <div class="stat-card stat-card--purple">
            <span class="dash-icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M22 7 13.5 15.5 8.5 10.5 2 17"/><path d="M16 7h6v6"/></svg></span>
            <div class="stat-card__label">Learning Progress</div>
            <div class="stat-card__value"><asp:Literal ID="litLearningProgress" runat="server" Text="0%" /></div>
        </div>
    </div>

    <!-- ================= CONTINUE LEARNING ================= -->
    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Continue Learning</h2>
        </div>

        <asp:Panel ID="pnlContinueLesson" runat="server" CssClass="card mock-exam-cta continue-cta" Visible="false">
            <span class="dash-icon dash-icon--lg"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><circle cx="12" cy="12" r="10"/><path d="M10 8l6 4-6 4z"/></svg></span>
            <div class="mock-exam-cta__text">
                <p class="continue-label">Lesson <asp:Literal ID="litContinueNumber" runat="server" /></p>
                <h3><asp:Literal ID="litContinueTitle" runat="server" /></h3>
            </div>
            <a id="lnkContinue" runat="server" class="btn btn-primary continue-cta__btn">Continue Learning</a>
        </asp:Panel>

        <asp:Panel ID="pnlContinueAllDone" runat="server" CssClass="empty-state empty-state--white" Visible="false">
            <div class="empty-state__title">All available lessons completed.</div>
            <a class="btn btn-secondary" href="~/Learner/Learn.aspx" runat="server">Review Lessons</a>
        </asp:Panel>

        <asp:Panel ID="pnlContinueNone" runat="server" CssClass="empty-state empty-state--white" Visible="false">
            <div class="empty-state__title">No learning content is available yet.</div>
        </asp:Panel>
    </div>

    <!-- ================= PRACTICE ================= -->
    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Practice</h2>
        </div>

        <div class="practice-grid--dashboard">
            <div class="card practice-card practice-card--red">
                <div class="card-icon">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="13.5" cy="6.5" r="1" /><circle cx="17.5" cy="10.5" r="1" /><circle cx="8.5" cy="7.5" r="1" /><circle cx="6.5" cy="12.5" r="1" /><path d="M12 2C6.5 2 2 6.5 2 12s4.5 10 10 10c.93 0 1.65-.75 1.65-1.69 0-.44-.18-.83-.44-1.12-.29-.29-.44-.65-.44-1.13a1.64 1.64 0 0 1 1.67-1.67h2c3.05 0 5.55-2.5 5.55-5.55C21.97 6.01 17.46 2 12 2z" /></svg>
                </div>
                <h3>Colour Recognition</h3>
                <p>Practice identifying the meaning of key road and signal colours.</p>
                <div class="practice-card__footer">
                    <a class="btn btn-secondary btn-block" href="~/Learner/PracticeSets.aspx?category=colour-recognition" runat="server" aria-label="Start Colour Recognition practice">Start Practice</a>
                </div>
            </div>

            <div class="card practice-card practice-card--blue">
                <div class="card-icon">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M10.3 3.9 1.8 18a2 2 0 0 0 1.7 3h17a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0z" /><path d="M12 9v4M12 17h.01" /></svg>
                </div>
                <h3>Road Signs</h3>
                <p>Test your knowledge of warning, regulatory and information signs.</p>
                <div class="practice-card__footer">
                    <a class="btn btn-secondary btn-block" href="~/Learner/PracticeSets.aspx?category=road-signs" runat="server" aria-label="Start Road Signs practice">Start Practice</a>
                </div>
            </div>

            <div class="card practice-card practice-card--green">
                <div class="card-icon">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 22 8 2M20 22 16 2M12 5v3M12 11v3M12 17v3" /></svg>
                </div>
                <h3>Rules of the Road</h3>
                <p>Review common driving scenarios and correct road behaviour.</p>
                <div class="practice-card__footer">
                    <a class="btn btn-secondary btn-block" href="~/Learner/PracticeSets.aspx?category=rules-of-the-road" runat="server" aria-label="Start Rules of the Road practice">Start Practice</a>
                </div>
            </div>

            <div class="card practice-card practice-card--purple">
                <div class="card-icon">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" /><path d="m9 12 2 2 4-4" /></svg>
                </div>
                <h3>KEJARA &amp; Safety</h3>
                <p>Practice questions on the demerit points system and road safety.</p>
                <div class="practice-card__footer">
                    <a class="btn btn-secondary btn-block" href="~/Learner/PracticeSets.aspx?category=kejara-safety" runat="server" aria-label="Start KEJARA &amp; Safety practice">Start Practice</a>
                </div>
            </div>
        </div>
    </div>

    <!-- ================= ROAD SIGN GAME + MOCK EXAM (side by side) ================= -->
    <div class="dash-pair">

    <!-- ROAD SIGN GAME -->
    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Road Sign Game</h2>
        </div>

        <div class="hrs-promo">
            <div class="hrs-promo__content">
                <span class="badge badge-accent">Interactive Game</span>
                <h3>Can you spot every sign?</h3>
                <p>Explore a real road scene, find the road signs hidden inside it, then answer a question about each one you discover.</p>
                <a class="btn btn-primary" href="~/Learner/Game.aspx" runat="server">Start Road Sign Game</a>
            </div>

            <div class="hrs-promo__scene" aria-hidden="true">
                <svg viewBox="0 0 320 200" preserveAspectRatio="xMidYMid slice" focusable="false">
                    <rect width="320" height="200" fill="#DBEAFE" />
                    <circle cx="262" cy="42" r="20" fill="#FDE68A" />
                    <path d="M0 120 L70 80 L130 112 L200 70 L320 125 V200 H0 Z" fill="#BFDBFE" />
                    <rect y="130" width="320" height="70" fill="#94A3B8" />
                    <path d="M0 170 H320" stroke="#F8FAFC" stroke-width="3" stroke-dasharray="22 16" />
                    <rect x="38" y="104" width="6" height="40" fill="#64748B" />
                    <circle cx="41" cy="96" r="16" fill="#16A34A" />
                    <rect x="248" y="86" width="5" height="50" fill="#64748B" />
                    <path d="M250 60 L270 82 H230 Z" fill="#F59E0B" stroke="#B45309" stroke-width="2" stroke-linejoin="round" />
                    <rect x="150" y="100" width="5" height="34" fill="#64748B" />
                    <circle cx="152" cy="92" r="14" fill="#DC2626" />
                    <rect x="144" y="90" width="16" height="4" rx="1" fill="#FFFFFF" />
                    <circle cx="152" cy="92" r="23" fill="none" stroke="#FACC15" stroke-width="3" stroke-dasharray="5 4" />
                </svg>
                <asp:Panel ID="pnlGameCount" runat="server" CssClass="hrs-promo__scene-tag" Visible="false"><asp:Literal ID="litGameCount" runat="server" /></asp:Panel>
            </div>
        </div>
    </div>

    <!-- MOCK EXAM -->
    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Mock Exam</h2>
        </div>

        <div class="exam-banner">
            <div class="exam-banner__text">
                <h3>Ready to test yourself?</h3>
                <p>Take a timed mock examination covering the Colour Blind, Road Signs, Rules of the Road and KEJARA &amp; Safety sections.</p>
                <div class="exam-banner__chips">
                    <span class="exam-banner__chip">KPP Standard: 58 Questions &middot; 45 Minutes</span>
                    <span class="exam-banner__chip">Combined: 78 Questions &middot; 75 Minutes</span>
                </div>
            </div>
            <a class="btn exam-banner__btn" href="~/Learner/MockExam.aspx" runat="server">Start Mock Exam</a>
        </div>
    </div>

    </div>

    <!-- ================= RECENT ACTIVITY ================= -->
    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Recent Activity</h2>
        </div>

        <div class="empty-state empty-state--white empty-state--activity">
            <span class="dash-icon dash-icon--lg"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg></span>
            <div class="empty-state__title">No recent activity yet.</div>
            <p>Your lessons, practice sets and mock exams will show up here.</p>
        </div>
    </div>

</asp:Content>
