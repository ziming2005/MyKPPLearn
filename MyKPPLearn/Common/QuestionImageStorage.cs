using System;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using MyKPPLearn.Data;

namespace MyKPPLearn.Common
{
    /// <summary>
    /// Question Bank images live in their own folder (Content/images/questions/), separate from lesson, profile and game images.
    /// Only the relative path is stored in Questions.ImagePath. Physical names are always generated GUIDs.
    /// Rules: .jpg/.jpeg/.png/.webp, 2 MB, and the file's real signature must match its type.
    /// </summary>
    public static class QuestionImageStorage
    {
        public const string RelativeFolder = "Content/images/questions/";
        public const int MaxBytes = 2 * 1024 * 1024;

        public static string Validate(HttpPostedFile file)
        {
            return LessonImageStorage.Validate(file, MaxBytes);
        }

        public static string Save(HttpPostedFile file)
        {
            string error = Validate(file);

            if (error != null)
            {
                throw new ArgumentException(error);
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string relativePath = NewRelativePath(extension);
            file.SaveAs(HttpContext.Current.Server.MapPath("~/" + relativePath));
            return relativePath;
        }

        /// <summary>Stores an image the browser sent as a data URL, after the same size and signature checks.</summary>
        public static string SaveDataUrl(string dataUrl)
        {
            if (string.IsNullOrEmpty(dataUrl) || !dataUrl.StartsWith("data:image/", StringComparison.Ordinal))
            {
                throw new ArgumentException("Choose a JPG, PNG or WEBP image.");
            }

            int comma = dataUrl.IndexOf(',');
            int semicolon = dataUrl.IndexOf(';');

            if (comma < 0 || semicolon < 0 || semicolon > comma)
            {
                throw new ArgumentException("The uploaded image is invalid.");
            }

            byte[] bytes;

            try
            {
                bytes = Convert.FromBase64String(dataUrl.Substring(comma + 1));
            }
            catch (FormatException)
            {
                throw new ArgumentException("The uploaded image is invalid.");
            }

            if (bytes.Length == 0 || bytes.Length > MaxBytes)
            {
                throw new ArgumentException("Image must be 2 MB or smaller.");
            }

            string type = dataUrl.Substring(5, semicolon - 5).ToLowerInvariant();
            string extension = type == "image/png" ? ".png"
                : type == "image/webp" ? ".webp"
                : type == "image/jpeg" ? ".jpg"
                : null;

            if (extension == null || !HasSignature(extension, bytes))
            {
                throw new ArgumentException("The uploaded file is not a valid image.");
            }

            string relativePath = NewRelativePath(extension);
            File.WriteAllBytes(HttpContext.Current.Server.MapPath("~/" + relativePath), bytes);
            return relativePath;
        }

        /// <summary>True only for a path this class could have produced: inside the questions folder, no traversal, one file name.</summary>
        public static bool IsValidPath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)
                || relativePath.Length > 500
                || !relativePath.StartsWith(RelativeFolder, StringComparison.Ordinal)
                || relativePath.IndexOf("..", StringComparison.Ordinal) >= 0
                || relativePath.IndexOf('\\') >= 0)
            {
                return false;
            }

            string name = relativePath.Substring(RelativeFolder.Length);
            string extension = Path.GetExtension(name).ToLowerInvariant();

            return name.Length > 0
                && name.IndexOf('/') < 0
                && (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".webp");
        }

        /// <summary>
        /// Removes an image that no question and no attempt snapshot refers to any more.
        /// Attempt snapshots keep the old path so a learner's past review still shows the image.
        /// </summary>
        public static void DeleteIfUnreferenced(string relativePath)
        {
            if (!IsValidPath(relativePath))
            {
                return;
            }

            const string sql = @"
                SELECT (SELECT COUNT(*) FROM Questions WHERE ImagePath = @Path)
                     + (SELECT COUNT(*) FROM PracticeAttemptQuestions WHERE ImagePathSnapshot = @Path)
                     + (SELECT COUNT(*) FROM MockExamAttemptQuestions WHERE ImagePathSnapshot = @Path)";

            int references = Convert.ToInt32(DatabaseHelper.ExecuteScalar(sql, new SqlParameter("@Path", relativePath)));

            if (references != 0)
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

        private static string NewRelativePath(string extension)
        {
            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/" + RelativeFolder));
            return RelativeFolder + Guid.NewGuid().ToString("N") + extension;
        }

        private static bool HasSignature(string extension, byte[] bytes)
        {
            if (extension == ".png")
            {
                return bytes.Length >= 4 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47;
            }

            if (extension == ".webp")
            {
                return bytes.Length >= 12
                    && bytes[0] == 'R' && bytes[1] == 'I' && bytes[2] == 'F' && bytes[3] == 'F'
                    && bytes[8] == 'W' && bytes[9] == 'E' && bytes[10] == 'B' && bytes[11] == 'P';
            }

            return bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
        }
    }
}
