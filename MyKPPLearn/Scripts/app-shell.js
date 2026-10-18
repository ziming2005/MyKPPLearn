(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        var toggle = document.getElementById("drawerToggle");
        var sidebar = document.getElementById("appSidebar");
        var backdrop = document.getElementById("appDrawerBackdrop");

        function isDrawerBreakpoint() {
            return window.matchMedia("(max-width: 1199px)").matches;
        }

        function openDrawer() {
            sidebar.classList.add("is-open");
            backdrop.classList.add("is-open");
            toggle.setAttribute("aria-expanded", "true");
        }

        function closeDrawer() {
            sidebar.classList.remove("is-open");
            backdrop.classList.remove("is-open");
            toggle.setAttribute("aria-expanded", "false");
        }

        if (toggle && sidebar && backdrop) {
            toggle.addEventListener("click", function () {
                if (sidebar.classList.contains("is-open")) {
                    closeDrawer();
                } else {
                    openDrawer();
                }
            });

            backdrop.addEventListener("click", closeDrawer);

            document.addEventListener("keydown", function (event) {
                if (event.key === "Escape" && sidebar.classList.contains("is-open")) {
                    closeDrawer();
                    toggle.focus();
                }
            });

            var navLinks = sidebar.querySelectorAll(".app-nav-link");
            for (var i = 0; i < navLinks.length; i++) {
                navLinks[i].addEventListener("click", function () {
                    if (isDrawerBreakpoint()) {
                        closeDrawer();
                    }
                });
            }
        }
    });
})();
