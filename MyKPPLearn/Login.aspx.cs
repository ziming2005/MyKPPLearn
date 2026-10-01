using System;
using System.Threading;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            lblFormError.Visible = false;
            lblFormError.Text = string.Empty;

            if (!Page.IsValid)
            {
                return;
            }

            string email = txtEmail.Text.Trim().ToLowerInvariant();
            string password = txtPassword.Text;

            try
            {
                AppUser user = UserRepository.GetUserByEmail(email);

                if (user == null || !PasswordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    ShowError("Invalid email or password.");
                    return;
                }

                SessionHelper.SignIn(Session, user.UserId, user.FullName, user.Role);

                if (string.Equals(user.Role, SessionHelper.RoleAdministrator, StringComparison.Ordinal))
                {
                    Response.Redirect("~/Admin/Dashboard.aspx", true);
                }
                else
                {
                    Response.Redirect("~/Learner/Dashboard.aspx", true);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception)
            {
                ShowError("We could not sign you in. Please try again.");
            }
        }

        private void ShowError(string message)
        {
            lblFormError.Text = message;
            lblFormError.Visible = true;
        }
    }
}
