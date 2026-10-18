using System;

namespace MyKPPLearn.Data
{
    /// <summary>The profile view of a Users row. Deliberately has no PasswordHash.</summary>
    public sealed class UserProfile
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string ProfileImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public sealed class ProfileUpdateResult
    {
        /// <summary>False when no such user exists.</summary>
        public bool Found;
        /// <summary>The picture path stored BEFORE a picture replacement (may be null); delete that file only after the update succeeded.</summary>
        public string PreviousImagePath;
    }
}
