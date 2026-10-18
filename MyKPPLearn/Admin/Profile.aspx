<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="MyKPPLearn.Admin.Profile" %>
<%@ Register Src="~/Controls/ProfileEditor.ascx" TagPrefix="uc" TagName="ProfileEditor" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Profile | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="dashboard-greeting">
        <h2>Profile</h2>
        <p>Manage your personal information and password.</p>
    </div>

    <uc:ProfileEditor ID="ProfileEditor1" runat="server" />
</asp:Content>
