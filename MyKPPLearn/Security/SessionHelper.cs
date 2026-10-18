using System;
using System.Web.SessionState;
using System.Web.UI;

namespace MyKPPLearn.Security
{
    /// <summary>
    /// Centralizes the Session-based authentication state (Session["UserId"],
    /// Session["FullName"], Session["Role"]) used across the application.
    /// </summary>
    public static class SessionHelper
    {
        public const string RoleLearner = "Learner";
        public const string RoleAdministrator = "Administrator";

        public static bool IsAuthenticated(HttpSessionState session)
        {
            return session != null
                && session["UserId"] != null
                && session["Role"] != null;
        }

        public static int? CurrentUserId(HttpSessionState session)
        {
            if (session == null || session["UserId"] == null)
            {
                return null;
            }

            return Convert.ToInt32(session["UserId"]);
        }

        public static string CurrentFullName(HttpSessionState session)
        {
            return session == null ? null : session["FullName"] as string;
        }

        /// <summary>The signed-in user's own dashboard (app-relative), or null when nobody is signed in.</summary>
        public static string DashboardUrl(HttpSessionState session)
        {
            if (!IsAuthenticated(session)) { return null; }

            return string.Equals(CurrentRole(session), RoleAdministrator, StringComparison.Ordinal)
                ? "~/Admin/Dashboard.aspx"
                : "~/Learner/Dashboard.aspx";
        }

        public static string CurrentRole(HttpSessionState session)
        {
            return session == null ? null : session["Role"] as string;
        }

        public static void SignIn(HttpSessionState session, int userId, string fullName, string role)
        {
            SignIn(session, userId, fullName, role, null);
        }

        public static void SignIn(HttpSessionState session, int userId, string fullName, string role, string profileImagePath)
        {
            session["UserId"] = userId;
            session["FullName"] = fullName;
            session["Role"] = role;
            SetProfileImagePath(session, profileImagePath);
        }

        /// <summary>Called after a successful profile save so the header shows the new name straight away.</summary>
        public static void SetFullName(HttpSessionState session, string fullName)
        {
            session["FullName"] = fullName;
        }

        /// <summary>
        /// The signed-in user's profile picture path (a small relative path string, never image data):
        /// null when it has not been loaded into this session yet, "" when the user has no picture.
        /// </summary>
        public static string CurrentProfileImagePath(HttpSessionState session)
        {
            return session == null ? null : session["ProfileImagePath"] as string;
        }

        public static void SetProfileImagePath(HttpSessionState session, string profileImagePath)
        {
            session["ProfileImagePath"] = profileImagePath ?? string.Empty;
        }

        public static void SignOut(HttpSessionState session)
        {
            session.Clear();
            session.Abandon();
        }

        /// <summary>
        /// Enforces that the current page is only reachable by an authenticated Learner.
        /// Must be called from Page_Load before any protected content is rendered.
        /// </summary>
        public static void RequireLearner(Page page)
        {
            HttpSessionState session = page.Session;

            if (!IsAuthenticated(session))
            {
                page.Response.Redirect("~/Login.aspx", true);
                return;
            }

            string role = CurrentRole(session);

            if (string.Equals(role, RoleAdministrator, StringComparison.Ordinal))
            {
                page.Response.Redirect("~/Admin/Dashboard.aspx", true);
                return;
            }

            if (!string.Equals(role, RoleLearner, StringComparison.Ordinal))
            {
                page.Response.Redirect("~/Login.aspx", true);
            }
        }

        /// <summary>
        /// Enforces that the current page is only reachable by an authenticated Administrator.
        /// Must be called from Page_Load before any protected content is rendered.
        /// </summary>
        public static void RequireAdministrator(Page page)
        {
            HttpSessionState session = page.Session;

            if (!IsAuthenticated(session))
            {
                page.Response.Redirect("~/Login.aspx", true);
                return;
            }

            string role = CurrentRole(session);

            if (string.Equals(role, RoleLearner, StringComparison.Ordinal))
            {
                page.Response.Redirect("~/Learner/Dashboard.aspx", true);
                return;
            }

            if (!string.Equals(role, RoleAdministrator, StringComparison.Ordinal))
            {
                page.Response.Redirect("~/Login.aspx", true);
            }
        }
    }
}
