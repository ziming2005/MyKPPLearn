<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="MyKPPLearn.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Login | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <section class="section auth-section">
        <div class="container">
            <div class="card auth-card">
                <div class="auth-card__header">
                    <span class="auth-card__icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="5" y="11" width="14" height="10" rx="2" /><path d="M8 11V8a4 4 0 0 1 8 0v3" /></svg></span>
                    <h1>Welcome Back</h1>
                    <p>Login to continue learning.</p>
                </div>

                <asp:Label ID="lblFormError" runat="server" CssClass="alert alert-error" Visible="false" EnableViewState="false" />

                <asp:Panel ID="pnlForm" runat="server" DefaultButton="btnLogin">
                    <div class="form-group">
                        <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" CssClass="form-label">Email</asp:Label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-input" TextMode="Email" autocomplete="email" MaxLength="254" />
                        <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" CssClass="field-error" Display="Dynamic" ErrorMessage="Email is required." SetFocusOnError="true" />
                        <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" CssClass="field-error" Display="Dynamic" ErrorMessage="Enter a valid email address." ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" />
                    </div>

                    <div class="form-group">
                        <asp:Label ID="lblPassword" runat="server" AssociatedControlID="txtPassword" CssClass="form-label">Password</asp:Label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-input" TextMode="Password" autocomplete="current-password" MaxLength="100" />
                        <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" CssClass="field-error" Display="Dynamic" ErrorMessage="Password is required." SetFocusOnError="true" />
                    </div>

                    <asp:Button ID="btnLogin" runat="server" CssClass="btn btn-primary btn-block" Text="Login" OnClick="btnLogin_Click" CausesValidation="true" />
                </asp:Panel>

                <div class="auth-card__footer">
                    New here? <a href="Register.aspx">Create Account</a>
                </div>
            </div>
        </div>
    </section>
</asp:Content>
