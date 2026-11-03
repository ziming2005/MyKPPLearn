<%@ Page Title="" Language="C#" MasterPageFile="~/Learner.Master" AutoEventWireup="true" CodeBehind="Game.aspx.cs" Inherits="MyKPPLearn.Learner.Game" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Road Sign Game | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="dashboard-greeting">
        <h2>Road Sign Game</h2>
        <p>Find hidden road signs in a driving scene and test your road-sign knowledge.</p>
    </div>


    <asp:Label ID="lblError" runat="server" Visible="false" CssClass="alert alert-error" role="alert" />

    <asp:Panel ID="pnlEmpty" runat="server" CssClass="empty-state" Visible="false">
        <div class="empty-state__title">No Road Sign Game sets are currently available.</div>
    </asp:Panel>

    <div class="card-grid mx-sets-grid">
        <asp:Repeater ID="rptSets" runat="server">
            <ItemTemplate>
                <div class="card mx-type-card mx-set-card">
                    <h3><%# "Set " + Eval("SetNumber") %></h3>
                    <p class="mx-type-card__subtitle"><%# SignCountText(Eval("ConfiguredCount")) %></p>
                    <a class="btn btn-primary" href='<%# ResolveUrl("~/Learner/HiddenRoadSignGame.aspx?set=" + Eval("HiddenRoadSignPracticeId")) %>'>Start Game</a>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

</asp:Content>
