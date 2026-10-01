<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="MyKPPLearn.Register" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Create Learner Account | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <section class="section auth-section">
        <div class="container">
            <div class="card auth-card">
                <div class="auth-card__header">
                    <h1>Create Account</h1>
                    <p>Start learning Malaysian driving theory, practise by topic, and prepare for mock exams.</p>
                </div>

                <asp:ValidationSummary ID="valSummary" runat="server" CssClass="validation-summary" HeaderText="Please fix the following:" DisplayMode="BulletList" />

                <asp:Label ID="lblFormError" runat="server" CssClass="alert alert-error" Visible="false" EnableViewState="false" />

                <asp:Panel ID="pnlForm" runat="server" DefaultButton="btnRegister">
                    <div class="form-group">
                        <asp:Label ID="lblFullName" runat="server" AssociatedControlID="txtFullName" CssClass="form-label">Full Name</asp:Label>
                        <asp:TextBox ID="txtFullName" runat="server" CssClass="form-input" autocomplete="name" MaxLength="150" />
                        <asp:RequiredFieldValidator ID="rfvFullName" runat="server" ControlToValidate="txtFullName" CssClass="field-error" Display="Dynamic" ErrorMessage="Full name is required." SetFocusOnError="true" />
                    </div>

                    <div class="form-group">
                        <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" CssClass="form-label">Email</asp:Label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-input" TextMode="Email" autocomplete="email" MaxLength="254" />
                        <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" CssClass="field-error" Display="Dynamic" ErrorMessage="Email is required." SetFocusOnError="true" />
                        <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" CssClass="field-error" Display="Dynamic" ErrorMessage="Enter a valid email address." ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" />
                    </div>

                    <div class="form-group">
                        <asp:Label ID="lblPassword" runat="server" AssociatedControlID="txtPassword" CssClass="form-label">Password</asp:Label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-input" TextMode="Password" autocomplete="new-password" MaxLength="100" />
                        <small class="form-hint">At least 8 characters, including an uppercase letter, a lowercase letter, and a number.</small>
                        <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" CssClass="field-error" Display="Dynamic" ErrorMessage="Password is required." SetFocusOnError="true" />
                        <asp:RegularExpressionValidator ID="revPassword" runat="server" ControlToValidate="txtPassword" CssClass="field-error" Display="Dynamic" ErrorMessage="Password must be at least 8 characters and include an uppercase letter, lowercase letter, and number." ValidationExpression="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$" />
                    </div>

                    <div class="form-group">
                        <asp:Label ID="lblConfirmPassword" runat="server" AssociatedControlID="txtConfirmPassword" CssClass="form-label">Confirm Password</asp:Label>
                        <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-input" TextMode="Password" autocomplete="new-password" MaxLength="100" />
                        <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword" CssClass="field-error" Display="Dynamic" ErrorMessage="Please confirm your password." SetFocusOnError="true" />
                        <asp:CompareValidator ID="cvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword" ControlToCompare="txtPassword" CssClass="field-error" Display="Dynamic" ErrorMessage="Passwords do not match." />
                    </div>

                    <asp:Button ID="btnRegister" runat="server" CssClass="btn btn-primary btn-block" Text="Create Account" OnClick="btnRegister_Click" CausesValidation="true" />
                </asp:Panel>

                <div class="auth-card__footer">
                    Already have an account? <a href="Login.aspx">Login</a>
                </div>
            </div>
        </div>
    </section>
</asp:Content>
