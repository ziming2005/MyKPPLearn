using System;
using System.IO;
using System.Web;

namespace MyKPPLearn.Common
{
    /// <summary>
    /// Stores Hidden Road Sign Game images. Road Sign Asset images and Scene Images are
    /// deliberately kept in separate folders. Only the relative path (for example
    /// "Content/images/road-signs/&lt;guid&gt;.png") is stored in the database; the physical
    /// file name is always a generated GUID, never anything the user typed.
    /// Upload rules (extension, size limit, file signature) are shared with the lesson images.
    /// </summary>
    public static class GameImageStorage
    {
        public const string RoadSignFolder = "Content/images/road-signs/";
        public const string SceneFolder = "Content/images/hidden-road-sign-scenes/";

        public const int MaxRoadSignBytes = 2 * 1024 * 1024;
        public const int MaxSceneBytes = 4 * 1024 * 1024;

        /// <summary>Returns null when a Road Sign Asset upload is acceptable (2 MB), otherwise a friendly error message.</summary>
        public static string Validate(HttpPostedFile file)
        {
            return LessonImageStorage.Validate(file, MaxRoadSignBytes);
        }

        /// <summary>Returns null when a Scene Image upload is acceptable (4 MB), otherwise a friendly error message.</summary>
        public static string ValidateScene(HttpPostedFile file)
        {
            return LessonImageStorage.Validate(file, MaxSceneBytes);
        }

        public static string SaveRoadSign(HttpPostedFile file)
        {
            return Save(file, RoadSignFolder);
        }

        public static string SaveScene(HttpPostedFile file)
        {
            return Save(file, SceneFolder);
        }

        private static string Save(HttpPostedFile file, string relativeFolder)
        {
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string fileName = Guid.NewGuid().ToString("N") + extension;
            string folder = HttpContext.Current.Server.MapPath("~/" + relativeFolder);

            Directory.CreateDirectory(folder);
            file.SaveAs(Path.Combine(folder, fileName));

            return relativeFolder + fileName;
        }

        /// <summary>
        /// Best-effort removal of a stored game image. Ignores null/empty paths and anything
        /// outside the two game folders. Callers must only call this AFTER the database change
        /// that stops referencing the file has been committed.
        /// </summary>
        public static void Delete(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)
                || relativePath.IndexOf("..", StringComparison.Ordinal) >= 0
                || (!relativePath.StartsWith(RoadSignFolder, StringComparison.Ordinal)
                    && !relativePath.StartsWith(SceneFolder, StringComparison.Ordinal)))
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
