/*
 * "Used Numbers: 1, 2, 4" helper text for the Admin number fields. Informational only: it lists numbers that are
 * already in use (ascending) and never suggests, fills in or validates a number.
 */
(function (global) {
    "use strict";

    global.UsedNumbers = {
        format: function (numbers) {
            var seen = {};
            var list = (numbers || []).filter(function (n) {
                if (seen[n]) { return false; }
                seen[n] = true;
                return true;
            }).sort(function (a, b) { return a - b; });

            return "Used Numbers: " + (list.length ? list.join(", ") : "None");
        }
    };
})(window);
