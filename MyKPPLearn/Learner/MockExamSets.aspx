<%@ Page Title="" Language="C#" MasterPageFile="~/Learner.Master" AutoEventWireup="true" CodeBehind="MockExamSets.aspx.cs" Inherits="MyKPPLearn.Learner.MockExamSets" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Mock Exam Sets | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <nav class="breadcrumb" aria-label="Breadcrumb">
        <a href="~/Learner/MockExam.aspx" runat="server">Mock Exam</a>
        <span aria-hidden="true">/</span>
        <span aria-current="page"><asp:Literal ID="litTypeName" runat="server" /></span>
    </nav>

    <div class="dashboard-greeting">
        <h2><asp:Literal ID="litHeading" runat="server" /></h2>
        <p><asp:Literal ID="litSubtitle" runat="server" /></p>
    </div>

    <div id="mockExamSetsContainer" class="card-grid mx-sets-grid" data-type='<%= TypeId %>' data-app-root='<%= ResolveUrl("~/") %>' aria-live="polite"></div>

    <div class="form-actions mx-sets-actions">
        <a class="btn btn-secondary" href="~/Learner/MockExam.aspx" runat="server">&larr; Back to Mock Exam</a>
    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/mock-exam-demo.js") + "?v=" + MockExamScriptVersion %>"></script>
</asp:Content>
