<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProfileEditor.ascx.cs" Inherits="MyKPPLearn.Controls.ProfileEditor" %>

<asp:Label ID="lblMessage" runat="server" Visible="false" EnableViewState="false" role="status" />

<div class="profile-editor" data-has-photo="<%= HasPhoto ? "1" : "0" %>">

    <!-- ================= PERSONAL INFORMATION ================= -->
    <section class="card profile-card" aria-labelledby="profilePersonalTitle">
        <h2 id="profilePersonalTitle">Personal Information</h2>

        <div class="profile-identity">
            <%-- The avatar is the upload trigger: a label tied to the (visually hidden) file input, also focusable and operable by keyboard. --%>
            <label class="profile-avatar" for="<%= fuPhoto.ClientID %>" tabindex="0" role="button" aria-label="Change profile picture" title="Change profile picture">
                <span class="profile-avatar__circle">
                    <img id="imgProfile" runat="server" class="profile-avatar__img" alt="" />
                    <span class="profile-avatar__initials"><asp:Literal ID="litInitials" runat="server" /></span>
                </span>
                <span class="profile-avatar__badge" aria-hidden="true">
                    <svg viewBox="0 0 24 24" width="15" height="15" focusable="false"><path fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" d="M4 8h3l2-2.5h6L17 8h3a1 1 0 0 1 1 1v9a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V9a1 1 0 0 1 1-1z"/><circle cx="12" cy="13" r="3.5" fill="none" stroke="currentColor" stroke-width="2"/></svg>
                </span>
            </label>
            <div class="profile-identity__text">
                <div class="profile-identity__name"><asp:Literal ID="litHeadName" runat="server" /></div>
                <%-- Choosing a picture uploads it at once: profile.js validates it, then clicks this (hidden) button, which posts only the picture flow. --%>
                <div class="profile-photo-controls">
                    <asp:FileUpload ID="fuPhoto" runat="server" CssClass="profile-photo-input" accept=".jpg,.jpeg,.png,.webp" />
                    <asp:Button ID="btnUploadPhoto" runat="server" CssClass="profile-upload-trigger" CausesValidation="false" TabIndex="-1" Text="Upload" OnClick="btnUploadPhoto_Click" />
                </div>
                <p class="profile-photo-hint">Click or tap the picture to change it. JPG, PNG or WebP, up to 2&nbsp;MB.</p>
                <asp:Button ID="btnRemovePhoto" runat="server" CssClass="profile-photo-remove" CausesValidation="false" Text="Remove photo" OnClick="btnRemovePhoto_Click" data-confirm="Your profile picture will be removed and your initials shown instead." data-confirm-title="Remove profile picture?" data-confirm-label="Remove" />
                <span class="profile-photo-status" role="status" hidden>Uploading&hellip;</span>
                <span class="field-error profile-photo-error" role="alert" hidden></span>
            </div>
        </div>

        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtFullName" CssClass="form-label">Full Name</asp:Label>
            <asp:TextBox ID="txtFullName" runat="server" CssClass="form-input" MaxLength="150" autocomplete="name" />
            <asp:RequiredFieldValidator ID="rfvFullName" runat="server" ControlToValidate="txtFullName" CssClass="field-error" Display="Dynamic" ErrorMessage="Full name is required." ValidationGroup="ProfileForm" />
            <asp:RegularExpressionValidator ID="revFullName" runat="server" ControlToValidate="txtFullName" CssClass="field-error" Display="Dynamic" ValidationExpression="^[^&lt;&gt;]*$" ErrorMessage="Full name cannot contain &lt; or &gt;." ValidationGroup="ProfileForm" />
        </div>

        <div class="form-group">
            <span class="form-label">Email</span>
            <div class="profile-readonly"><asp:Literal ID="litEmail" runat="server" /></div>        </div>

        <div class="form-group">
            <span class="form-label">Account Created</span>
            <div class="profile-readonly"><asp:Literal ID="litCreated" runat="server" /></div>
        </div>

        <div class="form-actions">
            <asp:Button ID="btnSaveProfile" runat="server" Text="Save Changes" CssClass="btn btn-primary" ValidationGroup="ProfileForm" OnClick="btnSaveProfile_Click" />
            <button type="button" id="profileOpenPassword" class="btn btn-secondary profile-open-password" aria-haspopup="dialog">Change Password</button>
        </div>
    </section>

    <!-- ================= ACTIVITY (learners only) ================= -->
    <asp:Panel ID="pnlActivity" runat="server" CssClass="card profile-card" Visible="false">
        <h2>Your Activity</h2>
        <div class="profile-activity">
            <div class="profile-activity__item">
                <strong><asp:Literal ID="litActivityLessons" runat="server" /></strong>
                <span>Lessons completed</span>
            </div>
            <div class="profile-activity__item">
                <strong><asp:Literal ID="litActivityGames" runat="server" /></strong>
                <span>Road Sign Games completed</span>
            </div>
        </div>
    </asp:Panel>

    <!-- ================= CHANGE PASSWORD (modal; hidden until the button is clicked or a failed attempt reopens it) ================= -->
    <div id="pnlPasswordModal" runat="server" class="modal-overlay profile-password-modal" hidden>
        <div class="modal-panel card profile-modal" role="dialog" aria-modal="true" aria-labelledby="profilePasswordTitle">
            <div class="profile-modal__head">
                <h3 id="profilePasswordTitle">Change Password</h3>
                <button type="button" class="profile-modal__close" aria-label="Close">&times;</button>
            </div>

            <asp:Label ID="lblPasswordMessage" runat="server" Visible="false" EnableViewState="false" role="alert" />

            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtCurrentPassword" CssClass="form-label">Current Password</asp:Label>
                <asp:TextBox ID="txtCurrentPassword" runat="server" CssClass="form-input" TextMode="Password" autocomplete="current-password" MaxLength="100" />
                <asp:RequiredFieldValidator ID="rfvCurrent" runat="server" ControlToValidate="txtCurrentPassword" CssClass="field-error" Display="Dynamic" ErrorMessage="Please enter your current password." ValidationGroup="PasswordForm" />
            </div>

            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtNewPassword" CssClass="form-label">New Password</asp:Label>
                <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-input" TextMode="Password" autocomplete="new-password" MaxLength="100" />
                <small class="form-hint">At least 8 characters, including an uppercase letter, a lowercase letter, and a number.</small>
                <asp:RequiredFieldValidator ID="rfvNew" runat="server" ControlToValidate="txtNewPassword" CssClass="field-error" Display="Dynamic" ErrorMessage="Please enter a new password." ValidationGroup="PasswordForm" />
                <asp:RegularExpressionValidator ID="revNew" runat="server" ControlToValidate="txtNewPassword" CssClass="field-error" Display="Dynamic" ValidationGroup="PasswordForm" /> <%-- rule and message come from Security/PasswordPolicy.cs (set in ProfileEditor.ascx.cs) --%>
            </div>

            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtConfirmPassword" CssClass="form-label">Confirm New Password</asp:Label>
                <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-input" TextMode="Password" autocomplete="new-password" MaxLength="100" />
                <asp:RequiredFieldValidator ID="rfvConfirm" runat="server" ControlToValidate="txtConfirmPassword" CssClass="field-error" Display="Dynamic" ErrorMessage="Please confirm your new password." ValidationGroup="PasswordForm" />
                <asp:CompareValidator ID="cvConfirm" runat="server" ControlToValidate="txtConfirmPassword" ControlToCompare="txtNewPassword" CssClass="field-error" Display="Dynamic" ErrorMessage="Passwords do not match." ValidationGroup="PasswordForm" />
            </div>

            <div class="form-actions profile-modal__actions">
                <button type="button" class="btn btn-secondary profile-modal__cancel">Cancel</button>
                <asp:Button ID="btnChangePassword" runat="server" Text="Change Password" CssClass="btn btn-primary" ValidationGroup="PasswordForm" OnClick="btnChangePassword_Click" />
            </div>
        </div>
    </div>
</div>

<script src="<%= ResolveUrl("~/Scripts/profile.js") %>?v=20261018"></script>
