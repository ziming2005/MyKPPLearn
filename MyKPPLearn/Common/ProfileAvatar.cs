using System;
using System.Web;
using System.Web.SessionState;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Common
{
    /// <summary>
    /// Fills the header avatar of Learner.Master / Admin.Master: the profile picture when there is a valid one, otherwise the
    /// initials. The picture path is a small string kept in Session (loaded once if this session does not have it yet), so
    /// there is no database query per page and no image data in Session.
    /// </summary>
    public static class ProfileAvatar
    {
        public static void Bind(HttpSessionState session, string fullName, Literal initials, HtmlImage image)
        {
            string path = SessionHelper.CurrentProfileImagePath(session);

            if (path == null)
            {
                // A session that started before profile pictures existed: load the path once.
                path = string.Empty;
                int? userId = SessionHelper.CurrentUserId(session);

                if (userId.HasValue)
                {
                    try
                    {
                        UserProfile profile = UserProfileRepository.GetProfile(userId.Value);
                        path = profile == null ? string.Empty : (profile.ProfileImagePath ?? string.Empty);
                        SessionHelper.SetProfileImagePath(session, path);
                    }
                    catch (Exception)
                    {
                        path = string.Empty;
                    }
                }
            }

            string display = ProfileImageStorage.ResolveForDisplay(path);

            if (display != null)
            {
                image.Src = VirtualPathUtility.ToAbsolute("~/" + display);
                image.Alt = string.Empty;
                image.Visible = true;
                initials.Visible = false;
            }
            else
            {
                image.Visible = false;
                initials.Visible = true;
                initials.Text = AvatarInitials.From(fullName);
            }
        }
    }
}
