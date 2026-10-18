using System.Text.RegularExpressions;

namespace MyKPPLearn.Security
{
    /// <summary>
    /// The password rule used at registration, shared by the Profile "Change Password" form:
    /// at least 8 characters with an uppercase letter, a lowercase letter and a number.
    /// Keep <see cref="Pattern"/> identical to the validator in Register.aspx.
    /// </summary>
    public static class PasswordPolicy
    {
        public const string Pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$";

        public const string Message = "Password must be at least 8 characters and include an uppercase letter, lowercase letter, and number.";

        public static bool IsValid(string password)
        {
            return password != null && Regex.IsMatch(password, Pattern);
        }
    }
}
