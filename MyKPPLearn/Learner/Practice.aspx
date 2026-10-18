<%@ Page Title="" Language="C#" MasterPageFile="~/Learner.Master" AutoEventWireup="true" CodeBehind="Practice.aspx.cs" Inherits="MyKPPLearn.Learner.Practice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Practice | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="dashboard-greeting">
        <h2>Practice</h2>
        <p>Pick a topic and start a practice set. Go at your own pace.</p>
    </div>

    <div id="practiceCategoryGrid" class="card-grid card-grid--2" aria-live="polite"></div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/practice-demo.js") + "?v=20261018-sets2" %>"></script>
</asp:Content>
