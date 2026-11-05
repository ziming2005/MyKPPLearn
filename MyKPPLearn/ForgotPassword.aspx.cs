using System;
using System.Threading;
using System.Web;
using MyKPPLearn.Common;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn
{
    /// <summary>
    /// Forgot password, three steps on one page: (1) email a 6-digit code, (2) verify the code,
    /// (3) choose a new password. Steps 1 and 2 answer the same way whether or not the email has an
    /// account, so the page cannot be used to find out who is registered.
    /// </summary>
    public partial class ForgotPassword : System.Web.UI.Page
    {
        private enum Step { Email, Code, Password }

        // Kept between steps in ViewState, which is signed, so the browser cannot swap them.
        // Step 3 is still checked against the database: the verified code must be unused and unexpired.
        private string PendingEmail
        {
            get { return ViewState["ResetEmail"] as string; }
            set { ViewState["ResetEmail"] = value; }
        }

        private int? VerifiedTokenId
        {
            get { return ViewState["ResetTokenId"] as int?; }
            set { ViewState["ResetTokenId"] = value; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            revPassword.ValidationExpression = PasswordPolicy.Pattern;
            revPassword.ErrorMessage = "Password does not meet the requirements above.";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            string dashboard = SessionHelper.DashboardUrl(Session);

            if (dashboard != null)
            {
                Response.Redirect(dashboard, true);
            }

            if (!IsPostBack)
            {
                ShowStep(Step.Email);
                txtEmail.Focus();
            }
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            string email = txtEmail.Text.Trim().ToLowerInvariant();

            if (!SendCode(email))
            {
                return;
            }

            PendingEmail = email;
            VerifiedTokenId = null;
            ShowStep(Step.Code);
            ShowMessage("If an account uses that email, we've sent it a 6-digit code. Check your spam folder if it doesn't arrive.", false);
            txtCode.Focus();
        }

        protected void btnResend_Click(object sender, EventArgs e)
        {
            if (PendingEmail == null)
            {
                ShowStep(Step.Email);
                return;
            }

            if (SendCode(PendingEmail))
            {
                ShowMessage("If an account uses that email, a new code is on its way. Codes can be resent once a minute.", false);
            }

            txtCode.Focus();
        }

        protected void btnChangeEmail_Click(object sender, EventArgs e)
        {
            PendingEmail = null;
            VerifiedTokenId = null;
            txtEmail.Text = string.Empty;
            ShowStep(Step.Email);
            txtEmail.Focus();
        }

        protected void btnVerify_Click(object sender, EventArgs e)
        {
            if (PendingEmail == null)
            {
                ShowStep(Step.Email);
                return;
            }

            if (!Page.IsValid)
            {
                return;
            }

            try
            {
                AppUser user = UserRepository.GetUserByEmail(PendingEmail);
                int? tokenId = user == null ? null : PasswordResetRepository.VerifyCode(user.UserId, txtCode.Text.Trim());
                txtCode.Text = string.Empty;

                if (tokenId == null)
                {
                    ShowMessage("That code is incorrect or has expired. Codes last " + (int)PasswordResetRepository.Lifetime.TotalMinutes
                        + " minutes and allow " + PasswordResetRepository.MaxAttempts + " tries. Check it, or resend a new code.", true);
                    txtCode.Focus();
                    return;
                }

                VerifiedTokenId = tokenId;
                ShowStep(Step.Password);
                ShowMessage("Code verified. Now choose your new password.", false);
                txtPassword.Focus();
            }
            catch (Exception)
            {
                ShowMessage("We could not check the code. Please try again.", true);
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            if (PendingEmail == null || VerifiedTokenId == null)
            {
                ShowStep(Step.Email);
                return;
            }

            if (!Page.IsValid)
            {
                return;
            }

            string password = txtPassword.Text;

            if (!PasswordPolicy.IsValid(password))
            {
                ShowMessage(PasswordPolicy.Message, true);
                return;
            }

            try
            {
                AppUser user = UserRepository.GetUserByEmail(PendingEmail);

                if (user == null || !PasswordResetRepository.ResetPassword(user.UserId, VerifiedTokenId.Value, PasswordHasher.HashPassword(password)))
                {
                    VerifiedTokenId = null;
                    ShowStep(Step.Code);
                    ShowMessage("Your code has expired. Resend a new code to try again.", true);
                    return;
                }

                // Anyone still signed in on this browser is signed out; the new password is used from now on.
                SessionHelper.SignOut(Session);
                FlashMessage.Set(Session, "Your password has been reset. Log in with your new password.");
                Response.Redirect("~/Login.aspx", true);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception)
            {
                ShowMessage("We could not reset your password. Please try again.", true);
            }
        }

        /// <summary>Emails a new code if the email belongs to an active account. False only if sending failed.</summary>
        private bool SendCode(string email)
        {
            try
            {
                AppUser user = UserRepository.GetUserByEmail(email);

                if (user != null && string.Equals(user.Status, "Active", StringComparison.Ordinal))
                {
                    string code = PasswordResetRepository.CreateCode(user.UserId);

                    if (code != null)
                    {
                        EmailSender.Send(user.Email, "Your MyKPP Learn password reset code", CodeEmailBody(user.FullName, code));
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Mail or database trouble: say so, without revealing whether the account exists.
                System.Diagnostics.Trace.TraceError("Password reset email failed: " + ex);
                ShowMessage("We could not send the email right now. Please try again later.", true);
                return false;
            }
        }

        private static string CodeEmailBody(string fullName, string code)
        {
            return "Hi " + fullName + ",\r\n\r\n"
                + "Your MyKPP Learn password reset code is:\r\n\r\n"
                + "    " + code + "\r\n\r\n"
                + "Enter it on the Forgot Password page within " + (int)PasswordResetRepository.Lifetime.TotalMinutes + " minutes.\r\n"
                + "Never share this code with anyone. MyKPP Learn will never ask you for it.\r\n\r\n"
                + "If you didn't ask for this, you can ignore this email. Your password won't change.\r\n\r\n"
                + "MyKPP Learn";
        }

        private void ShowStep(Step step)
        {
            pnlEmail.Visible = step == Step.Email;
            pnlCode.Visible = step == Step.Code;
            pnlPassword.Visible = step == Step.Password;

            string email = "<strong>" + HttpUtility.HtmlEncode(PendingEmail) + "</strong>";

            switch (step)
            {
                case Step.Code:
                    litIntro.Text = "<p>Enter the 6-digit code we emailed to " + email + ".</p>";
                    break;
                case Step.Password:
                    litIntro.Text = "<p>Choose a new password for " + email + ".</p>";
                    break;
                default:
                    litIntro.Text = "<p>Enter your email and we'll send you a 6-digit code to reset your password.</p>";
                    break;
            }
        }

        private void ShowMessage(string text, bool isError)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(text);
            lblMessage.CssClass = isError ? "alert alert-error" : "alert alert-success";
            lblMessage.Visible = true;
        }
    }
}
