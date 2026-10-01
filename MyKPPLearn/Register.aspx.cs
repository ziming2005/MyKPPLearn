using System;
using System.Threading;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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
