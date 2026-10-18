using System.Web.UI.WebControls;

namespace MyKPPLearn.Common
{
    /// <summary>
    /// Configures the Active / Inactive status action used in the Admin Learning tables.
    /// Visible text is the current state; the accessible name says what a click will do.
    /// </summary>
    public static class StatusToggle
    {
        public static void Apply(LinkButton button, bool isActive, string recordName)
        {
            string action = (isActive ? "Deactivate " : "Activate ") + recordName;

            button.Text = isActive ? "Active" : "Inactive";
            button.CssClass = isActive
                ? "btn btn-secondary btn-small btn-status btn-status--active"
                : "btn btn-secondary btn-small btn-status btn-status--inactive";
            button.ToolTip = action;
            button.Attributes["aria-label"] = button.Text + ". " + action;
        }
    }
}
