<%@ Page Title="" Language="C#" MasterPageFile="~/Learner.Master" AutoEventWireup="true" CodeBehind="Chapter.aspx.cs" Inherits="MyKPPLearn.Learner.Chapter" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Chapter | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <nav class="breadcrumb" aria-label="Breadcrumb">
        <a href="~/Learner/Learn.aspx" runat="server">Learning</a>
        <span aria-hidden="true">/</span>
        <span aria-current="page">Chapter <asp:Literal ID="litCrumbChapter" runat="server" /></span>
    </nav>

    <div class="card chapter-header">
        <span class="badge">Chapter <asp:Literal ID="litChapterNumber" runat="server" /></span>
        <h2><asp:Literal ID="litTitle" runat="server" /></h2>
        <p><asp:Literal ID="litDescription" runat="server" /></p>
    </div>

    <div class="panel-section">
        <div class="panel-section__header">
            <h2>Lessons</h2>
        </div>

        <asp:Panel ID="pnlEmpty" runat="server" CssClass="empty-state" Visible="false">
            <div class="empty-state__title">No lessons are currently available in this chapter.</div>
        </asp:Panel>

        <div class="lesson-list">
            <asp:Repeater ID="rptLessons" runat="server">
                <ItemTemplate>
                    <%-- The whole card is the link to the lesson. --%>
                    <a class="card lesson-row lesson-row--link" href='<%# ResolveUrl("~/Learner/Lesson.aspx?id=" + Eval("LessonId")) %>'>
                        <div class="lesson-row__main">
                            <div class="lesson-row__number">Lesson <%# MyKPPLearn.Data.LessonRepository.FormatLessonNumber(Eval("ChapterOrder"), Eval("DisplayOrder")) %></div>
                            <div class="lesson-row__title"><%# Server.HtmlEncode(Convert.ToString(Eval("Title"))) %></div>
                        </div>
                        <span class='<%# (bool)Eval("IsCompleted") ? "status-badge status-badge--active" : "status-badge status-badge--inactive" %>'><%# (bool)Eval("IsCompleted") ? "Completed" : "Not Completed" %></span>
                    </a>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </div>

</asp:Content>
