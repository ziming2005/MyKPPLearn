using System;
using System.IO;
using System.Web;

namespace MyKPPLearn.Common
{
    /// <summary>
    /// Validates and stores the single optional lesson image under Content/images/learning/.
    /// Only the relative path (e.g. "Content/images/learning/&lt;guid&gt;.png") is stored in the database.
    /// </summary>
    public static class LessonImageStorage
    {
        public const int MaxBytes = 2 * 1024 * 1024;
        private const string RelativeFolder = "Content/images/learning/";

        /// <summary>Returns null when the upload is acceptable, otherwise a friendly error message.</summary>
        public static string Validate(HttpPostedFile file)
        {
            return Validate(file, MaxBytes);
        }

        /// <summary>Same checks with a caller-chosen size limit (used by the game Scene Image).</summary>
        public static string Validate(HttpPostedFile file, int maxBytes)
        {
            string extension = Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();

            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".webp")
            {
                return "Image must be a .jpg, .jpeg, .png or .webp file.";
            }

            if (file.ContentLength <= 0 || file.ContentLength > maxBytes)
            {
                return "Image must be smaller than " + (maxBytes / (1024 * 1024)) + " MB.";
            }

            byte[] header = new byte[12];
            int read = file.InputStream.Read(header, 0, header.Length);
            file.InputStream.Position = 0;

            if (read < 12 || !MatchesSignature(extension, header))
            {
                return "The uploaded file is not a valid image.";
            }

            return null;
        }

        /// <summary>Saves a previously validated upload and returns its relative path.</summary>
        public static string Save(HttpPostedFile file)
        {
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string fileName = Guid.NewGuid().ToString("N") + extension;
            string folder = HttpContext.Current.Server.MapPath("~/" + RelativeFolder);

            Directory.CreateDirectory(folder);
            file.SaveAs(Path.Combine(folder, fileName));

            return RelativeFolder + fileName;
        }

        /// <summary>Best-effort removal of a stored lesson image; ignores paths outside the learning folder.</summary>
        public static void Delete(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)
                || !relativePath.StartsWith(RelativeFolder, StringComparison.Ordinal)
                || relativePath.IndexOf("..", StringComparison.Ordinal) >= 0)
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

        private static bool MatchesSignature(string extension, byte[] h)
        {
            if (extension == ".png")
            {
                return h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47;
            }

            if (extension == ".webp")
            {
                return h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F'
                    && h[8] == 'W' && h[9] == 'E' && h[10] == 'B' && h[11] == 'P';
            }

            return h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF;
        }
    }
}
