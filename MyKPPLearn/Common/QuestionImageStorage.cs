using System;
using System.IO;
using System.Web;
using MyKPPLearn.Data;

namespace MyKPPLearn.Common
{
    /// <summary>Stores optional Question Bank images separately from lesson and game assets.</summary>
    public static class QuestionImageStorage
    {
        public const string RelativeFolder = "Content/images/questions/";
        public const int MaxBytes = 2 * 1024 * 1024;
        public static string Validate(HttpPostedFile file) { return LessonImageStorage.Validate(file, MaxBytes); }
        public static string Save(HttpPostedFile file)
        {
            string error=Validate(file); if(error!=null) throw new ArgumentException(error);
            string extension=Path.GetExtension(file.FileName).ToLowerInvariant(); string folder=HttpContext.Current.Server.MapPath("~/"+RelativeFolder); Directory.CreateDirectory(folder);
            string path=RelativeFolder+Guid.NewGuid().ToString("N")+extension; file.SaveAs(Path.Combine(folder,Path.GetFileName(path))); return path;
        }
        /// <summary>Stores a browser-uploaded data URL after applying the same size and signature rules.</summary>
        public static string SaveDataUrl(string dataUrl)
        {
            if(string.IsNullOrEmpty(dataUrl) || !dataUrl.StartsWith("data:image/",StringComparison.Ordinal)) throw new ArgumentException("Choose a JPG, PNG or WEBP image.");
            int comma=dataUrl.IndexOf(','); if(comma<0)throw new ArgumentException("The uploaded image is invalid.");
            byte[] bytes; try { bytes=Convert.FromBase64String(dataUrl.Substring(comma+1)); } catch(FormatException){throw new ArgumentException("The uploaded image is invalid.");}
            if(bytes.Length==0||bytes.Length>MaxBytes)throw new ArgumentException("Image must be smaller than 2 MB.");
            string type=dataUrl.Substring(5,Math.Max(0,dataUrl.IndexOf(';')-5)).ToLowerInvariant(); string extension=type=="image/png"?".png":type=="image/webp"?".webp":type=="image/jpeg"?".jpg":null;
            if(extension==null||!IsSignature(extension,bytes))throw new ArgumentException("The uploaded file is not a valid image.");
            string folder=HttpContext.Current.Server.MapPath("~/"+RelativeFolder);Directory.CreateDirectory(folder);string path=RelativeFolder+Guid.NewGuid().ToString("N")+extension;File.WriteAllBytes(Path.Combine(folder,Path.GetFileName(path)),bytes);return path;
        }
        /// <summary>Removes an obsolete image only after checking both immutable attempt snapshots.</summary>
        public static void DeleteIfUnreferenced(string relativePath)
        {
            if(string.IsNullOrEmpty(relativePath)||!relativePath.StartsWith(RelativeFolder,StringComparison.Ordinal)||relativePath.IndexOf("..",StringComparison.Ordinal)>=0)return;
            int references=Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT (SELECT COUNT(*) FROM Questions WHERE ImagePath=@Path)+(SELECT COUNT(*) FROM PracticeAttemptQuestions WHERE ImagePathSnapshot=@Path)+(SELECT COUNT(*) FROM MockExamAttemptQuestions WHERE ImagePathSnapshot=@Path)",new System.Data.SqlClient.SqlParameter("@Path",relativePath)));
            if(references!=0)return;
            try { string physical=HttpContext.Current.Server.MapPath("~/"+relativePath); if(File.Exists(physical))File.Delete(physical); } catch(IOException){} catch(UnauthorizedAccessException){}
        }
        private static bool IsSignature(string extension,byte[] bytes) { if(extension==".png")return bytes.Length>=4&&bytes[0]==0x89&&bytes[1]==0x50&&bytes[2]==0x4E&&bytes[3]==0x47; if(extension==".webp")return bytes.Length>=12&&bytes[0]=='R'&&bytes[1]=='I'&&bytes[2]=='F'&&bytes[3]=='F'&&bytes[8]=='W'&&bytes[9]=='E'&&bytes[10]=='B'&&bytes[11]=='P';return bytes.Length>=3&&bytes[0]==0xFF&&bytes[1]==0xD8&&bytes[2]==0xFF; }
    }
}
