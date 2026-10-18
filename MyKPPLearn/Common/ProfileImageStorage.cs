using System;
using System.IO;
using System.Web;

namespace MyKPPLearn.Common
{
    /// <summary>
    /// Profile pictures live in their own folder (Content/images/profile/), separate from lesson images, road sign assets and
    /// scene images. Only the relative path is stored in Users.ProfileImagePath. Physical names are always generated GUIDs.
    /// Rules: .jpg/.jpeg/.png/.webp, 2 MB, and the file's real signature must match (the extension alone is not trusted).
    /// </summary>
    public static class ProfileImageStorage
    {
        public const string Folder = "Content/images/profile/";
        public const int MaxBytes = 2 * 1024 * 1024;

        /// <summary>Returns null when the upload is acceptable, otherwise a friendly message.</summary>
        public static string Validate(HttpPostedFile file)
        {
            string extension = Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();

            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".webp")
            {
                return "Please select a JPG, JPEG, PNG or WEBP image.";
            }

            if (file.ContentLength <= 0 || file.ContentLength > MaxBytes)
            {
                return "Profile image must be 2 MB or smaller.";
            }

            // Real content check (file signature), shared with the other image uploads.
            return LessonImageStorage.Validate(file, MaxBytes);
        }

        /// <summary>Saves a previously validated upload under a generated GUID name and returns its relative path.</summary>
        public static string Save(HttpPostedFile file)
        {
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string fileName = Guid.NewGuid().ToString("N") + extension;
            string folder = HttpContext.Current.Server.MapPath("~/" + Folder);

            Directory.CreateDirectory(folder);
            file.SaveAs(Path.Combine(folder, fileName));

            return Folder + fileName;
        }

        /// <summary>True only for a path this application could have produced: inside the profile folder, no traversal, a GUID-style file name.</summary>
        public static bool IsValidPath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)
                || relativePath.Length > 500
                || !relativePath.StartsWith(Folder, StringComparison.Ordinal)
                || relativePath.IndexOf("..", StringComparison.Ordinal) >= 0
                || relativePath.IndexOf('\\') >= 0)
            {
                return false;
            }

            string name = relativePath.Substring(Folder.Length);

            if (name.IndexOf('/') >= 0 || name.Length == 0)
            {
                return false;
            }

            string extension = Path.GetExtension(name).ToLowerInvariant();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".webp";
        }

        /// <summary>The path to show, or null (=> use initials) when there is no picture, the path is not a valid profile path, or the file is missing.</summary>
        public static string ResolveForDisplay(string relativePath)
        {
            if (!IsValidPath(relativePath))
            {
                return null;
            }

            try
            {
                return File.Exists(HttpContext.Current.Server.MapPath("~/" + relativePath)) ? relativePath : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Best-effort removal of a stored profile picture; ignores anything outside the profile folder.</summary>
        public static void Delete(string relativePath)
        {
            if (!IsValidPath(relativePath))
            {
                return;
            }

            try
            {
                string physical = HttpContext.Current.Server.MapPath("~/" + relativePath);

                if (File.Exists(physical))
                {
                    File.Delete(physical);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
