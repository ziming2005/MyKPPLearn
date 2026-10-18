/* Copies each column heading onto its cells as data-label, so the stacked mobile layout of
   .data-table can print the label before each value. Works for server-rendered tables and for
   tables whose rows are built by script (a MutationObserver relabels them when rows change). */
(function () {
    "use strict";

    function label(table) {
        var heads = table.querySelectorAll("thead th");

        if (!heads.length) { return; }

        var names = Array.prototype.map.call(heads, function (th) { return th.textContent.trim(); });

        Array.prototype.forEach.call(table.querySelectorAll("tbody tr"), function (row) {
            Array.prototype.forEach.call(row.children, function (cell, index) {
                if (cell.tagName === "TD" && !cell.hasAttribute("data-label") && names[index]) {
                    cell.setAttribute("data-label", names[index]);
                }
            });
        });
    }

    function init() {
        Array.prototype.forEach.call(document.querySelectorAll("table.data-table"), function (table) {
            label(table);

            if (window.MutationObserver) {
                new MutationObserver(function () { label(table); }).observe(table, { childList: true, subtree: true });
            }
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
