using System;

namespace MyKPPLearn.Common
{
    /// <summary>
    /// Derives a short initials string (1-2 characters) from a user's full name,
    /// used for the authenticated header avatar badge.
    /// </summary>
    public static class AvatarInitials
    {
        public static string From(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return "?";
            }

            string[] parts = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return "?";
            }

            if (parts.Length == 1)
            {
                return char.ToUpperInvariant(parts[0][0]).ToString();
            }

            char first = char.ToUpperInvariant(parts[0][0]);
            char last = char.ToUpperInvariant(parts[parts.Length - 1][0]);

            return string.Concat(first, last);
        }
    }
}
