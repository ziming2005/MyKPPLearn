<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="MyKPPLearn.ForgotPassword" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Forgot Password | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <section class="section auth-section">
        <div class="container">
            <div class="card auth-card">
                <div class="auth-card__header">
                    <span class="auth-card__icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="5" width="18" height="14" rx="2" /><path d="m3 7 9 6 9-6" /></svg></span>
                    <h1>Forgot Password</h1>
                    <asp:Literal ID="litIntro" runat="server" />
                </div>

                <asp:Label ID="lblMessage" runat="server" Visible="false" EnableViewState="false" role="status" />

                <%-- Step 1: ask for the email --%>
                <asp:Panel ID="pnlEmail" runat="server" DefaultButton="btnSend">
                    <div class="form-group">
                        <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" CssClass="form-label">Email</asp:Label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-input" TextMode="Email" autocomplete="email" MaxLength="254" />
                        <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" CssClass="field-error" Display="Dynamic" ErrorMessage="Email is required." SetFocusOnError="true" ValidationGroup="Email" />
                        <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" CssClass="field-error" Display="Dynamic" ErrorMessage="Enter a valid email address." ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" ValidationGroup="Email" />
                    </div>

                    <asp:Button ID="btnSend" runat="server" CssClass="btn btn-primary btn-block" Text="Send Code" OnClick="btnSend_Click" ValidationGroup="Email" />
                </asp:Panel>

                <%-- Step 2: the emailed code --%>
                <asp:Panel ID="pnlCode" runat="server" DefaultButton="btnVerify" Visible="false">
                    <div class="form-group">
                        <asp:Label ID="lblCode" runat="server" AssociatedControlID="txtCode" CssClass="form-label">6-Digit Code</asp:Label>
                        <asp:TextBox ID="txtCode" runat="server" CssClass="form-input otp-input" MaxLength="6" autocomplete="one-time-code" inputmode="numeric" pattern="[0-9]{6}" />
                        <asp:RequiredFieldValidator ID="rfvCode" runat="server" ControlToValidate="txtCode" CssClass="field-error" Display="Dynamic" ErrorMessage="Enter the code from the email." SetFocusOnError="true" ValidationGroup="Code" />
                        <asp:RegularExpressionValidator ID="revCode" runat="server" ControlToValidate="txtCode" CssClass="field-error" Display="Dynamic" ErrorMessage="The code is 6 digits." ValidationExpression="^\d{6}$" ValidationGroup="Code" />
                    </div>

                    <asp:Button ID="btnVerify" runat="server" CssClass="btn btn-primary btn-block" Text="Verify Code" OnClick="btnVerify_Click" ValidationGroup="Code" />

                    <div class="otp-actions">
                        <asp:LinkButton ID="btnResend" runat="server" OnClick="btnResend_Click" CausesValidation="false">Resend code</asp:LinkButton>
                        <asp:LinkButton ID="btnChangeEmail" runat="server" OnClick="btnChangeEmail_Click" CausesValidation="false">Use a different email</asp:LinkButton>
                    </div>
                </asp:Panel>

                <%-- Step 3: the new password, only after the code is verified --%>
                <asp:Panel ID="pnlPassword" runat="server" DefaultButton="btnReset" Visible="false">
                    <div class="form-group">
                        <asp:Label ID="lblPassword" runat="server" AssociatedControlID="txtPassword" CssClass="form-label">New Password</asp:Label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-input" TextMode="Password" autocomplete="new-password" MaxLength="100" />
                        <small class="form-hint">At least 8 characters, including an uppercase letter, a lowercase letter, and a number.</small>
                        <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" CssClass="field-error" Display="Dynamic" ErrorMessage="Password is required." SetFocusOnError="true" ValidationGroup="Reset" />
                        <asp:RegularExpressionValidator ID="revPassword" runat="server" ControlToValidate="txtPassword" CssClass="field-error" Display="Dynamic" ValidationGroup="Reset" />
                    </div>

                    <div class="form-group">
                        <asp:Label ID="lblConfirmPassword" runat="server" AssociatedControlID="txtConfirmPassword" CssClass="form-label">Confirm New Password</asp:Label>
                        <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-input" TextMode="Password" autocomplete="new-password" MaxLength="100" />
                        <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword" CssClass="field-error" Display="Dynamic" ErrorMessage="Please confirm your password." SetFocusOnError="true" ValidationGroup="Reset" />
                        <asp:CompareValidator ID="cvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword" ControlToCompare="txtPassword" CssClass="field-error" Display="Dynamic" ErrorMessage="Passwords do not match." ValidationGroup="Reset" />
                    </div>

                    <asp:Button ID="btnReset" runat="server" CssClass="btn btn-primary btn-block" Text="Reset Password" OnClick="btnReset_Click" ValidationGroup="Reset" />
                </asp:Panel>

                <div class="auth-card__footer">
                    Remembered it? <a href="Login.aspx">Back to Login</a>
                </div>
            </div>
        </div>
    </section>
</asp:Content>
