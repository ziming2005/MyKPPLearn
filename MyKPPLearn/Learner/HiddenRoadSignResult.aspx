<%@ Page Title="" Language="C#" MasterPageFile="~/Learner.Master" AutoEventWireup="true" CodeBehind="HiddenRoadSignResult.aspx.cs" Inherits="MyKPPLearn.Learner.HiddenRoadSignResult" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Road Sign Game Result | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <section class="card practice-result hrs-result">
        <h2 class="hrs-result__heading">Road Sign Game Complete</h2>
        <p class="hrs-result__set"><asp:Literal ID="litSetName" runat="server" /></p>

        <div class="practice-result__score">
            <div class="practice-result__score-value"><asp:Literal ID="litScore" runat="server" /></div>
            <div class="practice-result__score-pct"><asp:Literal ID="litPercent" runat="server" /></div>
        </div>

        <p class="practice-result__message"><asp:Literal ID="litMessage" runat="server" /></p>
        <p class="hrs-result__best"><asp:Literal ID="litBest" runat="server" /></p>

        <div class="form-actions">
            <asp:HyperLink ID="lnkNextSet" runat="server" CssClass="btn btn-primary" Visible="false">Next Set</asp:HyperLink>
            <asp:HyperLink ID="lnkReview" runat="server" CssClass="btn btn-secondary">Review Answers</asp:HyperLink>
            <asp:HyperLink ID="lnkTryAgain" runat="server" CssClass="btn btn-secondary">Try Again</asp:HyperLink>
            <a class="btn btn-secondary" href="~/Learner/Game.aspx" runat="server">Back to Game</a>
        </div>
    </section>

</asp:Content>
