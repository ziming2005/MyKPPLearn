/*
 * Admin Learner Records UI (list + detail). Works on MOCK data from learner-records-demo.js only; nothing is read from or written to
 * the database. Text from the data is only ever written with textContent.
 */
(function () {
    "use strict";

    var Data = window.LearnerRecordsData;
    if (!Data) { return; }

    function el(tag, className, text) {
        var node = document.createElement(tag);
        if (className) { node.className = className; }
        if (text !== undefined && text !== null) { node.textContent = text; }
        return node;
    }

    function avatar(name, small) {
        return el("span", "lr-avatar" + (small ? " lr-avatar--sm" : ""), Data.initials(name));
    }

    // ------------------------------------------------------------------
    // List page: search + pagination
    // ------------------------------------------------------------------
    (function initList() {
        var root = document.getElementById("lrList");
        if (!root) { return; }

        var searchInput = document.getElementById("lrSearch");
        var statusFilter = document.getElementById("lrStatusFilter");
        var tableWrap = document.getElementById("lrTableWrap");
        var tbody = document.getElementById("lrTableBody");
        var empty = document.getElementById("lrEmpty");
        var footer = document.getElementById("lrFooter");
        var summary = document.getElementById("lrSummary");
        var pager = document.getElementById("lrPager");
        var toast = document.getElementById("lrToast");
        var toastTimer = null;

        var all = Data.getAll();
        var filtered = all;
        var page = 1;

        // ---- modals (Edit learner, and one confirmation dialog for Disable / Enable / Delete) ----
        var editModal = document.getElementById("lrEditModal");
        var editName = document.getElementById("lrEditName");
        var editEmail = document.getElementById("lrEditEmail");
        var editError = document.getElementById("lrEditError");
        var editSave = document.getElementById("lrEditSave");
        var confirmModal = document.getElementById("lrConfirmModal");
        var confirmTitle = document.getElementById("lrConfirmTitle");
        var confirmText = document.getElementById("lrConfirmText");
        var confirmOk = document.getElementById("lrConfirmOk");

        var openModalEl = null;
        var returnFocus = null;
        var editingId = null;
        var pendingConfirm = null;      // function run when the confirmation is accepted

        function showToast(text) {
            toast.textContent = text;
            toast.hidden = false;
            window.clearTimeout(toastTimer);
            toastTimer = window.setTimeout(function () { toast.hidden = true; }, 4000);
        }

        function openModal(modal, focusEl) {
            returnFocus = document.activeElement;
            openModalEl = modal;
            modal.hidden = false;
            if (focusEl) { focusEl.focus(); }
        }

        function closeModal() {
            if (!openModalEl) { return; }
            openModalEl.hidden = true;
            openModalEl = null;
            pendingConfirm = null;
            editingId = null;
            if (returnFocus && returnFocus.focus && document.contains(returnFocus)) { returnFocus.focus(); }
        }

        [editModal, confirmModal].forEach(function (modal) {
            modal.addEventListener("click", function (e) { if (e.target === modal) { closeModal(); } });
            Array.prototype.forEach.call(modal.querySelectorAll("[data-close]"), function (btn) { btn.addEventListener("click", closeModal); });
        });

        document.addEventListener("keydown", function (e) {
            if (!openModalEl) { return; }

            if (e.key === "Escape") { e.preventDefault(); closeModal(); return; }

            if (e.key === "Tab") {
                var items = Array.prototype.filter.call(openModalEl.querySelectorAll("button, input"), function (x) { return !x.disabled && x.offsetParent !== null; });
                if (!items.length) { return; }
                var first = items[0], last = items[items.length - 1];
                if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
                else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
            }
        });

        // ---- list state ----
        function refresh() {
            all = Data.getAll();
            applyFilter();
            page = Math.min(page, pageCount());      // e.g. after deleting the last row of the last page
            render();
        }

        function applyFilter() {
            var term = searchInput.value.trim().toLowerCase();
            var status = statusFilter ? statusFilter.value : "";

            filtered = all.filter(function (l) {
                return (!term || l.name.toLowerCase().indexOf(term) !== -1 || l.email.toLowerCase().indexOf(term) !== -1)
                    && (!status || l.status === status);
            });
        }

        function pageCount() { return Math.max(1, Math.ceil(filtered.length / Data.pageSize)); }

        function goTo(target) {
            page = Math.min(Math.max(1, target), pageCount());
            render();
        }

        function pagerButton(label, target, options) {
            var button = el("button", "lr-pager__btn" + (options.active ? " is-active" : ""), label);
            button.type = "button";
            if (options.disabled) { button.disabled = true; }
            if (options.active) { button.setAttribute("aria-current", "page"); }
            if (options.ariaLabel) { button.setAttribute("aria-label", options.ariaLabel); }
            button.addEventListener("click", function () { goTo(target); });
            return button;
        }

        function renderPager() {
            pager.textContent = "";
            var pages = pageCount();

            pager.appendChild(pagerButton("Previous", page - 1, { disabled: page === 1 }));

            for (var i = 1; i <= pages; i++) {
                pager.appendChild(pagerButton(String(i), i, { active: i === page, ariaLabel: "Page " + i }));
            }

            pager.appendChild(pagerButton("Next", page + 1, { disabled: page === pages }));
        }

        function actionButton(label, className, handler) {
            var button = el("button", "btn btn-secondary btn-small " + className, label);
            button.type = "button";
            button.addEventListener("click", handler);
            return button;
        }

        // ---- prototype actions (in-memory only) ----
        function openEdit(learner) {
            editingId = learner.id;
            editName.value = learner.name;
            editEmail.value = learner.email;
            editError.textContent = "";
            editError.hidden = true;
            openModal(editModal, editName);
        }

        function saveEdit() {
            var name = editName.value.trim();
            var email = editEmail.value.trim();
            var message = "";

            if (!name) { message = "Full name is required."; }
            else if (!email) { message = "Email is required."; }
            else if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email)) { message = "Enter a valid email address."; }
            else if (Data.getAll().some(function (l) { return l.id !== editingId && l.email.toLowerCase() === email.toLowerCase(); })) {
                message = "Another learner already uses this email.";
            }

            if (message) {
                editError.textContent = message;
                editError.hidden = false;
                return;
            }

            Data.updateLearner(editingId, name, email);
            closeModal();
            refresh();
            showToast("Learner updated successfully.");
        }

        function askConfirm(options) {
            confirmTitle.textContent = options.title;
            confirmText.textContent = options.text;
            confirmOk.textContent = options.okLabel;
            confirmOk.className = "btn " + options.okClass;
            pendingConfirm = options.onConfirm;
            openModal(confirmModal, confirmOk);
        }

        function disableLearner(learner) {
            askConfirm({
                title: "Disable Learner?",
                text: "This learner will be marked as disabled in this UI prototype.",
                okLabel: "Disable Learner",
                okClass: "btn-primary",
                onConfirm: function () { Data.setStatus(learner.id, "Disabled"); refresh(); showToast("Learner disabled."); }
            });
        }

        function enableLearner(learner) {
            askConfirm({
                title: "Enable Learner?",
                text: "This learner will be marked as active again in this UI prototype.",
                okLabel: "Enable Learner",
                okClass: "btn-primary",
                onConfirm: function () { Data.setStatus(learner.id, "Active"); refresh(); showToast("Learner enabled."); }
            });
        }

        function deleteLearner(learner) {
            askConfirm({
                title: "Delete Learner?",
                text: "Are you sure you want to remove this learner from the prototype list?",
                okLabel: "Delete Learner",
                okClass: "btn-secondary btn-danger",
                onConfirm: function () { Data.remove(learner.id); refresh(); showToast("Learner removed from the prototype list."); }
            });
        }

        editSave.addEventListener("click", saveEdit);

        confirmOk.addEventListener("click", function () {
            var action = pendingConfirm;
            closeModal();
            if (action) { action(); }
        });

        function render() {
            tbody.textContent = "";

            if (!filtered.length) {
                tableWrap.hidden = true;
                footer.hidden = true;
                empty.hidden = false;
                return;
            }

            tableWrap.hidden = false;
            footer.hidden = false;
            empty.hidden = true;

            var start = (page - 1) * Data.pageSize;
            var slice = filtered.slice(start, start + Data.pageSize);

            slice.forEach(function (learner) {
                var row = document.createElement("tr");
                var disabled = learner.status === "Disabled";

                var nameCell = document.createElement("td");
                var who = el("div", "lr-learner");
                who.appendChild(avatar(learner.name, true));
                who.appendChild(el("strong", "", learner.name));
                nameCell.appendChild(who);
                row.appendChild(nameCell);

                var emailCell = el("td", "data-table__muted lr-email", learner.email);
                emailCell.title = learner.email;          // the full address stays available when the cell is truncated
                row.appendChild(emailCell);

                var statusCell = document.createElement("td");
                // Same rectangular status button as the Learning Chapters Status column; clicking it asks to disable / enable (mock only).
                var statusBtn = el("button", "btn btn-secondary btn-small btn-status " + (disabled ? "lr-status--disabled" : "btn-status--active"), learner.status);
                statusBtn.type = "button";
                statusBtn.title = (disabled ? "Enable " : "Disable ") + learner.name;
                statusBtn.setAttribute("aria-label", learner.status + ". " + (disabled ? "Enable" : "Disable") + " learner " + learner.name);
                statusBtn.addEventListener("click", function () { if (disabled) { enableLearner(learner); } else { disableLearner(learner); } });
                statusCell.appendChild(statusBtn);
                row.appendChild(statusCell);

                row.appendChild(el("td", "data-table__nowrap", Data.formatDate(learner.joined, true)));

                var actions = document.createElement("td");
                var group = el("div", "lr-actions");

                var view = el("a", "btn btn-secondary btn-small", "View");
                view.href = "LearnerRecordDetails.aspx?id=" + encodeURIComponent(learner.id);
                view.setAttribute("aria-label", "View " + learner.name);
                group.appendChild(view);

                group.appendChild(actionButton("Edit", "", function () { openEdit(learner); }));

                group.appendChild(actionButton("Delete", "btn-danger", function () { deleteLearner(learner); }));

                actions.appendChild(group);
                row.appendChild(actions);

                tbody.appendChild(row);
            });

            summary.textContent = "Showing " + (start + 1) + "\u2013" + (start + slice.length) + " of " + filtered.length
                + (filtered.length === 1 ? " learner" : " learners");

            renderPager();
        }

        searchInput.addEventListener("input", function () {
            applyFilter();
            page = 1;          // a new search always starts on the first page
            render();
        });

        if (statusFilter) {
            statusFilter.addEventListener("change", function () {
                applyFilter();
                page = 1;
                render();
            });
        }

        render();
    })();

    // ------------------------------------------------------------------
    // Detail page
    // ------------------------------------------------------------------
    (function initDetail() {
        var root = document.getElementById("lrDetail");
        if (!root) { return; }

        var match = /[?&]id=([^&]*)/.exec(window.location.search);
        var learner = match ? Data.getById(decodeURIComponent(match[1])) : null;

        // Missing / unknown id: show a "not found" notice (with a link back to the list) instead of the empty page.
        if (!learner) {
            root.hidden = true;
            document.getElementById("lrNotFound").hidden = false;
            return;
        }

        function byId(id) { return document.getElementById(id); }

        // ---- Learner Information ----
        var photo = byId("lrInfoAvatar");
        photo.textContent = Data.initials(learner.name);
        byId("lrInfoName").textContent = learner.name;
        byId("lrInfoEmail").textContent = learner.email;
        var statusBadge = byId("lrInfoStatus");
        statusBadge.textContent = learner.status;
        statusBadge.className = "status-badge " + (learner.status === "Disabled" ? "status-badge--inactive" : "status-badge--active");
        byId("lrInfoCreated").textContent = Data.formatDate(learner.joined, false);

        // ---- Overview ----
        var done = Data.lessonsCompleted(learner);
        var total = Data.lessonsTotal(learner);
        byId("lrOvLessons").textContent = done + " / " + total;
        byId("lrOvPractice").textContent = String(learner.practice.length);
        byId("lrOvMock").textContent = String(learner.mock.length);
        byId("lrOvGames").textContent = String(learner.games.length);

        // ---- Learning Progress ----
        // Same states as the learner Progress page: a finished chapter is green with a tick, a chapter without lessons says so.
        function progressBlock(title, completed, totalLessons, extraClass, ariaLabel) {
            var pct = Data.percent(completed, totalLessons);
            var isChapter = !!extraClass;
            var empty = isChapter && totalLessons <= 0;
            var finished = isChapter && totalLessons > 0 && completed >= totalLessons;
            var block = el("div", "lr-progress-block" + (extraClass ? " " + extraClass : "") + (finished ? " lr-progress-block--done" : ""));

            var head = el("div", "lr-progress-block__head");
            head.appendChild(el("span", "lr-progress-block__title", title));
            head.appendChild(el("span", "lr-progress-block__percent", (finished ? "✓ " : "") + pct + "%"));
            block.appendChild(head);

            block.appendChild(el("div", "lr-progress-block__meta", empty ? "No lessons yet" : completed + " / " + totalLessons + " Lessons Completed"));

            var track = el("div", "progress-track");
            track.setAttribute("role", "progressbar");
            track.setAttribute("aria-valuemin", "0");
            track.setAttribute("aria-valuemax", "100");
            track.setAttribute("aria-valuenow", String(pct));
            track.setAttribute("aria-label", ariaLabel);
            var fill = el("div", "progress-track__fill");
            fill.style.width = pct + "%";
            track.appendChild(fill);
            block.appendChild(track);

            return block;
        }

        var learning = byId("lrLearning");
        learning.appendChild(progressBlock("Overall Learning Progress", done, total, "", "Overall learning progress"));
        if (!learner.chapters.length) {
            learning.appendChild(el("p", "lr-note", "No learning content is available yet."));
        }
        learner.chapters.forEach(function (chapter) {
            learning.appendChild(progressBlock("Chapter " + chapter.number + " \u2014 " + chapter.title, chapter.completed, chapter.total, "lr-progress-block--chapter", "Chapter progress"));
        });

        // ---- history lists ----
        // Like the learner Progress page: the 3 most recent are shown, older ones sit behind a View All / Show Less button.
        var RECENT = 3;
        var listCount = 0;

        function historyList(containerId, items, emptyText, build, summaryText) {
            var container = byId(containerId);

            if (!items.length) {
                container.appendChild(el("p", "lr-note", emptyText));
                return;
            }

            if (summaryText) { container.appendChild(el("div", "lr-history-summary", summaryText)); }

            var listId = containerId + "List";
            var list = el("ul", "lr-history");
            list.id = listId;
            items.forEach(function (item, index) {
                var li = el("li", "lr-history__item" + (index >= RECENT ? " lr-history__item--extra" : ""));
                build(li, item);
                list.appendChild(li);
            });
            container.appendChild(list);

            if (items.length > RECENT) {
                var more = el("div", "lr-history-more");
                var toggle = el("button", "btn btn-secondary", "View All");
                toggle.type = "button";
                toggle.setAttribute("aria-expanded", "false");
                toggle.setAttribute("aria-controls", listId);
                toggle.addEventListener("click", function () {
                    var expanded = list.classList.toggle("is-expanded");
                    toggle.setAttribute("aria-expanded", expanded ? "true" : "false");
                    toggle.textContent = expanded ? "Show Less" : "View All";
                });
                more.appendChild(toggle);
                container.appendChild(more);
            }
        }

        // "N games played \u00b7 Best: x / y \u00b7 Average: z%" (same wording and maths as the learner page).
        function gameSummary(games) {
            var best = games[0], ratioSum = 0;
            games.forEach(function (g) {
                var ratio = g.total > 0 ? g.correct / g.total : 0;
                ratioSum += ratio;
                if (ratio > (best.total > 0 ? best.correct / best.total : 0)) { best = g; }
            });
            return games.length + (games.length === 1 ? " game played" : " games played")
                + " \u00b7 Best: " + best.correct + " / " + best.total
                + " \u00b7 Average: " + Math.round(ratioSum / games.length * 100) + "%";
        }

        historyList("lrPractice", learner.practice, "No Practice attempts yet.", function (li, a) {
            var main = el("div", "lr-history__main");
            main.appendChild(el("div", "lr-history__title", a.category + " \u00B7 Practice Set " + a.setNumber));
            main.appendChild(el("div", "lr-history__line", a.correct + " / " + a.total + " Correct"));
            main.appendChild(el("div", "lr-history__date", Data.formatDate(a.date, false)));
            li.appendChild(main);
            li.appendChild(el("div", "lr-history__score", Data.percent(a.correct, a.total) + "%"));
        });

        historyList("lrMock", learner.mock, "No Mock Exams completed yet.", function (li, a) {
            var main = el("div", "lr-history__main");
            main.appendChild(el("div", "lr-history__title", a.examType + " \u00B7 Mock Exam Set " + a.setNumber));
            main.appendChild(el("div", "lr-history__line", "Colour Blind: " + a.colourBlind + " / " + a.colourBlindTotal));
            main.appendChild(el("div", "lr-history__line", "Theory: " + a.theory + " / " + a.theoryTotal));
            main.appendChild(el("div", "lr-history__date", Data.formatDate(a.date, false)));
            li.appendChild(main);
            li.appendChild(el("span", "status-badge " + (a.passed ? "status-badge--active" : "status-badge--error"), a.passed ? "PASS" : "FAIL"));
        });

        historyList("lrGames", learner.games, "No Road Sign Games completed yet.", function (li, a) {
            var main = el("div", "lr-history__main");
            main.appendChild(el("div", "lr-history__title", "Road Sign Game Set " + a.setNumber));
            main.appendChild(el("div", "lr-history__line", a.correct + " / " + a.total + " Questions Correct"));
            main.appendChild(el("div", "lr-history__date", Data.formatDate(a.date, false)));
            li.appendChild(main);
        }, learner.games.length ? gameSummary(learner.games) : "");
    })();
})();
