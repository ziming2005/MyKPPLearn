/*
 * Profile page (Learner and Admin): the avatar opens the file chooser; choosing a picture uploads it straight away.
 *  - The picture is checked here first (type, 2 MB) for instant feedback; the server re-checks everything.
 *  - A valid file shows a local preview and "Uploading..." and then clicks the hidden Upload button, which posts only the
 *    picture flow (not the name, not the passwords). The server saves it, the page reloads and the header shows it.
 *  - If the server rejects it, the page reloads with the previous picture still in place and an error message.
 * A saved picture can be replaced or removed (Remove photo posts on the server). Full Name has its own Save Changes button,
 * which stays disabled until the name is actually changed.
 * Text from the page is only written with textContent.
 */
(function () {
    "use strict";

    var root = document.querySelector(".profile-editor");
    if (!root) { return; }

    var MAX_BYTES = 2 * 1024 * 1024;

    var fileInput = root.querySelector(".profile-photo-input");
    var uploadBtn = root.querySelector('input[id$="btnUploadPhoto"]');
    var statusEl = root.querySelector(".profile-photo-status");
    var errorEl = root.querySelector(".profile-photo-error");
    var avatar = root.querySelector(".profile-avatar");
    var avatarImg = root.querySelector(".profile-avatar__img");
    var initialsEl = root.querySelector(".profile-avatar__initials");
    var nameInput = root.querySelector('input[id$="txtFullName"]');

    var hasPhoto = root.getAttribute("data-has-photo") === "1";   // a saved picture exists: initials are only for accounts without one
    var uploading = false;

    function initialsOf(name) {
        var parts = (name || "").trim().split(/\s+/).filter(Boolean);
        if (!parts.length) { return "?"; }
        if (parts.length === 1) { return parts[0].charAt(0).toUpperCase(); }
        return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
    }

    function showError(text) {
        if (!errorEl) { return; }
        errorEl.textContent = text || "";
        errorEl.hidden = !text;
    }

    function render() {
        avatarImg.hidden = !hasPhoto;
        initialsEl.hidden = hasPhoto;
    }

    // Save Changes is only available once the name differs from the saved one.
    var saveBtn = root.querySelector('input[id$="btnSaveProfile"]');

    function syncSave() {
        if (!saveBtn || !nameInput) { return; }
        saveBtn.disabled = nameInput.value.trim() === (nameInput.getAttribute("data-initial") || "").trim();
    }

    if (nameInput) { nameInput.addEventListener("input", syncSave); }
    syncSave();

    // While there is no picture the avatar shows the initials of the name being typed.
    if (nameInput) {
        nameInput.addEventListener("input", function () {
            if (!hasPhoto && initialsEl) { initialsEl.textContent = initialsOf(nameInput.value); }
        });
    }

    if (fileInput && uploadBtn) {
        fileInput.addEventListener("change", function () {
            var file = fileInput.files && fileInput.files[0];
            showError("");

            if (!file || uploading) { return; }

            if (!/\.(jpe?g|png|webp)$/i.test(file.name || "")) {
                showError("Please select a JPG, JPEG, PNG or WEBP image.");
                fileInput.value = "";
                return;
            }

            if (file.size <= 0 || file.size > MAX_BYTES) {
                showError("Profile image must be 2 MB or smaller.");
                fileInput.value = "";
                return;
            }

            // Valid: preview it and upload at once. Further choices are ignored until the page reloads.
            uploading = true;
            avatarImg.src = URL.createObjectURL(file);
            hasPhoto = true;
            render();
            if (avatar) { avatar.classList.add("is-uploading"); avatar.setAttribute("aria-busy", "true"); }
            if (statusEl) { statusEl.hidden = false; }
            uploadBtn.click();
        });
    }

    // The avatar (a label for the file input) opens the file chooser on click; make Enter / Space do the same.
    if (avatar && fileInput) {
        avatar.addEventListener("keydown", function (e) {
            if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                if (!uploading) { fileInput.click(); }
            }
        });
    }

    // ------------------------------------------------------------------
    // Change Password modal
    // ------------------------------------------------------------------
    var modal = root.querySelector(".profile-password-modal");
    var openBtn = root.querySelector(".profile-open-password");

    if (modal && openBtn) {
        var panel = modal.querySelector(".modal-panel");
        var closeBtn = modal.querySelector(".profile-modal__close");
        var cancelBtn = modal.querySelector(".profile-modal__cancel");
        var passwordInputs = modal.querySelectorAll('input[type="password"]');
        var modalMessage = modal.querySelector('[id$="lblPasswordMessage"]');

        // Show / Hide button inside each password field.
        Array.prototype.forEach.call(passwordInputs, function (input) {
            var wrap = document.createElement("div");
            wrap.className = "password-field";
            input.parentNode.insertBefore(wrap, input);
            wrap.appendChild(input);

            var toggle = document.createElement("button");
            toggle.type = "button";
            toggle.className = "password-field__toggle";
            toggle.textContent = "Show";
            toggle.setAttribute("aria-label", "Show password");
            toggle.addEventListener("click", function () {
                var show = input.type === "password";
                input.type = show ? "text" : "password";
                toggle.textContent = show ? "Hide" : "Show";
                toggle.setAttribute("aria-label", show ? "Hide password" : "Show password");
            });
            wrap.appendChild(toggle);
        });

        function focusables() {
            return Array.prototype.filter.call(
                modal.querySelectorAll("button, input:not([type=hidden]), a[href]"),
                function (el) { return !el.disabled && el.offsetParent !== null; });
        }

        function openModal() {
            modal.hidden = false;
            document.body.classList.add("profile-modal-open");
            if (passwordInputs.length) { passwordInputs[0].focus(); }
        }

        // Closing never submits anything: it only hides the dialog and clears what was typed and any messages.
        function closeModal() {
            if (modal.hidden) { return; }

            Array.prototype.forEach.call(passwordInputs, function (input) { input.value = ""; input.type = "password"; });
            Array.prototype.forEach.call(modal.querySelectorAll(".password-field__toggle"), function (t) { t.textContent = "Show"; t.setAttribute("aria-label", "Show password"); });

            if (modalMessage) { modalMessage.hidden = true; modalMessage.textContent = ""; }

            if (window.Page_Validators) {
                window.Page_Validators.forEach(function (validator) {
                    if (modal.contains(validator)) {
                        validator.isvalid = true;
                        if (window.ValidatorUpdateDisplay) { window.ValidatorUpdateDisplay(validator); }
                    }
                });
            }

            modal.hidden = true;
            modal.removeAttribute("data-open");
            document.body.classList.remove("profile-modal-open");
            openBtn.focus();
        }

        openBtn.addEventListener("click", openModal);
        if (closeBtn) { closeBtn.addEventListener("click", closeModal); }
        if (cancelBtn) { cancelBtn.addEventListener("click", closeModal); }

        modal.addEventListener("click", function (e) { if (e.target === modal) { closeModal(); } });

        document.addEventListener("keydown", function (e) {
            if (modal.hidden) { return; }

            if (e.key === "Escape") {
                e.preventDefault();
                closeModal();
                return;
            }

            if (e.key === "Tab") {
                var items = focusables();
                if (!items.length) { return; }

                var first = items[0], last = items[items.length - 1];

                if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
                else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
            }
        });

        // A rejected attempt comes back from the server with the message inside the modal: reopen it.
        if (modal.getAttribute("data-open") === "1") { openModal(); }
    }

    render();
})();
