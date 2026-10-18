using System.Web;
using System.Web.SessionState;
using System.Web.UI.WebControls;

namespace MyKPPLearn.Common
{
    /// <summary>
    /// One-shot feedback message kept in Session so it survives a Post/Redirect/Get
    /// and is shown once. Also renders a message into an alert Label.
    /// </summary>
    public static class FlashMessage
    {
        private const string TextKey = "Flash.Text";
        private const string ErrorKey = "Flash.IsError";

        public static void Set(HttpSessionState session, string text, bool isError = false)
        {
            session[TextKey] = text;
            session[ErrorKey] = isError;
        }

        /// <summary>Shows and clears any pending message in the given label.</summary>
        public static void ShowPending(HttpSessionState session, Label label)
        {
            string text = session[TextKey] as string;

            if (text == null)
            {
                return;
            }

            bool isError = session[ErrorKey] is bool && (bool)session[ErrorKey];
            session.Remove(TextKey);
            session.Remove(ErrorKey);
            Show(label, text, isError);
        }

        /// <summary>A cautionary (amber) message: the action worked, but the administrator should know about a side effect.</summary>
        public static void ShowWarning(Label label, string text)
        {
            label.Text = HttpUtility.HtmlEncode(text);
            label.CssClass = "alert alert-warning";
            label.Visible = true;
        }

        public static void Show(Label label, string text, bool isError)
        {
            label.Text = HttpUtility.HtmlEncode(text);
            label.CssClass = isError ? "alert alert-error" : "alert alert-success";
            label.Visible = true;
        }
    }
}
