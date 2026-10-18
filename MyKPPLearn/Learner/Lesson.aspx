<%@ Page Title="" Language="C#" MasterPageFile="~/Learner.Master" AutoEventWireup="true" CodeBehind="Lesson.aspx.cs" Inherits="MyKPPLearn.Learner.Lesson" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Lesson | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <a id="lnkBackToChapter" runat="server" class="lesson-back-link">&lsaquo; Back to Chapter</a>

    <article class="card lesson-article">
        <div class="lesson-article__head">
            <span class="badge">Lesson <asp:Literal ID="litLessonNumber" runat="server" /></span>
            <asp:Literal ID="litStatus" runat="server" />
        </div>
        <h2><asp:Literal ID="litTitle" runat="server" /></h2>

        <asp:Panel ID="pnlImageAbove" runat="server" Visible="false" CssClass="lesson-article__image">
            <img id="imgLessonAbove" runat="server" class="lesson-image" alt="" />
        </asp:Panel>

        <div class="lesson-content">
            <asp:Literal ID="litContent" runat="server" />
        </div>

        <asp:Panel ID="pnlImageBelow" runat="server" Visible="false" CssClass="lesson-article__image">
            <img id="imgLessonBelow" runat="server" class="lesson-image" alt="" />
        </asp:Panel>
    </article>

    <%-- Anchor target after "Complete Lesson": the page returns here, so the learner stays at the buttons and sees the message. --%>
    <div id="lessonActions" class="lesson-actions">
        <asp:Label ID="lblMessage" runat="server" Visible="false" EnableViewState="false" role="status" />

        <div class="lesson-nav">
            <div class="lesson-nav__side">
                <a id="lnkPrevious" runat="server" class="btn btn-secondary lesson-nav-btn" visible="false"></a>
            </div>

            <div class="lesson-nav__center">
                <asp:Button ID="btnComplete" runat="server" Text="Complete Lesson" CssClass="btn btn-primary" OnClick="btnComplete_Click" />
            </div>

            <div class="lesson-nav__side lesson-nav__side--end">
                <a id="lnkNext" runat="server" class="btn btn-secondary lesson-nav-btn lesson-nav-btn--next" visible="false"></a>
            </div>
        </div>
    </div>


</asp:Content>
