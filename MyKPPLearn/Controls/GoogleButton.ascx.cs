using System;
using System.Threading;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Controls
{
    /// <summary>
    /// "Continue with Google" on the Login and Register pages. A verified Google account signs in to the
    /// learner account it is linked to, or to the learner account with the same email (linking it),
    /// or creates a new learner. Administrators always sign in with their password.
    /// </summary>
    public partial class GoogleButton : System.Web.UI.UserControl
    {
        /// <summary>Google's button label: "signin_with", "signup_with" or "continue_with".</summary>
        public string ButtonText { get; set; } = "continue_with";

        protected string GoogleClientId { get { return GoogleSignIn.ClientId; } }

        /// <summary>The text on our own button, matching Google's label for <see cref="ButtonText"/>.</summary>
        protected string ButtonLabel
        {
            get
            {
                switch (ButtonText)
                {
                    case "signin_with": return "Sign in with Google";
                    case "signup_with": return "Sign up with Google";
                    default: return "Continue with Google";
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Not configured: show the page without the Google option rather than a broken button.
            phGoogle.Visible = GoogleSignIn.ClientId != null;
        }

        protected void btnGoogle_Click(object sender, EventArgs e)
        {
            string credential = hfCredential.Value;
            hfCredential.Value = string.Empty;

            try
            {
                GoogleIdentity identity = GoogleSignIn.Verify(credential);

                if (identity == null)
                {
                    ShowError("We could not sign you in with Google. Please try again.");
                    return;
                }

                AppUser user = UserRepository.GetUserByGoogleSubject(identity.Subject);

                if (user == null)
                {
                    user = UserRepository.GetUserByEmail(identity.Email);

                    if (user != null && string.Equals(user.Role, SessionHelper.RoleLearner, StringComparison.Ordinal))
                    {
                        // Google has verified this email, so it is the same person: link the accounts.
                        UserRepository.LinkGoogleSubject(user.UserId, identity.Subject);
                    }
                    else if (user == null)
                    {
                        string name = string.IsNullOrWhiteSpace(identity.Name) ? identity.Email.Split('@')[0] : identity.Name.Trim();
                        int userId = UserRepository.CreateGoogleLearner(name.Length > 150 ? name.Substring(0, 150) : name, identity.Email, identity.Subject);
                        user = UserRepository.GetUserByEmail(identity.Email);

                        if (user == null || user.UserId != userId)
                        {
                            ShowError("We could not sign you in with Google. Please try again.");
                            return;
                        }
                    }
                }

                if (!string.Equals(user.Role, SessionHelper.RoleLearner, StringComparison.Ordinal))
                {
                    ShowError("Administrators sign in with their email and password.");
                    return;
                }

                if (!string.Equals(user.Status, "Active", StringComparison.Ordinal))
                {
                    ShowError("We could not sign you in with Google. Please try again.");
                    return;
                }

                SessionHelper.SignIn(Session, user.UserId, user.FullName, user.Role, user.ProfileImagePath);
                Response.Redirect("~/Learner/Dashboard.aspx", true);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception)
            {
                ShowError("We could not sign you in with Google. Please try again.");
            }
        }

        private void ShowError(string message)
        {
            lblGoogleError.Text = message;
            lblGoogleError.Visible = true;
        }
    }
}
