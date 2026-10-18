<%@ Page Title="" Language="C#" MasterPageFile="~/Learner.Master" AutoEventWireup="true" CodeBehind="Progress.aspx.cs" Inherits="MyKPPLearn.Learner.Progress" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Progress | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="dashboard-greeting">
        <h2>Progress</h2>
        <p>Track your learning and completed activities.</p>
    </div>

    <asp:Label ID="lblError" runat="server" Visible="false" CssClass="alert alert-error" role="alert" />

    <!-- ================= OVERVIEW ================= -->
    <section class="panel-section" aria-labelledby="progOverviewTitle">
        <div class="panel-section__header"><h2 id="progOverviewTitle">Overview</h2></div>

        <div class="stat-grid progress-stats">
            <div class="stat-card stat-card--blue">
                <span class="dash-icon"><svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20V3H6.5A2.5 2.5 0 0 0 4 5.5v14z"/><path d="M20 17v4H6.5A2.5 2.5 0 0 1 4 19.5"/></svg></span>
                <div class="stat-card__label">Lessons Completed</div>
                <div class="stat-card__value"><%= OverallCompleted %> / <%= OverallTotal %></div>
            </div>
            <%-- Practice and Mock Exam attempts are not stored yet, so these show a dash (not 0) until they are tracked. --%>
            <div class="stat-card stat-card--green">
                <span class="dash-icon"><svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><rect x="8" y="2" width="8" height="4" rx="1"/><path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2"/><path d="M9 14l2 2 4-4"/></svg></span>
                <div class="stat-card__label">Practice Attempts</div>
                <div class="stat-card__value" title="Not tracked yet">&mdash;</div>
            </div>
            <div class="stat-card stat-card--amber">
                <span class="dash-icon"><svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><circle cx="12" cy="13" r="8"/><path d="M12 9v4l2.5 2.5"/><path d="M9 2h6"/></svg></span>
                <div class="stat-card__label">Mock Exams Completed</div>
                <div class="stat-card__value" title="Not tracked yet">&mdash;</div>
            </div>
            <div class="stat-card stat-card--purple">
                <span class="dash-icon"><svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M6 12h4M8 10v4"/><path d="M15 13h.01M18 11h.01"/><path d="M17.3 5H6.7a4 4 0 0 0-3.9 3.2l-1.5 7A3 3 0 0 0 4.2 19a3 3 0 0 0 2.4-1.2l1.4-2h8l1.4 2a3 3 0 0 0 2.4 1.2 3 3 0 0 0 2.9-3.8l-1.5-7A4 4 0 0 0 17.3 5z"/></svg></span>
                <div class="stat-card__label">Games Completed</div>
                <div class="stat-card__value"><%= GamesCompleted %></div>
            </div>
        </div>
    </section>

    <!-- ================= LEARNING PROGRESS ================= -->
    <section class="panel-section" aria-labelledby="progLearningTitle">
        <div class="panel-section__header"><h2 id="progLearningTitle">Learning Progress</h2></div>

        <div class="card progress-card">
            <div class="progress-block">
                <div class="progress-block__head">
                    <span class="progress-block__title">Overall Learning Progress</span>
                    <span class="progress-block__percent"><%= OverallPercent %>%</span>
                </div>
                <div class="progress-block__meta"><%= OverallCompleted %> / <%= OverallTotal %> Lessons Completed</div>
                <div class="progress-track" role="progressbar" aria-valuemin="0" aria-valuemax="100" aria-valuenow="<%= OverallPercent %>" aria-label="Overall learning progress">
                    <div class="progress-track__fill" style="width: <%= OverallPercent %>%;"></div>
                </div>
            </div>

            <asp:Panel ID="pnlNoChapters" runat="server" CssClass="progress-note" Visible="false">No learning content is available yet.</asp:Panel>

            <asp:Repeater ID="rptChapters" runat="server">
                <ItemTemplate>
                    <div class='<%# "progress-block progress-block--chapter progress-block--" + ChapterState(Eval("CompletedCount"), Eval("LessonCount")) %>'>
                        <div class="progress-block__head">
                            <a class="progress-block__title progress-block__link" href='<%# ResolveUrl("~/Learner/Chapter.aspx?id=" + Eval("ChapterId")) %>'>Chapter <%# Eval("DisplayOrder") %> &mdash; <%# Server.HtmlEncode(Convert.ToString(Eval("Title"))) %></a>
                            <span class="progress-block__percent"><%# ChapterState(Eval("CompletedCount"), Eval("LessonCount")) == "done" ? "&#10003; " : "" %><%# ChapterPercent(Eval("CompletedCount"), Eval("LessonCount")) %>%</span>
                        </div>
                        <div class="progress-block__meta"><%# ChapterState(Eval("CompletedCount"), Eval("LessonCount")) == "empty" ? "No lessons yet" : (Eval("CompletedCount") + " / " + Eval("LessonCount") + " Lessons Completed") %></div>
                        <div class="progress-track" role="progressbar" aria-valuemin="0" aria-valuemax="100" aria-valuenow="<%# ChapterPercent(Eval("CompletedCount"), Eval("LessonCount")) %>" aria-label="Chapter progress">
                            <div class="progress-track__fill" style="width: <%# ChapterPercent(Eval("CompletedCount"), Eval("LessonCount")) %>%;"></div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>

            <a id="lnkContinue" runat="server" class="btn btn-primary progress-card__continue" visible="false">Continue Learning</a>
        </div>
    </section>

    <!-- ================= HIDDEN ROAD SIGN GAME HISTORY ================= -->
    <section class="panel-section" aria-labelledby="progGameTitle">
        <div class="panel-section__header"><h2 id="progGameTitle">Road Sign Game History</h2></div>

        <div class="card progress-card">
            <asp:Panel ID="pnlNoGames" runat="server" CssClass="progress-note" Visible="false">
                No completed Road Sign Games yet.
                <div class="progress-note__action"><a class="btn btn-primary btn-small" href="~/Learner/Game.aspx" runat="server">Play Road Sign Game</a></div>
            </asp:Panel>

            <asp:Panel ID="pnlGameSummary" runat="server" CssClass="history-summary" Visible="false"><asp:Literal ID="litGameSummary" runat="server" /></asp:Panel>

            <ul id="gameHistoryList" class="history-list">
                <asp:Repeater ID="rptGames" runat="server">
                    <ItemTemplate>
                        <li class="history-item<%# Container.ItemIndex >= RecentGames ? " history-item--extra" : "" %>">
                            <div class="history-item__main">
                                <div class="history-item__title"><%# MyKPPLearn.Data.HiddenRoadSignPracticeRepository.FormatSetName(Eval("SetNumber")) %></div>
                                <div class="history-item__score"><%# Eval("CorrectCount") %> / <%# Eval("TotalItems") %> Questions Correct</div>
                                <div class="history-item__date"><%# FormatDate(Eval("CompletedAt")) %></div>
                            </div>
                            <a class="btn btn-secondary btn-small btn-result" href='<%# ResolveUrl("~/Learner/HiddenRoadSignResult.aspx?attempt=" + Eval("HiddenRoadSignAttemptId")) %>'>View Result</a>
                        </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>

            <%-- Older games are in the page but hidden; this button expands / collapses them in place (no reload). --%>
            <asp:Panel ID="pnlGameToggle" runat="server" CssClass="history-more" Visible="false">
                <button type="button" id="gameHistoryToggle" class="btn btn-secondary" aria-expanded="false" aria-controls="gameHistoryList" data-more="View All" data-less="Show Less">View All</button>
            </asp:Panel>
        </div>
    </section>

    <!-- ================= PRACTICE + MOCK EXAM (placeholder until attempts are stored) ================= -->
    <section class="panel-section" aria-labelledby="progPracticeTitle">
        <div class="panel-section__header"><h2 id="progPracticeTitle">Practice and Mock Exam</h2></div>
        <div class="card progress-card">
            <p class="progress-note">Your practice and mock exam results will appear here once they&rsquo;re available.</p>
        </div>
    </section>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script>
        (function () {
            var button = document.getElementById("gameHistoryToggle");
            var list = document.getElementById("gameHistoryList");

            if (!button || !list) { return; }

            button.addEventListener("click", function () {
                var expanded = list.classList.toggle("is-expanded");
                button.setAttribute("aria-expanded", expanded ? "true" : "false");
                button.textContent = expanded ? button.getAttribute("data-less") : button.getAttribute("data-more");
            });
        })();
    </script>
</asp:Content>
