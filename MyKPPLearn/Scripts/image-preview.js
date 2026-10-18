/*
 * Admin image upload preview. Any <input type="file"> carrying
 *   data-preview-img  = id of the <img> to update
 *   data-preview-wrap = id of the element to reveal
 * shows the newly chosen picture locally (nothing is uploaded until the form is saved).
 *
 * Optional, checked before anything is previewed (the server checks again on Save):
 *   data-max-bytes = largest allowed file size in bytes
 *   data-error-el  = id of the element that shows the problem
 * Only JPG, PNG and WEBP are accepted.
 */
(function () {
    "use strict";

    var ALLOWED = /\.(jpe?g|png|webp)$/i;

    var inputs = document.querySelectorAll("input[type='file'][data-preview-img]");

    Array.prototype.forEach.call(inputs, function (input) {
        var img = document.getElementById(input.getAttribute("data-preview-img"));
        var wrap = document.getElementById(input.getAttribute("data-preview-wrap"));
        var errorEl = document.getElementById(input.getAttribute("data-error-el") || "");
        var maxBytes = parseInt(input.getAttribute("data-max-bytes"), 10) || 0;
        var objectUrl = null;

        if (!img) { return; }

        function showProblem(message) {
            if (!errorEl) { return; }
            errorEl.textContent = message || "";
            errorEl.hidden = !message;
        }

        input.addEventListener("change", function () {
            var file = input.files && input.files[0];

            if (objectUrl) {
                URL.revokeObjectURL(objectUrl);
                objectUrl = null;
            }

            showProblem("");

            if (!file) {
                return;
            }

            if (!ALLOWED.test(file.name || "") || !/^image\//.test(file.type)) {
                input.value = "";
                showProblem("Choose a JPG, PNG or WEBP image.");
                return;
            }

            if (maxBytes && file.size > maxBytes) {
                input.value = "";
                showProblem("The image must be " + Math.round(maxBytes / (1024 * 1024)) + " MB or smaller.");
                return;
            }

            objectUrl = URL.createObjectURL(file);
            img.src = objectUrl;
            img.alt = "Selected image preview";

            if (wrap) {
                wrap.style.display = "block";
            }
        });
    });
})();
