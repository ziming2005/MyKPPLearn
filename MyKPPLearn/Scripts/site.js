(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        var toggle = document.getElementById("navToggle");
        var nav = document.getElementById("mainNav");

        if (!toggle || !nav) {
            return;
        }

        toggle.addEventListener("click", function () {
            var isOpen = nav.classList.toggle("is-open");
            toggle.setAttribute("aria-expanded", isOpen ? "true" : "false");
        });

        nav.addEventListener("keydown", function (event) {
            if (event.key === "Escape" && nav.classList.contains("is-open")) {
                nav.classList.remove("is-open");
                toggle.setAttribute("aria-expanded", "false");
                toggle.focus();
            }
        });
    });
})();

// Sign-in / register forms: show-hide button on password fields, and a red border on invalid fields.
(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        var inputs = document.querySelectorAll(".auth-card input[type='password']");

        Array.prototype.forEach.call(inputs, function (input) {
            var wrap = document.createElement("div");
            wrap.className = "password-field";
            input.parentNode.insertBefore(wrap, input);
            wrap.appendChild(input);

            var button = document.createElement("button");
            button.type = "button";
            button.className = "password-field__toggle";
            button.textContent = "Show";
            button.setAttribute("aria-label", "Show password");
            button.addEventListener("click", function () {
                var show = input.type === "password";
                input.type = show ? "text" : "password";
                button.textContent = show ? "Hide" : "Show";
                button.setAttribute("aria-label", show ? "Hide password" : "Show password");
            });
            wrap.appendChild(button);
        });
    });

    // ASP.NET calls this after each validator runs; mirror the result on the input's border.
    var original = window.ValidatorUpdateDisplay;

    if (typeof original === "function") {
        window.ValidatorUpdateDisplay = function (val) {
            original.apply(this, arguments);

            var input = document.getElementById(val.controltovalidate);

            if (!input || !window.Page_Validators) { return; }

            var invalid = window.Page_Validators.some(function (v) {
                return v.controltovalidate === val.controltovalidate && v.isvalid === false;
            });

            input.classList.toggle("input-invalid", invalid);
        };
    }
})();
