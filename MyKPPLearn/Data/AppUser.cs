namespace MyKPPLearn.Data
{
    /// <summary>
    /// Plain data holder for a Users row. Never carries the plaintext password.
    /// </summary>
    public class AppUser
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }
}
