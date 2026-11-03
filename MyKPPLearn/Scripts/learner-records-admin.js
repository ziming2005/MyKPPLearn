/*
 * Admin Learner Records UI (list + detail). Data comes from the database through the page methods wrapped by
 * learner-records-data.js. Text from the data is only ever written with textContent.
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
    // List page: search, status filter, pagination and the account actions
    // ------------------------------------------------------------------
    (function initList() {
        var root = document.getElementById("lrList");
        if (!root) { return; }

        var searchInput = document.getElementById("lrSearch");
        var statusFilter = document.getElementById("lrStatusFilter");
        var tableWrap = document.getElementById("lrTableWrap");
        var tbody = document.getElementById("lrTableBody");
        var empty = document.getElementById("lrEmpty");
        var emptyTitle = document.getElementById("lrEmptyTitle");
        var emptyClear = document.getElementById("lrEmptyClear");
        var footer = document.getElementById("lrFooter");
        var summary = document.getElementById("lrSummary");
        var pager = document.getElementById("lrPager");
        var toast = document.getElementById("lrToast");
        var toastTimer = null;

        var all = [];
        var filtered = [];
        var page = 1;
        var loaded = false;

        // ---- modals (Add / Edit learner, and one confirmation dialog for Disable / Enable / Delete) ----
        var editModal = document.getElementById("lrEditModal");
        var editTitle = document.getElementById("lrEditTitle");
        var editName = document.getElementById("lrEditName");
        var editEmail = document.getElementById("lrEditEmail");
        var emailHint = document.getElementById("lrEmailHint");
        var editPassword = document.getElementById("lrEditPassword");
        var passwordGroup = document.getElementById("lrPasswordGroup");
        var editError = document.getElementById("lrEditError");
        var editSave = document.getElementById("lrEditSave");
        var confirmModal = document.getElementById("lrConfirmModal");
        var confirmTitle = document.getElementById("lrConfirmTitle");
        var confirmText = document.getElementById("lrConfirmText");
        var confirmOk = document.getElementById("lrConfirmOk");

        var fieldErrors = {
            name: { input: editName, error: document.getElementById("lrNameError") },
            email: { input: editEmail, error: document.getElementById("lrEmailError") },
            password: { input: editPassword, error: document.getElementById("lrPasswordError") }
        };

        var openModalEl = null;
        var returnFocus = null;
        var editingId = null;           // null while adding a learner
        var pendingConfirm = null;      // function run when the confirmation is accepted
        var busy = false;

        function showToast(text, isError) {
            toast.textContent = text;
            toast.className = "alert alert-" + (isError ? "error" : "success");
            toast.hidden = false;
            window.clearTimeout(toastTimer);
            toastTimer = window.setTimeout(function () { toast.hidden = true; }, isError ? 8000 : 4000);
        }

        function fail(e) { showToast(e.message, true); }

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

            // Enter in a field saves the form instead of posting the whole ASP.NET page back.
            if (e.key === "Enter" && openModalEl === editModal && e.target.tagName === "INPUT") {
                e.preventDefault();
                saveEdit();
                return;
            }

            if (e.key === "Tab") {
                var items = Array.prototype.filter.call(openModalEl.querySelectorAll("button, input"), function (x) { return !x.disabled && x.offsetParent !== null; });
                if (!items.length) { return; }
                var first = items[0], last = items[items.length - 1];
                if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
                else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
            }
        });

        // ---- list state ----
        function load() {
            return Data.loadLearners().then(function (learners) {
                all = learners || [];
                loaded = true;
                applyFilter();
                page = Math.min(page, pageCount());      // e.g. after deleting the last row of the last page
                render();
            }).catch(fail);
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
            if (pages <= 1) { return; }

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

        // ---- Add / Edit ----
        function clearFieldErrors() {
            Object.keys(fieldErrors).forEach(function (key) {
                fieldErrors[key].input.removeAttribute("aria-invalid");
                fieldErrors[key].error.hidden = true;
                fieldErrors[key].error.textContent = "";
            });
            editError.hidden = true;
            editError.textContent = "";
        }

        /** Shows a message under its field ("name", "email", "password"), or under the form when there is no field. */
        function showFieldError(field, message) {
            var target = fieldErrors[field];

            if (!target) {
                editError.textContent = message;
                editError.hidden = false;
                return;
            }

            target.error.textContent = message;
            target.error.hidden = false;
            target.input.setAttribute("aria-invalid", "true");
            target.input.focus();
        }

        function openForm(learner) {
            clearFieldErrors();
            editingId = learner ? learner.id : null;
            editTitle.textContent = learner ? "Edit Learner" : "Add Learner";
            editSave.textContent = learner ? "Save Changes" : "Add Learner";
            editName.value = learner ? learner.name : "";
            editEmail.value = learner ? learner.email : "";
            editEmail.readOnly = !!learner;        // the email is the learner's sign-in, so it is fixed once the account exists
            emailHint.hidden = !learner;
            editPassword.value = "";
            passwordGroup.hidden = !!learner;      // a password is only chosen when the account is created
            openModal(editModal, editName);
        }

        /** The same rules as the server (and the Register page); returns the first problem or null. */
        function validate(name, email, password, adding) {
            if (!name) { return { field: "name", error: "Full name is required." }; }
            if (name.length > 150) { return { field: "name", error: "Full name must be 150 characters or fewer." }; }
            if (!email) { return { field: "email", error: "Email is required." }; }
            if (email.length > 254 || !/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email)) { return { field: "email", error: "Enter a valid email address." }; }
            if (all.some(function (l) { return l.id !== editingId && l.email.toLowerCase() === email.toLowerCase(); })) {
                return { field: "email", error: "Another learner already uses this email." };
            }
            if (adding && !/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/.test(password)) {
                return { field: "password", error: "Password must be at least 8 characters and include an uppercase letter, lowercase letter, and number." };
            }
            return null;
        }

        function saveEdit() {
            if (busy) { return; }

            var adding = editingId === null;
            var name = editName.value.trim();
            var email = editEmail.value.trim();
            var password = editPassword.value;

            clearFieldErrors();

            var problem = validate(name, email, password, adding);
            if (problem) { showFieldError(problem.field, problem.error); return; }

            busy = true;
            editSave.disabled = true;

            var request = adding ? Data.create(name, email, password) : Data.update(editingId, name);

            request.then(function (result) {
                if (result && result.error) {
                    showFieldError(result.field, result.error);
                    return;
                }

                closeModal();
                load();
                showToast(adding ? "Learner added successfully." : "Learner updated successfully.");
            }).catch(function (e) {
                showFieldError(null, e.message);
            }).then(function () {
                busy = false;
                editSave.disabled = false;
            });
        }

        // ---- Disable / Enable / Delete ----
        function askConfirm(options) {
            confirmTitle.textContent = options.title;
            confirmText.textContent = options.text;
            confirmOk.textContent = options.okLabel;
            confirmOk.className = "btn " + options.okClass;
            pendingConfirm = options.onConfirm;
            openModal(confirmModal, confirmOk);
        }

        /** Runs a server action; a result with { error } or a message other than "Deleted" is shown as an error. */
        function runAction(request, successText) {
            request.then(function (result) {
                if (result && result.error) { throw new Error(result.error); }
                if (typeof result === "string" && result !== "Deleted") { throw new Error(result); }
                load();
                showToast(successText);
            }).catch(fail);
        }

        function disableLearner(learner) {
            askConfirm({
                title: "Disable Learner?",
                text: learner.name + " will not be able to sign in until the account is enabled again. Their progress and results are kept.",
                okLabel: "Disable Learner",
                okClass: "btn-primary",
                onConfirm: function () { runAction(Data.setStatus(learner.id, false), "Learner disabled."); }
            });
        }

        function enableLearner(learner) {
            askConfirm({
                title: "Enable Learner?",
                text: learner.name + " will be able to sign in again.",
                okLabel: "Enable Learner",
                okClass: "btn-primary",
                onConfirm: function () { runAction(Data.setStatus(learner.id, true), "Learner enabled."); }
            });
        }

        function deleteLearner(learner) {
            askConfirm({
                title: "Delete Learner?",
                text: "This permanently deletes " + learner.name + "'s account together with all of their learning progress and Practice, "
                    + "Mock Exam and Road Sign Game results. This cannot be undone. To keep their history, disable the account instead.",
                okLabel: "Delete Learner",
                okClass: "btn-secondary btn-danger",
                onConfirm: function () { runAction(Data.remove(learner.id), "Learner deleted."); }
            });
        }

        editSave.addEventListener("click", saveEdit);
        document.getElementById("lrAddBtn").addEventListener("click", function () { openForm(null); });

        Object.keys(fieldErrors).forEach(function (key) {
            fieldErrors[key].input.addEventListener("input", function () {
                fieldErrors[key].input.removeAttribute("aria-invalid");
                fieldErrors[key].error.hidden = true;
            });
        });

        confirmOk.addEventListener("click", function () {
            var action = pendingConfirm;
            closeModal();
            if (action) { action(); }
        });

        function render() {
            tbody.textContent = "";

            if (!filtered.length) {
                var filtering = !!(searchInput.value.trim() || (statusFilter && statusFilter.value));
                emptyTitle.textContent = !loaded ? "Loading learners…"
                    : filtering ? "No learners match your search."
                    : "No learners have registered yet.";
                emptyClear.hidden = !loaded || !filtering;
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
                // Same status switch as the other admin lists; clicking it asks to disable / enable the account.
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

                group.appendChild(actionButton("Edit", "", function () { openForm(learner); }));

                group.appendChild(actionButton("Delete", "btn-danger", function () { deleteLearner(learner); }));

                actions.appendChild(group);
                row.appendChild(actions);

                tbody.appendChild(row);
            });

            summary.textContent = "Showing " + (start + 1) + "–" + (start + slice.length) + " of " + filtered.length
                + (filtered.length === 1 ? " learner" : " learners");

            renderPager();
        }

        searchInput.addEventListener("input", function () {
            applyFilter();
            page = 1;          // a new search always starts on the first page
            render();
        });

        // Enter in the search box would otherwise post the whole page back.
        searchInput.addEventListener("keydown", function (e) { if (e.key === "Enter") { e.preventDefault(); } });

        if (statusFilter) {
            statusFilter.addEventListener("change", function () {
                applyFilter();
                page = 1;
                render();
            });
        }

        emptyClear.addEventListener("click", function () {
            searchInput.value = "";
            if (statusFilter) { statusFilter.value = ""; }
            applyFilter();
            page = 1;
            render();
        });

        render();
        load();
    })();

    // ------------------------------------------------------------------
    // Detail page
    // ------------------------------------------------------------------
    (function initDetail() {
        var root = document.getElementById("lrDetail");
        if (!root) { return; }

        function byId(id) { return document.getElementById(id); }

        // Missing / unknown id (or an administrator's id): a "not found" notice with a link back to the list.
        function notFound(message) {
            root.hidden = true;
            byId("lrNotFound").hidden = false;
            if (message) { byId("lrNotFound").querySelector(".alert").textContent = message; }
        }

        var match = /[?&]id=([1-9][0-9]{0,9})(?:&|$)/.exec(window.location.search);
        if (!match) { notFound(); return; }

        Data.getDetails(parseInt(match[1], 10)).then(function (learner) {
            if (!learner) { notFound(); return; }
            root.hidden = false;
            renderDetail(learner);
        }).catch(function (e) { notFound(e.message); });

        function renderDetail(learner) {
            // ---- Learner Information ----
            byId("lrInfoAvatar").textContent = Data.initials(learner.name);
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
            var learning = byId("lrLearning");
            learning.appendChild(progressBlock("Overall Learning Progress", done, total, "", "Overall learning progress"));
            if (!learner.chapters.length) {
                learning.appendChild(el("p", "lr-note", "No learning content is available yet."));
            }
            learner.chapters.forEach(function (chapter) {
                learning.appendChild(progressBlock("Chapter " + chapter.number + " — " + chapter.title, chapter.completed, chapter.total, "lr-progress-block--chapter", "Chapter progress"));
            });

            // ---- history lists ----
            historyList("lrPractice", learner.practice, "No Practice attempts yet.", function (li, a) {
                var main = el("div", "lr-history__main");
                main.appendChild(el("div", "lr-history__title", a.category + " · Practice Set " + a.setNumber));
                main.appendChild(el("div", "lr-history__line", a.correct + " / " + a.total + " Correct"));
                main.appendChild(el("div", "lr-history__date", Data.formatDate(a.date, false)));
                li.appendChild(main);
                li.appendChild(el("div", "lr-history__score", Data.percent(a.correct, a.total) + "%"));
            });

            historyList("lrMock", learner.mock, "No Mock Exams completed yet.", function (li, a) {
                var main = el("div", "lr-history__main");
                main.appendChild(el("div", "lr-history__title", a.examType + " · Mock Exam Set " + a.setNumber));
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
        }

        // Same states as the learner Progress page: a finished chapter is green with a tick, a chapter without lessons says so.
        function progressBlock(title, completed, totalLessons, extraClass, ariaLabel) {
            var pct = Data.percent(completed, totalLessons);
            var isChapter = !!extraClass;
            var noLessons = isChapter && totalLessons <= 0;
            var finished = isChapter && totalLessons > 0 && completed >= totalLessons;
            var block = el("div", "lr-progress-block" + (extraClass ? " " + extraClass : "") + (finished ? " lr-progress-block--done" : ""));

            var head = el("div", "lr-progress-block__head");
            head.appendChild(el("span", "lr-progress-block__title", title));
            head.appendChild(el("span", "lr-progress-block__percent", (finished ? "✓ " : "") + pct + "%"));
            block.appendChild(head);

            block.appendChild(el("div", "lr-progress-block__meta", noLessons ? "No lessons yet" : completed + " / " + totalLessons + " Lessons Completed"));

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

        // Like the learner Progress page: the 3 most recent are shown, older ones sit behind a View All / Show Less button.
        var RECENT = 3;

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

        // "N games played · Best: x / y · Average: z%" (same wording and maths as the learner page).
        function gameSummary(games) {
            var best = games[0], ratioSum = 0;
            games.forEach(function (g) {
                var ratio = g.total > 0 ? g.correct / g.total : 0;
                ratioSum += ratio;
                if (ratio > (best.total > 0 ? best.correct / best.total : 0)) { best = g; }
            });
            return games.length + (games.length === 1 ? " game played" : " games played")
                + " · Best: " + best.correct + " / " + best.total
                + " · Average: " + Math.round(ratioSum / games.length * 100) + "%";
        }
    })();
})();
