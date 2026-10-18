using System;
using System.Globalization;
using System.Web;
using System.Web.UI;
using MyKPPLearn.Common;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Controls
{
    /// <summary>
    /// The Profile form shared by Learner/Profile.aspx and Admin/Profile.aspx (each page keeps its own master page and
    /// does its own role authorization before this control loads). It always works on the signed-in user taken from
    /// Session, never on an id from the request, so a user can only ever edit their own account.
    ///   Personal Information: Full Name (Save Changes) and the profile picture (saved as soon as a picture is chosen; it can also be removed).
    ///   Email and Account Created are read-only.
    ///   Change Password: a modal opened from the Security card (always needs the current password). A failed attempt
    ///   keeps the message inside the modal and reopens it; a success reloads the page with the usual success message.
    /// The two forms are independent: saving one never touches the other's data.
    /// </summary>
    public partial class ProfileEditor : UserControl
    {
        private const int MaxNameLength = 150;
        private const string GenericError = "Something went wrong. Please try again.";

        private UserProfile _profile;

        /// <summary>Learner pages turn this on to show a small activity summary (lessons and games completed).</summary>
        public bool ShowActivity { get; set; }

        public bool HasPhoto
        {
            get { return _profile != null && ProfileImageStorage.ResolveForDisplay(_profile.ProfileImagePath) != null; }
        }

        private int? UserId
        {
            get { return SessionHelper.CurrentUserId(Session); }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // One definition of the password rule (PasswordPolicy), shared with Registration: the browser-side validator
            // uses the same pattern and message as the server-side PasswordPolicy.IsValid check in btnChangePassword_Click.
            revNew.ValidationExpression = PasswordPolicy.Pattern;
            revNew.ErrorMessage = PasswordPolicy.Message;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserId.HasValue)
            {
                Response.Redirect("~/Login.aspx", true);
                return;
            }

            try
            {
                _profile = UserProfileRepository.GetProfile(UserId.Value);
            }
            catch (Exception)
            {
                _profile = null;
            }

            if (_profile == null)
            {
                FlashMessage.Show(lblMessage, GenericError, true);
                return;
            }

            if (!IsPostBack)
            {
                FlashMessage.ShowPending(Session, lblMessage);
                txtFullName.Text = _profile.FullName;
            }

            BindReadOnly();
        }

        private void BindReadOnly()
        {
            string photo = ProfileImageStorage.ResolveForDisplay(_profile.ProfileImagePath);

            if (photo != null)
            {
                imgProfile.Src = ResolveUrl("~/" + photo);
                imgProfile.Attributes.Remove("hidden");
            }
            else
            {
                imgProfile.Src = "data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==";
                imgProfile.Attributes["hidden"] = "hidden";
            }

            litInitials.Text = HttpUtility.HtmlEncode(AvatarInitials.From(_profile.FullName));
            litHeadName.Text = HttpUtility.HtmlEncode(_profile.FullName);
            btnRemovePhoto.Visible = photo != null;
            txtFullName.Attributes["data-initial"] = _profile.FullName;
            BindActivity();
            litEmail.Text = HttpUtility.HtmlEncode(_profile.Email);
            litCreated.Text = HttpUtility.HtmlEncode(
                DateTime.SpecifyKind(_profile.CreatedAt, DateTimeKind.Utc).ToLocalTime().ToString("d MMMM yyyy", CultureInfo.InvariantCulture));
        }

        private void BindActivity()
        {
            if (!ShowActivity || !UserId.HasValue)
            {
                return;
            }

            try
            {
                int completed = LearningProgressRepository.GetCompletedLessonCount(UserId.Value);
                int total = LessonRepository.GetTotalActiveLessonCount();
                int games = HiddenRoadSignAttemptRepository.GetCompletedCount(UserId.Value);

                litActivityLessons.Text = completed.ToString(CultureInfo.InvariantCulture) + " / " + total.ToString(CultureInfo.InvariantCulture);
                litActivityGames.Text = games.ToString(CultureInfo.InvariantCulture);
                pnlActivity.Visible = true;
            }
            catch (Exception)
            {
                // The summary is an extra; the profile works without it.
            }
        }

        // ------------------------------------------------------------------
        // Personal Information: Full Name only (Save Changes)
        // ------------------------------------------------------------------
        protected void btnSaveProfile_Click(object sender, EventArgs e)
        {
            Page.Validate("ProfileForm");

            if (!Page.IsValid || _profile == null || !UserId.HasValue)
            {
                return;
            }

            string fullName = (txtFullName.Text ?? string.Empty).Trim();

            if (fullName.Length == 0 || fullName.Length > MaxNameLength || fullName.IndexOfAny(new[] { '<', '>' }) >= 0)
            {
                FlashMessage.Show(lblMessage, "Please enter your full name (up to " + MaxNameLength + " characters, without < or >).", true);
                return;
            }

            try
            {
                // Only the name: the profile picture and the password have their own actions.
                if (!UserProfileRepository.UpdateFullName(UserId.Value, fullName))
                {
                    FlashMessage.Show(lblMessage, GenericError, true);
                    return;
                }

                SessionHelper.SetFullName(Session, fullName);

                FlashMessage.Set(Session, "Profile updated successfully.");
                Response.Redirect(Request.RawUrl, true);
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception)
            {
                FlashMessage.Show(lblMessage, GenericError, true);
            }
        }

        // ------------------------------------------------------------------
        // Profile picture: saved the moment a picture is chosen (no Save Changes needed). It can be replaced or removed.
        // ------------------------------------------------------------------
        protected void btnUploadPhoto_Click(object sender, EventArgs e)
        {
            if (_profile == null || !UserId.HasValue)
            {
                return;
            }

            if (!fuPhoto.HasFile)
            {
                FlashMessage.Show(lblMessage, "Please select a JPG, JPEG, PNG or WEBP image.", true);
                return;
            }

            string newImagePath = null;
            bool committed = false;

            try
            {
                string imageError = ProfileImageStorage.Validate(fuPhoto.PostedFile);   // extension, 2 MB, real file signature

                if (imageError != null)
                {
                    FlashMessage.Show(lblMessage, imageError, true);
                    return;
                }

                newImagePath = ProfileImageStorage.Save(fuPhoto.PostedFile);   // GUID file name inside Content/images/profile/

                ProfileUpdateResult result = UserProfileRepository.ReplaceProfileImage(UserId.Value, newImagePath);

                if (!result.Found)
                {
                    ProfileImageStorage.Delete(newImagePath);
                    FlashMessage.Show(lblMessage, "Could not update profile picture. Please try again.", true);
                    return;
                }

                committed = true;

                // The database now points at the new picture; only now is the old, unreferenced file removed.
                if (!string.IsNullOrEmpty(result.PreviousImagePath)
                    && !string.Equals(result.PreviousImagePath, newImagePath, StringComparison.Ordinal))
                {
                    ProfileImageStorage.Delete(result.PreviousImagePath);
                }

                // The header reads the path from Session (a small string, never image data).
                SessionHelper.SetProfileImagePath(Session, newImagePath);

                FlashMessage.Set(Session, "Profile picture updated successfully.");
                Response.Redirect(Request.RawUrl, true);
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception)
            {
                if (!committed)
                {
                    // The previous path and file are untouched; drop the failed new file.
                    ProfileImageStorage.Delete(newImagePath);
                }

                FlashMessage.Show(lblMessage, "Could not update profile picture. Please try again.", true);
            }
        }

        protected void btnRemovePhoto_Click(object sender, EventArgs e)
        {
            if (_profile == null || !UserId.HasValue)
            {
                return;
            }

            try
            {
                ProfileUpdateResult result = UserProfileRepository.ClearProfileImage(UserId.Value);

                if (!result.Found)
                {
                    FlashMessage.Show(lblMessage, "Could not remove profile picture. Please try again.", true);
                    return;
                }

                // The database no longer points at the file; only now is it deleted.
                if (!string.IsNullOrEmpty(result.PreviousImagePath))
                {
                    ProfileImageStorage.Delete(result.PreviousImagePath);
                }

                SessionHelper.SetProfileImagePath(Session, null);

                FlashMessage.Set(Session, "Profile picture removed.");
                Response.Redirect(Request.RawUrl, true);
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception)
            {
                FlashMessage.Show(lblMessage, "Could not remove profile picture. Please try again.", true);
            }
        }

        // ------------------------------------------------------------------
        // Security: Change Password (never logs the user out)
        // ------------------------------------------------------------------
        /// <summary>A failed password attempt: show the message INSIDE the modal and have the page reopen the modal.</summary>
        private void ShowPasswordError(string message)
        {
            FlashMessage.Show(lblPasswordMessage, message, true);
            pnlPasswordModal.Attributes["data-open"] = "1";
        }

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            Page.Validate("PasswordForm");

            if (!Page.IsValid || !UserId.HasValue)
            {
                pnlPasswordModal.Attributes["data-open"] = "1";
                return;
            }

            string current = txtCurrentPassword.Text ?? string.Empty;
            string newPassword = txtNewPassword.Text ?? string.Empty;
            string confirm = txtConfirmPassword.Text ?? string.Empty;

            try
            {
                string storedHash = UserProfileRepository.GetPasswordHash(UserId.Value);

                if (storedHash == null || !PasswordHasher.VerifyPassword(current, storedHash))
                {
                    ShowPasswordError("Current password is incorrect.");
                    return;
                }

                if (!PasswordPolicy.IsValid(newPassword))
                {
                    ShowPasswordError(PasswordPolicy.Message);
                    return;
                }

                if (!string.Equals(newPassword, confirm, StringComparison.Ordinal))
                {
                    ShowPasswordError("Passwords do not match.");
                    return;
                }

                if (PasswordHasher.VerifyPassword(newPassword, storedHash))
                {
                    ShowPasswordError("New password must be different from your current password.");
                    return;
                }

                // Same PBKDF2-SHA256 helper as registration/login: a fresh salt and hash.
                if (!UserProfileRepository.UpdatePasswordHash(UserId.Value, PasswordHasher.HashPassword(newPassword)))
                {
                    ShowPasswordError(GenericError);
                    return;
                }

                // The session stays signed in; reloading the page clears the password boxes.
                FlashMessage.Set(Session, "Password changed successfully.");
                Response.Redirect(Request.RawUrl, true);
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception)
            {
                ShowPasswordError(GenericError);
            }
        }
    }
}
