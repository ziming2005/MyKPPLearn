<%@ Page Title="" Language="C#" MasterPageFile="~/Learner.Master" AutoEventWireup="true" CodeBehind="Learn.aspx.cs" Inherits="MyKPPLearn.Learner.Learn" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Learning | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="dashboard-greeting">
        <h2>Learning</h2>
        <p>Work through each chapter to prepare for your driving theory test.</p>
    </div>

    <asp:Label ID="lblError" runat="server" CssClass="alert alert-error" Visible="false" />

    <asp:Panel ID="pnlSummary" runat="server" CssClass="card learn-summary" Visible="false">
        <div class="learn-summary__main">
            <div class="learn-summary__label">Your progress</div>
            <div class="learn-summary__value"><asp:Literal ID="litSummaryCompleted" runat="server" /> of <asp:Literal ID="litSummaryTotal" runat="server" /> lessons completed &middot; <asp:Literal ID="litSummaryPercent" runat="server" />%</div>
            <div class="progress-track" role="progressbar" aria-valuemin="0" aria-valuemax="100" aria-label="Overall learning progress">
                <div id="summaryFill" runat="server" class="progress-track__fill"></div>
            </div>
        </div>
        <a id="lnkContinue" runat="server" class="btn btn-primary learn-summary__btn" visible="false">Continue Learning</a>
        <asp:Panel ID="pnlAllDone" runat="server" CssClass="learn-summary__done" Visible="false">All lessons completed</asp:Panel>
    </asp:Panel>

    <asp:Panel ID="pnlEmpty" runat="server" CssClass="empty-state" Visible="false">
        <div class="empty-state__title">No learning content is available yet.</div>
    </asp:Panel>

    <div class="chapter-grid">
        <asp:Repeater ID="rptChapters" runat="server">
            <ItemTemplate>
                <div class='<%# "card chapter-card chapter-card--" + ChapterState(Eval("CompletedCount"), Eval("LessonCount")) %>'>
                    <div class="chapter-card__top">
                        <span class="dash-icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20V3H6.5A2.5 2.5 0 0 0 4 5.5v14z"/><path d="M20 17v4H6.5A2.5 2.5 0 0 1 4 19.5"/></svg></span>
                        <span class="badge">Chapter <%# Eval("DisplayOrder") %></span>
                    </div>
                    <h3><%# Server.HtmlEncode(Convert.ToString(Eval("Title"))) %></h3>
                    <p><%# Server.HtmlEncode(Convert.ToString(Eval("Description"))) %></p>

                    <div class="chapter-card__meta">
                        <div class="progress-row">
                            <span><%# ChapterProgressText(Eval("CompletedCount"), Eval("LessonCount")) %></span>
                            <%# ChapterState(Eval("CompletedCount"), Eval("LessonCount")) == "done" ? "<span class=\"chapter-card__done\">&#10003; Completed</span>" : "" %>
                        </div>
                        <div class="progress-track" role="progressbar" aria-valuemin="0" aria-valuemax="100" aria-valuenow='<%# ChapterPercent(Eval("CompletedCount"), Eval("LessonCount")) %>' aria-label="Chapter progress">
                            <div class="progress-track__fill" style='width: <%# ChapterPercent(Eval("CompletedCount"), Eval("LessonCount")) %>%;'></div>
                        </div>
                    </div>

                    <%# ChapterState(Eval("CompletedCount"), Eval("LessonCount")) == "empty"
                        ? "<span class=\"btn btn-secondary chapter-card__soon\" aria-disabled=\"true\">Coming soon</span>"
                        : "<a class=\"btn " + (ChapterState(Eval("CompletedCount"), Eval("LessonCount")) == "done" ? "btn-secondary" : "btn-primary") + " btn-block chapter-card__link\" href=\"" + ResolveUrl("~/Learner/Chapter.aspx?id=" + Eval("ChapterId")) + "\">" + ChapterButtonText(Eval("CompletedCount"), Eval("LessonCount")) + "</a>" %>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

</asp:Content>
