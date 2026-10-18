using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MyKPPLearn.Common
{
    /// <summary>Helpers for safely rendering admin-entered lesson text.</summary>
    public static class LearningText
    {
        // Matches **bold** only; never *, _, __, HTML, or any other markup.
        private static readonly Regex BoldPattern = new Regex(@"\*\*(.+?)\*\*", RegexOptions.Singleline);

        /// <summary>
        /// HTML-encodes plain text, turns blank-line separated blocks into paragraphs and single
        /// line breaks into &lt;br /&gt;, then converts the plain-text **bold** syntax into
        /// &lt;strong&gt; tags. The text is always HTML-encoded first, so no admin-supplied markup
        /// other than the controlled **bold** syntax is ever rendered.
        /// </summary>
        public static string ToSafeHtml(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            string normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
            string[] paragraphs = normalized.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder html = new StringBuilder();

            foreach (string paragraph in paragraphs)
            {
                string trimmed = paragraph.Trim('\n', ' ', '\t');

                if (trimmed.Length == 0)
                {
                    continue;
                }

                string encoded = ApplyBold(HttpUtility.HtmlEncode(trimmed));
                html.Append("<p>")
                    .Append(encoded.Replace("\n", "<br />"))
                    .Append("</p>");
            }

            return html.ToString();
        }

        /// <summary>
        /// Replaces **text** with &lt;strong&gt;text&lt;/strong&gt;. Must only ever be called on
        /// text that has already been HTML-encoded, so the captured text can never introduce markup
        /// beyond the &lt;strong&gt; tag added here. Unmatched/malformed markers (e.g. "**Incomplete")
        /// are left exactly as typed.
        /// </summary>
        private static string ApplyBold(string encodedText)
        {
            return BoldPattern.Replace(encodedText, "<strong>$1</strong>");
        }

        public static string Truncate(string text, int maxLength)
        {
            if (text == null || text.Length <= maxLength)
            {
                return text;
            }

            return text.Substring(0, maxLength).TrimEnd() + "...";
        }
    }
}
