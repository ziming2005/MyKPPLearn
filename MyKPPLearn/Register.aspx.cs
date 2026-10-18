using System;
using System.Threading;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            // The password rule has ONE definition (PasswordPolicy), shared with Profile > Change Password.
            // The validator keeps giving instant feedback in the browser, using that same rule and message.
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
                txtFullName.Focus();
            }
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            lblFormError.Visible = false;
            lblFormError.Text = string.Empty;

            if (!Page.IsValid)
            {
                return;
            }

            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim().ToLowerInvariant();
            string password = txtPassword.Text;

            // Authoritative server-side check, from the same policy as Profile > Change Password.
            if (!PasswordPolicy.IsValid(password))
            {
                ShowError(PasswordPolicy.Message);
                return;
            }

            try
            {
                if (UserRepository.EmailExists(email))
                {
                    ShowError("An account with this email already exists.");
                    return;
                }

                string passwordHash = PasswordHasher.HashPassword(password);
                int userId = UserRepository.CreateLearner(fullName, email, passwordHash);

                SessionHelper.SignIn(Session, userId, fullName, SessionHelper.RoleLearner);
                Session["RegistrationSuccess"] = true;

                Response.Redirect("~/Learner/Dashboard.aspx", true);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception)
            {
                ShowError("We could not create your account. Please try again.");
            }
        }

        private void ShowError(string message)
        {
            lblFormError.Text = message;
            lblFormError.Visible = true;
        }
    }
}
