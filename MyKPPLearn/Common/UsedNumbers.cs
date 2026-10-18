using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace MyKPPLearn.Common
{
    /// <summary>
    /// The "Used Numbers: 1, 2, 4" helper shown under the Admin number fields. Informational only: it lists numbers that
    /// already exist (ascending) and never suggests or fills in a number. The duplicate checks stay in the save code.
    /// </summary>
    public static class UsedNumbers
    {
        public static string Format(IEnumerable<int> numbers)
        {
            List<int> sorted = (numbers ?? Enumerable.Empty<int>()).Distinct().OrderBy(n => n).ToList();

            return "Used Numbers: " + (sorted.Count == 0
                ? "None"
                : string.Join(", ", sorted.Select(n => n.ToString(CultureInfo.InvariantCulture))));
        }

        /// <summary>A scope-to-numbers map as an HTML-attribute-safe JSON string (used when the scope changes in the browser).</summary>
        public static string MapAttribute(IDictionary<int, List<int>> map)
        {
            Dictionary<string, List<int>> plain = new Dictionary<string, List<int>>();

            foreach (KeyValuePair<int, List<int>> pair in map)
            {
                plain[pair.Key.ToString(CultureInfo.InvariantCulture)] = pair.Value;
            }

            return HttpUtility.HtmlAttributeEncode(new JavaScriptSerializer().Serialize(plain));
        }
    }
}
