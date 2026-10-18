/* Confirm modal for server-side actions.
   Add data-confirm="Message" to a button or link (optional: data-confirm-title, data-confirm-label).
   The click is held until the user confirms, then the original click (e.g. an ASP.NET postback) runs. */
(function () {
    "use strict";

    var overlay = null;
    var titleEl = null;
    var textEl = null;
    var okBtn = null;
    var cancelBtn = null;
    var pending = null;
    var lastFocus = null;

    function build() {
        overlay = document.createElement("div");
        overlay.className = "modal-overlay";
        overlay.hidden = true;

        var panel = document.createElement("div");
        panel.className = "modal-panel card";
        panel.setAttribute("role", "alertdialog");
        panel.setAttribute("aria-modal", "true");
        panel.setAttribute("aria-labelledby", "confirmModalTitle");
        panel.setAttribute("aria-describedby", "confirmModalText");

        titleEl = document.createElement("h3");
        titleEl.id = "confirmModalTitle";
        textEl = document.createElement("p");
        textEl.id = "confirmModalText";

        var actions = document.createElement("div");
        actions.className = "form-actions";

        cancelBtn = document.createElement("button");
        cancelBtn.type = "button";
        cancelBtn.className = "btn btn-secondary";
        cancelBtn.textContent = "Cancel";

        okBtn = document.createElement("button");
        okBtn.type = "button";
        okBtn.className = "btn btn-secondary btn-danger";

        actions.appendChild(cancelBtn);
        actions.appendChild(okBtn);
        panel.appendChild(titleEl);
        panel.appendChild(textEl);
        panel.appendChild(actions);
        overlay.appendChild(panel);
        document.body.appendChild(overlay);

        cancelBtn.addEventListener("click", close);
        overlay.addEventListener("click", function (e) { if (e.target === overlay) { close(); } });
        document.addEventListener("keydown", function (e) {
            if (!overlay.hidden && e.key === "Escape") { close(); }
        });
        okBtn.addEventListener("click", function () {
            var target = pending;
            close(true);
            if (target) {
                target.setAttribute("data-confirmed", "1");
                target.click();
            }
        });
    }

    function close(keepFocus) {
        overlay.hidden = true;
        pending = null;
        if (keepFocus !== true && lastFocus && lastFocus.focus) { lastFocus.focus(); }
    }

    document.addEventListener("click", function (e) {
        var el = e.target.closest ? e.target.closest("[data-confirm]") : null;

        if (!el) { return; }

        // A disabled control does nothing, so it must not ask for confirmation either.
        if (el.disabled || el.getAttribute("aria-disabled") === "true" || el.classList.contains("aspNetDisabled")) {
            e.preventDefault();
            e.stopPropagation();
            return;
        }

        if (el.getAttribute("data-confirmed") === "1") {
            el.removeAttribute("data-confirmed");
            return;
        }

        e.preventDefault();
        e.stopPropagation();

        if (!overlay) { build(); }

        pending = el;
        lastFocus = el;
        titleEl.textContent = el.getAttribute("data-confirm-title") || "Delete this item?";
        textEl.textContent = el.getAttribute("data-confirm");
        okBtn.textContent = el.getAttribute("data-confirm-label") || "Delete";
        overlay.hidden = false;
        cancelBtn.focus();
    }, true);
})();
