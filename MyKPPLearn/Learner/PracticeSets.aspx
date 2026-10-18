<%@ Page Title="" Language="C#" MasterPageFile="~/Learner.Master" AutoEventWireup="true" CodeBehind="PracticeSets.aspx.cs" Inherits="MyKPPLearn.Learner.PracticeSets" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Practice Sets | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <nav class="breadcrumb" aria-label="Breadcrumb">
        <a href="~/Learner/Practice.aspx" runat="server">Practice</a>
        <span aria-hidden="true">/</span>
        <span aria-current="page"><asp:Literal ID="litCategoryTitle" runat="server" /></span>
    </nav>

    <div class="dashboard-greeting">
        <h2><asp:Literal ID="litCategoryHeading" runat="server" /></h2>
        <p>Choose a Practice Set to begin. Each set is a short, self-contained round of questions.</p>
    </div>

    <div id="practiceSetsContainer" class="card-grid card-grid--2" data-category='<%= CategoryId %>' aria-live="polite"></div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/practice-demo.js") + "?v=20261017-set-labels" %>"></script>
</asp:Content>
