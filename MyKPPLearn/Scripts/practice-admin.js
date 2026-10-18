/*
 * ============================================================================
 * Phase 5 frontend-only Admin UI prototype for Practice Sets and Question Bank.
 *
 * This file performs NO server calls and NO database operations. All rows
 * shown here come from window.PracticeDemoData (see Scripts/practice-demo.js,
 * which must be loaded first) and are copied into local, in-memory arrays
 * that this script mutates directly. Add/Edit/Delete/status-toggle are all
 * UI-only: nothing is sent to the server, and a page refresh resets every
 * change back to the original mock data. Replace with real repository-backed
 * Admin pages in a later phase.
 * ============================================================================
 */
(function () {
    "use strict";

    function setCrumb(crumbId, text) {
        var crumb = document.getElementById(crumbId);
        if (crumb) { crumb.textContent = text; }
    }

    // With a message the toast is rewritten (type: "success" by default, "warning" or "error"); without one it just re-shows its own text.
    function showToast(el, message, type) {
        if (message) {
            el.textContent = message;
            el.className = "alert alert-" + (type || "success");
        }

        el.hidden = false;
        window.clearTimeout(el._hideTimer);
        el._hideTimer = window.setTimeout(function () {
            el.hidden = true;
        }, 4000);
    }

    function statusButton(status, onToggle) {
        var button = document.createElement("button");
        button.type = "button";
        setStatusButtonState(button, status);
        button.addEventListener("click", function () {
            onToggle();
        });
        return button;
    }

    function setStatusButtonState(button, status) {
        var isActive = status === "Active";
        button.textContent = isActive ? "Active" : "Inactive";
        button.className = "btn btn-secondary btn-small btn-status " + (isActive ? "btn-status--active" : "btn-status--inactive");
        button.setAttribute("aria-label", button.textContent + ". Click to " + (isActive ? "deactivate" : "activate") + ".");
    }

    // ========================================================================
    // Admin / PracticeSets.aspx
    // ========================================================================
    (function initPracticeSetsAdmin() {
        var root = document.getElementById("psListView");

        if (!root || !window.PracticeDemoData) { return; }

        var NOTE = " (Prototype only: not saved.)";

        var categories = PracticeDemoData.getCategories();
        var sets = PracticeDemoData.getAllSets(); // local mutable copy
        var nextLocalId = 1;

        // Question counts per set come from the mock questions, so they always agree with the Question Bank's starting data.
        var questionCounts = {};
        PracticeDemoData.getAllQuestions().forEach(function (q) {
            var entry = questionCounts[q.setId] || (questionCounts[q.setId] = { active: 0, total: 0 });
            entry.total++;
            if (q.status === "Active") { entry.active++; }
        });

        function counts(set) {
            return questionCounts[set.id] || { active: 0, total: 0 };
        }

        var categoryFilter = document.getElementById("psCategoryFilter");
        var clearFilterBtn = document.getElementById("psClearFilter");
        var tableWrap = document.getElementById("psTableWrap");
        var tableBody = document.getElementById("psTableBody");
        var empty = document.getElementById("psEmpty");
        var emptyTitle = document.getElementById("psEmptyTitle");
        var emptyAddBtn = document.getElementById("psEmptyAdd");
        var emptyClearBtn = document.getElementById("psEmptyClear");
        var toast = document.getElementById("psToast");

        var addBtn = document.getElementById("psAddBtn");
        var listView = document.getElementById("psListView");
        var formView = document.getElementById("psFormView");

        var formHeading = document.getElementById("psFormHeading");
        var formCategory = document.getElementById("psFormCategory");
        var categoryHint = document.getElementById("psCategoryHint");
        var categoryError = document.getElementById("psCategoryError");
        var formNumber = document.getElementById("psFormNumber");
        var numberError = document.getElementById("psNumberError");
        var usedNumbers = document.getElementById("psUsedNumbers");
        var statusHint = document.getElementById("psStatusHint");
        var saveBtn = document.getElementById("psSaveBtn");
        var cancelBtn = document.getElementById("psCancelBtn");
        var crumbLink = formView.querySelector(".breadcrumb a");

        var pvCategory = document.getElementById("pvCategory");
        var pvCategoryDot = document.getElementById("pvCategoryDot");
        var pvSetTitle = document.getElementById("pvSetTitle");
        var pvQuestionCount = document.getElementById("pvQuestionCount");

        var editingId = null;
        var lastAuto = null;       // the number suggested automatically (so typing your own is never overwritten)

        function categoryTitle(categoryId) {
            var match = categories.filter(function (c) { return c.id === categoryId; })[0];
            return match ? match.title : categoryId;
        }

        function dotClass(categoryId) {
            var colour = PracticeDemoData.getCategoryColour(categoryId);
            return "ps-cat__dot" + (colour ? " ps-cat__dot--" + colour : "");
        }

        function findSet(id) {
            return sets.filter(function (s) { return s.id === id; })[0];
        }

        populateSelect(categoryFilter, [{ id: "", title: "All Categories" }].concat(categories), "id", "title");
        populateSelect(formCategory, categories, "id", "title");

        function populateSelect(select, items, valueField, textField) {
            select.innerHTML = "";
            items.forEach(function (item) {
                var option = document.createElement("option");
                option.value = item[valueField];
                option.textContent = item[textField];
                select.appendChild(option);
            });
        }

        function showView(name) {
            listView.hidden = name !== "list";
            formView.hidden = name !== "form";
        }

        function questionsUrl(set) {
            return "QuestionBank.aspx?category=" + encodeURIComponent(set.categoryId) + "&set=" + encodeURIComponent(set.id);
        }

        function renderTable() {
            var filterValue = categoryFilter.value;
            var filtered = sets
                .filter(function (s) { return !filterValue || s.categoryId === filterValue; })
                .sort(function (a, b) { return a.categoryId === b.categoryId ? a.number - b.number : 0; });

            tableBody.innerHTML = "";
            tableWrap.hidden = filtered.length === 0;
            empty.hidden = filtered.length !== 0;
            clearFilterBtn.hidden = !filterValue;

            // Nothing created yet: offer "Add". Nothing matches the filter: offer "Clear Filter".
            emptyTitle.textContent = sets.length === 0 ? "No practice sets have been created yet." : "No practice sets match the selected filter.";
            emptyAddBtn.hidden = sets.length !== 0;
            emptyClearBtn.hidden = sets.length === 0;

            filtered.forEach(function (set) {
                var row = document.createElement("tr");
                if (set.status !== "Active") { row.className = "row--inactive"; }

                var setCell = document.createElement("td");
                setCell.className = "data-table__nowrap";
                var strong = document.createElement("strong");
                strong.textContent = String(set.number);
                setCell.appendChild(strong);
                row.appendChild(setCell);

                var categoryCell = document.createElement("td");
                var dot = document.createElement("span");
                dot.className = dotClass(set.categoryId);
                dot.setAttribute("aria-hidden", "true");
                categoryCell.appendChild(dot);
                categoryCell.appendChild(document.createTextNode(categoryTitle(set.categoryId)));
                row.appendChild(categoryCell);

                var countCell = document.createElement("td");
                var count = counts(set);
                var countLink = document.createElement("a");
                countLink.href = questionsUrl(set);
                countLink.className = "ps-count-link";
                countLink.textContent = String(count.active);
                countLink.title = count.active + " active of " + count.total + " questions. Open the Question Bank.";
                countCell.appendChild(countLink);
                row.appendChild(countCell);

                var statusCell = document.createElement("td");
                statusCell.appendChild(statusButton(set.status, function () { toggleStatus(set); }));
                row.appendChild(statusCell);

                var actionsCell = document.createElement("td");
                var actions = document.createElement("div");
                actions.className = "row-actions";

                var view = document.createElement("a");
                view.className = "btn btn-secondary btn-small";
                view.href = questionsUrl(set);
                view.textContent = "View";
                view.setAttribute("aria-label", "View questions in " + PracticeDemoData.setLabel(set.number) + " (" + categoryTitle(set.categoryId) + ")");
                actions.appendChild(view);

                actions.appendChild(actionButton("Edit", function () { openForm(set); }));
                actions.appendChild(deleteButton(set));
                actionsCell.appendChild(actions);
                row.appendChild(actionsCell);

                tableBody.appendChild(row);
            });
        }

        function actionButton(label, handler) {
            var button = document.createElement("button");
            button.type = "button";
            button.className = "btn btn-secondary btn-small";
            button.textContent = label;
            button.addEventListener("click", handler);
            return button;
        }

        // A set that still has questions cannot be deleted; otherwise the shared confirm dialog (confirm-modal.js) asks first.
        function deleteButton(set) {
            var button = document.createElement("button");
            button.type = "button";
            button.className = "btn btn-secondary btn-small btn-danger";
            button.textContent = "Delete";

            if (counts(set).total > 0) {
                button.setAttribute("aria-disabled", "true");
                button.title = "Remove this set's questions first";
                return button;
            }

            button.setAttribute("data-confirm", "Delete " + PracticeDemoData.setLabel(set.number) + " (" + categoryTitle(set.categoryId) + ")?");
            button.setAttribute("data-confirm-title", "Delete Practice Set?");
            button.addEventListener("click", function () {
                sets = sets.filter(function (s) { return s.id !== set.id; });
                renderTable();
                showToast(toast, "Practice set deleted." + NOTE);
            });
            return button;
        }

        function toggleStatus(set) {
            var label = PracticeDemoData.setLabel(set.number);

            if (set.status === "Active") {
                var othersActive = sets.some(function (s) { return s.id !== set.id && s.categoryId === set.categoryId && s.status === "Active"; });
                set.status = "Inactive";
                renderTable();

                if (othersActive) {
                    showToast(toast, label + " deactivated." + NOTE);
                } else {
                    showToast(toast, label + " deactivated. " + categoryTitle(set.categoryId) + " has no active sets now, so learners will see it as “Coming soon”." + NOTE, "warning");
                }
                return;
            }

            // An empty set would show learners a practice with no questions.
            if (counts(set).active === 0) {
                showToast(toast, label + " can't be activated until it has at least one active question. Add questions in the Question Bank first.", "error");
                return;
            }

            set.status = "Active";
            renderTable();
            showToast(toast, label + " activated." + NOTE);
        }

        // "Used Numbers" for the CURRENT Category only; informational, the duplicate check below is unchanged.
        function usedInCategory() {
            return sets
                .filter(function (s) { return s.categoryId === formCategory.value; })
                .map(function (s) { return s.number; });
        }

        function renderUsedNumbers() {
            if (!usedNumbers || !window.UsedNumbers) { return; }
            usedNumbers.textContent = UsedNumbers.format(usedInCategory());
        }

        // Adding: suggest the next free number for the chosen category, unless the admin typed their own.
        function suggestNumber() {
            if (editingId) { return; }
            if (formNumber.value !== "" && formNumber.value !== lastAuto) { return; }

            var max = 0;
            usedInCategory().forEach(function (n) { if (n > max) { max = n; } });
            lastAuto = String(max + 1);
            formNumber.value = lastAuto;
        }

        // Same look as the learner's practice set card; only the number and the category are live.
        function updatePreview() {
            var raw = formNumber.value.trim();
            var valid = /^[1-9][0-9]{0,8}$/.test(raw);
            var editing = editingId ? findSet(editingId) : null;
            var questionCount = editing ? counts(editing).active : 0;

            pvCategory.textContent = categoryTitle(formCategory.value);
            pvCategoryDot.className = dotClass(formCategory.value);
            pvSetTitle.textContent = valid ? "Set " + raw : "Set";
            pvQuestionCount.textContent = questionCount + (questionCount === 1 ? " Question" : " Questions");
        }

        function clearErrors() {
            categoryError.hidden = true;
            numberError.hidden = true;
        }

        function fail(errorEl, message, field) {
            errorEl.textContent = message;
            errorEl.hidden = false;
            field.focus();
        }

        function openForm(set) {
            clearErrors();
            editingId = set ? set.id : null;
            lastAuto = null;

            if (set) {
                formHeading.textContent = "Edit Practice Set";
                setCrumb("psFormCrumb", "Edit Practice Set");
                formCategory.value = set.categoryId;
                formNumber.value = String(set.number);

                // Its questions belong to this category, so the category stays fixed once the set has any.
                var locked = counts(set).total > 0;
                formCategory.disabled = locked;
                categoryHint.hidden = !locked;
                statusHint.hidden = true;
            } else {
                formHeading.textContent = "Add Practice Set";
                setCrumb("psFormCrumb", "Add Practice Set");
                formCategory.disabled = false;
                categoryHint.hidden = true;
                statusHint.hidden = false;
                formCategory.value = categoryFilter.value || categories[0].id;     // start in the category being filtered, if any
                formNumber.value = "";
                suggestNumber();
            }

            renderUsedNumbers();
            updatePreview();
            showView("form");
            (formCategory.disabled ? formNumber : formCategory).focus();
        }

        function backToList() {
            showView("list");
            addBtn.focus();
        }

        addBtn.addEventListener("click", function () { openForm(null); });
        emptyAddBtn.addEventListener("click", function () { openForm(null); });
        cancelBtn.addEventListener("click", backToList);

        // The breadcrumb behaves like Cancel (a normal link would reload the page and throw away every unsaved mock edit).
        crumbLink.addEventListener("click", function (event) {
            event.preventDefault();
            backToList();
        });

        formCategory.addEventListener("change", function () {
            clearErrors();
            renderUsedNumbers();
            suggestNumber();
            updatePreview();
        });

        formNumber.addEventListener("input", function () {
            numberError.hidden = true;
            updatePreview();
        });

        formNumber.addEventListener("keydown", function (event) {
            if (event.key === "Enter") {
                event.preventDefault();
                saveBtn.click();
            }
        });

        saveBtn.addEventListener("click", function () {
            clearErrors();

            var raw = formNumber.value.trim();

            if (!formCategory.value) {
                fail(categoryError, "Please select a category.", formCategory);
                return;
            }

            if (!/^[1-9][0-9]{0,8}$/.test(raw)) {
                fail(numberError, "Practice Set number must be a whole number greater than 0 (for example 1, 2, 3).", formNumber);
                return;
            }

            var number = parseInt(raw, 10);

            var duplicate = sets.some(function (s) {
                return s.id !== editingId && s.categoryId === formCategory.value && s.number === number;
            });

            if (duplicate) {
                fail(numberError, categoryTitle(formCategory.value) + " already has " + PracticeDemoData.setLabel(number) + ". Please choose a different number.", formNumber);
                return;
            }

            var message;

            if (editingId) {
                var existing = findSet(editingId);
                existing.categoryId = formCategory.value;
                existing.number = number;
                message = "Practice set updated.";
            } else {
                // Inactive until it has questions (see the hint on the form).
                sets.push({
                    id: "demo-set-" + (nextLocalId++),
                    categoryId: formCategory.value,
                    number: number,
                    questionCount: 0,
                    status: "Inactive"
                });
                message = "Practice set created as Inactive. Add questions in the Question Bank, then activate it.";
            }

            renderTable();
            backToList();
            showToast(toast, message + NOTE);
        });

        function clearFilter() {
            categoryFilter.value = "";
            renderTable();
        }

        clearFilterBtn.addEventListener("click", clearFilter);
        emptyClearBtn.addEventListener("click", clearFilter);
        categoryFilter.addEventListener("change", renderTable);

        renderTable();
    })();

    // ========================================================================
    // Admin / QuestionBank.aspx
    // ========================================================================
    (function initQuestionBankAdmin() {
        var root = document.getElementById("qbListView");

        if (!root || !window.PracticeDemoData) { return; }

        var NOTE = " (Prototype only: not saved.)";
        var LETTERS = "ABCD";
        var MAX_TEXT = 500;
        var MAX_OPTION = 200;
        var MAX_EXPLANATION = 500;
        var MAX_IMAGE_BYTES = 2 * 1024 * 1024;
        var IMAGE_TYPES = ["image/jpeg", "image/png", "image/webp"];

        var categories = PracticeDemoData.getCategories();
        var allSets = PracticeDemoData.getAllSets().sort(function (a, b) {
            var ca = categoryIndex(a.categoryId), cb = categoryIndex(b.categoryId);
            return ca !== cb ? ca - cb : a.number - b.number;
        });
        var questions = PracticeDemoData.getAllQuestions(); // local mutable copy
        var appRoot = root.getAttribute("data-app-root") || "";
        var nextLocalId = 1;

        var filterCategory = document.getElementById("qbCategoryFilter");
        var filterSet = document.getElementById("qbSetFilter");
        var filterStatus = document.getElementById("qbStatusFilter");
        var filterSearch = document.getElementById("qbSearch");
        var clearFiltersBtn = document.getElementById("qbClearFilters");

        var tableWrap = document.getElementById("qbTableWrap");
        var tableBody = document.getElementById("qbTableBody");
        var empty = document.getElementById("qbEmpty");
        var emptyTitle = document.getElementById("qbEmptyTitle");
        var emptyAddBtn = document.getElementById("qbEmptyAdd");
        var emptyClearBtn = document.getElementById("qbEmptyClear");
        var toast = document.getElementById("qbToast");

        var addBtn = document.getElementById("qbAddBtn");
        var listView = document.getElementById("qbListView");
        var formView = document.getElementById("qbFormView");
        var crumbLink = formView.querySelector(".breadcrumb a");

        var formHeading = document.getElementById("qbFormHeading");
        var formCategory = document.getElementById("qbFormCategory");
        var formSet = document.getElementById("qbFormSet");
        var noSetsHint = document.getElementById("qbNoSets");
        var formText = document.getElementById("qbFormText");
        var formImage = document.getElementById("qbFormImage");
        var imagePreviewWrap = document.getElementById("qbImagePreviewWrap");
        var imagePreview = document.getElementById("qbImagePreview");
        var imageTools = document.getElementById("qbImageTools");
        var removeImageBtn = document.getElementById("qbRemoveImage");
        var altGroup = document.getElementById("qbAltGroup");
        var formImageAlt = document.getElementById("qbFormImageAlt");
        var optionInputs = ["A", "B", "C", "D"].map(function (letter) { return document.getElementById("qbFormOption" + letter); });
        var radios = document.querySelectorAll("input[name='qbCorrectAnswer']");
        var formExplanation = document.getElementById("qbFormExplanation");
        var statusHint = document.getElementById("qbStatusHint");
        var textCount = document.getElementById("qbTextCount");
        var explanationCount = document.getElementById("qbExplanationCount");
        var saveBtn = document.getElementById("qbSaveBtn");
        var cancelBtn = document.getElementById("qbCancelBtn");

        var errors = {
            category: document.getElementById("qbCategoryError"),
            set: document.getElementById("qbSetError"),
            text: document.getElementById("qbTextError"),
            image: document.getElementById("qbImageError"),
            alt: document.getElementById("qbAltError"),
            options: document.getElementById("qbOptionsError"),
            correct: document.getElementById("qbCorrectError"),
            explanation: document.getElementById("qbExplanationError")
        };

        var pvImageWrap = document.getElementById("pvImageWrap");
        var pvImage = document.getElementById("pvImage");
        var pvQuestion = document.getElementById("pvQuestion");
        var pvOptions = document.getElementById("pvOptions");
        var pvExplanation = document.getElementById("pvExplanation");
        var pvExplanationText = document.getElementById("pvExplanationText");

        var editingId = null;

        // Image being edited: the question's saved image (if any), a newly chosen local file (pending) or "removed".
        var imageState = { existing: null, pendingUrl: null, removed: false };

        function categoryIndex(categoryId) {
            for (var i = 0; i < categories.length; i++) {
                if (categories[i].id === categoryId) { return i; }
            }
            return categories.length;
        }

        function categoryTitle(categoryId) {
            var match = categories.filter(function (c) { return c.id === categoryId; })[0];
            return match ? match.title : categoryId;
        }

        function dotClass(categoryId) {
            var colour = PracticeDemoData.getCategoryColour(categoryId);
            return "ps-cat__dot" + (colour ? " ps-cat__dot--" + colour : "");
        }

        function findSet(setId) {
            return allSets.filter(function (s) { return s.id === setId; })[0];
        }

        function setsForCategory(categoryId) {
            return allSets.filter(function (s) { return !categoryId || s.categoryId === categoryId; });
        }

        // In the filter with "All Categories", the category is part of the label so the sets can be told apart.
        function setOption(set, withCategory) {
            var label = PracticeDemoData.setLabel(set.number);
            return { id: set.id, title: withCategory ? categoryTitle(set.categoryId) + " – " + label : label };
        }

        function setNumber(setId) {
            var match = findSet(setId);
            return match ? String(match.number) : setId;
        }

        function imageSrc(question) {
            return question.imageLocal ? question.image : appRoot + question.image;
        }

        function activeCount(setId) {
            return questions.filter(function (q) { return q.setId === setId && q.status === "Active"; }).length;
        }

        function populateSelect(select, items, valueField, textField) {
            select.innerHTML = "";
            items.forEach(function (item) {
                var option = document.createElement("option");
                option.value = item[valueField];
                option.textContent = item[textField];
                select.appendChild(option);
            });
        }

        function refreshFilterSetOptions() {
            var categoryId = filterCategory.value;
            populateSelect(filterSet, [{ id: "", title: "All Practice Sets" }].concat(setsForCategory(categoryId).map(function (s) { return setOption(s, !categoryId); })), "id", "title");
        }

        function refreshFormSetOptions() {
            var sets = setsForCategory(formCategory.value);

            populateSelect(formSet, sets.map(function (s) {
                var option = setOption(s, false);
                if (s.status !== "Active") { option.title += " (Inactive)"; }
                return option;
            }), "id", "title");

            noSetsHint.hidden = sets.length !== 0;
        }

        populateSelect(filterCategory, [{ id: "", title: "All Categories" }].concat(categories), "id", "title");
        populateSelect(formCategory, categories, "id", "title");
        refreshFilterSetOptions();
        refreshFormSetOptions();

        function showView(name) {
            listView.hidden = name !== "list";
            formView.hidden = name !== "form";
        }

        function truncate(text, max) {
            return text.length > max ? text.substring(0, max).trim() + "…" : text;
        }

        function isFiltering() {
            return !!(filterCategory.value || filterSet.value || filterStatus.value || filterSearch.value.trim());
        }

        function renderTable() {
            var categoryValue = filterCategory.value;
            var setValue = filterSet.value;
            var statusValue = filterStatus.value;
            var term = filterSearch.value.trim().toLowerCase();

            var filtered = questions
                .filter(function (q) {
                    return (!categoryValue || q.categoryId === categoryValue)
                        && (!setValue || q.setId === setValue)
                        && (!statusValue || q.status === statusValue)
                        && (!term || q.text.toLowerCase().indexOf(term) !== -1);
                })
                .sort(function (a, b) {
                    var sa = findSet(a.setId), sb = findSet(b.setId);
                    var ca = categoryIndex(a.categoryId), cb = categoryIndex(b.categoryId);
                    if (ca !== cb) { return ca - cb; }
                    return (sa ? sa.number : 0) - (sb ? sb.number : 0);
                });

            var filtering = isFiltering();
            tableBody.innerHTML = "";
            tableWrap.hidden = filtered.length === 0;
            empty.hidden = filtered.length !== 0;
            clearFiltersBtn.hidden = !filtering;

            // Nothing created yet: offer "Add". Nothing matches the filters: offer "Clear Filters".
            emptyTitle.textContent = questions.length === 0 ? "No questions have been created yet." : "No questions match the selected filters.";
            emptyAddBtn.hidden = questions.length !== 0;
            emptyClearBtn.hidden = questions.length === 0;

            filtered.forEach(function (question) {
                var row = document.createElement("tr");
                if (question.status !== "Active") { row.className = "row--inactive"; }

                var textCell = document.createElement("td");
                textCell.className = "data-table__muted";
                var textSpan = document.createElement("span");
                textSpan.textContent = truncate(question.text, 70);
                textSpan.title = question.text;
                textCell.appendChild(textSpan);

                if (question.image) {
                    var imageBadge = document.createElement("span");
                    imageBadge.className = "badge qb-badge-image";
                    imageBadge.textContent = "Image";
                    textCell.appendChild(imageBadge);
                }

                row.appendChild(textCell);

                var categoryCell = document.createElement("td");
                categoryCell.className = "data-table__nowrap";
                var dot = document.createElement("span");
                dot.className = dotClass(question.categoryId);
                dot.setAttribute("aria-hidden", "true");
                categoryCell.appendChild(dot);
                categoryCell.appendChild(document.createTextNode(categoryTitle(question.categoryId)));
                row.appendChild(categoryCell);

                var setCell = document.createElement("td");
                setCell.className = "data-table__nowrap";
                setCell.textContent = setNumber(question.setId);
                row.appendChild(setCell);

                var statusCell = document.createElement("td");
                statusCell.appendChild(statusButton(question.status, function () { toggleStatus(question); }));
                row.appendChild(statusCell);

                var actionsCell = document.createElement("td");
                var actions = document.createElement("div");
                actions.className = "row-actions";
                actions.appendChild(actionButton("Edit", function () { openForm(question); }));
                actions.appendChild(deleteButton(question));
                actionsCell.appendChild(actions);
                row.appendChild(actionsCell);

                tableBody.appendChild(row);
            });
        }

        function actionButton(label, handler) {
            var button = document.createElement("button");
            button.type = "button";
            button.className = "btn btn-secondary btn-small";
            button.textContent = label;
            button.addEventListener("click", handler);
            return button;
        }

        // The shared confirm dialog (confirm-modal.js) asks first; the click handler only runs once it is confirmed.
        function deleteButton(question) {
            var button = document.createElement("button");
            button.type = "button";
            button.className = "btn btn-secondary btn-small btn-danger";
            button.textContent = "Delete";
            button.setAttribute("data-confirm", "Delete this question from " + PracticeDemoData.setLabel(setNumber(question.setId)) + " (" + categoryTitle(question.categoryId) + ")?");
            button.setAttribute("data-confirm-title", "Delete Question?");
            button.addEventListener("click", function () {
                questions = questions.filter(function (q) { return q.id !== question.id; });
                renderTable();
                reportSetState(question.setId, "Question deleted.");
            });
            return button;
        }

        function toggleStatus(question) {
            question.status = question.status === "Active" ? "Inactive" : "Active";
            renderTable();
            reportSetState(question.setId, question.status === "Active" ? "Question activated." : "Question deactivated.");
        }

        // A set that is Active but has no active question left would show learners an empty practice: warn the admin.
        function reportSetState(setId, message) {
            var set = findSet(setId);

            if (set && set.status === "Active" && activeCount(setId) === 0) {
                showToast(toast, message + " " + PracticeDemoData.setLabel(set.number) + " (" + categoryTitle(set.categoryId)
                    + ") is still Active but has no active questions now, so learners would get an empty practice. Deactivate the set in Practice Sets, or add or activate another question." + NOTE, "warning");
                return;
            }

            showToast(toast, message + NOTE);
        }

        // ---- form helpers ----
        function getCorrect() {
            var checked = Array.prototype.filter.call(radios, function (r) { return r.checked; })[0];
            return checked ? parseInt(checked.value, 10) : -1;
        }

        function setCorrect(index) {
            Array.prototype.forEach.call(radios, function (radio) {
                radio.checked = parseInt(radio.value, 10) === index;
            });
        }

        function clearErrors() {
            Object.keys(errors).forEach(function (key) { errors[key].hidden = true; });
        }

        function effectiveImage() {
            if (imageState.pendingUrl) { return { src: imageState.pendingUrl, path: imageState.pendingUrl, local: true, pending: true }; }
            if (imageState.existing && !imageState.removed) { return imageState.existing; }
            return null;
        }

        function revokePending() {
            if (imageState.pendingUrl) {
                URL.revokeObjectURL(imageState.pendingUrl);
                imageState.pendingUrl = null;
            }
        }

        function resetImageState(question) {
            revokePending();
            formImage.value = "";
            imageState.removed = false;
            imageState.existing = question && question.image
                ? { src: imageSrc(question), path: question.image, local: !!question.imageLocal }
                : null;
            formImageAlt.value = question && question.imageAlt ? question.imageAlt : "";
        }

        function updateImageUi() {
            var image = effectiveImage();
            imagePreviewWrap.hidden = !image;
            imageTools.hidden = !image;
            altGroup.hidden = !image;

            if (image) {
                imagePreview.src = image.src;
                imagePreview.alt = formImageAlt.value.trim();
            } else {
                imagePreview.removeAttribute("src");
                imagePreview.alt = "";
            }
        }

        // Same look as the learner's question card; the correct option is highlighted for the admin.
        function updatePreview() {
            var text = formText.value.trim();
            var explanation = formExplanation.value.trim();
            var correct = getCorrect();
            var image = effectiveImage();

            textCount.textContent = String(formText.value.length);
            explanationCount.textContent = String(formExplanation.value.length);

            pvQuestion.textContent = text || "Question text";
            pvImageWrap.hidden = !image;

            if (image) {
                pvImage.src = image.src;
            } else {
                pvImage.removeAttribute("src");
            }

            pvOptions.innerHTML = "";
            optionInputs.forEach(function (input, index) {
                var row = document.createElement("div");
                row.className = "qb-preview__option" + (index === correct ? " qb-preview__option--correct" : "");

                var letter = document.createElement("span");
                letter.className = "qb-preview__letter";
                letter.textContent = LETTERS.charAt(index);
                row.appendChild(letter);

                var label = document.createElement("span");
                label.className = "qb-preview__option-text";
                label.textContent = input.value.trim() || "Option " + LETTERS.charAt(index);
                row.appendChild(label);

                if (index === correct) {
                    var mark = document.createElement("span");
                    mark.className = "qb-preview__mark";
                    mark.textContent = "✓";
                    row.appendChild(mark);
                }

                pvOptions.appendChild(row);
            });

            pvExplanation.hidden = !explanation;
            pvExplanationText.textContent = explanation;
        }

        function openForm(question) {
            clearErrors();
            resetImageState(question);
            editingId = question ? question.id : null;

            if (question) {
                formHeading.textContent = "Edit Question";
                setCrumb("qbFormCrumb", "Edit Question");
                formCategory.value = question.categoryId;
                refreshFormSetOptions();
                formSet.value = question.setId;
                formText.value = question.text;
                optionInputs.forEach(function (input, index) { input.value = question.options[index]; });
                setCorrect(question.correctIndex);
                formExplanation.value = question.explanation;
                statusHint.hidden = true;
            } else {
                formHeading.textContent = "Add Question";
                setCrumb("qbFormCrumb", "Add Question");

                // Start in the category / set being filtered, if any.
                formCategory.value = filterCategory.value || (filterSet.value && findSet(filterSet.value) ? findSet(filterSet.value).categoryId : categories[0].id);
                refreshFormSetOptions();

                if (filterSet.value && findSet(filterSet.value) && findSet(filterSet.value).categoryId === formCategory.value) {
                    formSet.value = filterSet.value;
                }

                formText.value = "";
                optionInputs.forEach(function (input) { input.value = ""; });
                setCorrect(-1);          // nothing is pre-selected: the admin must choose
                formExplanation.value = "";
                statusHint.hidden = false;
            }

            updateImageUi();
            updatePreview();
            showView("form");
            formCategory.focus();
        }

        function backToList() {
            revokePending();
            showView("list");
            addBtn.focus();
        }

        addBtn.addEventListener("click", function () { openForm(null); });
        emptyAddBtn.addEventListener("click", function () { openForm(null); });
        cancelBtn.addEventListener("click", backToList);

        // The breadcrumb behaves like Cancel (a normal link would reload the page and throw away every unsaved mock edit).
        crumbLink.addEventListener("click", function (event) {
            event.preventDefault();
            backToList();
        });

        formCategory.addEventListener("change", function () {
            errors.category.hidden = true;
            errors.set.hidden = true;
            refreshFormSetOptions();
        });

        formSet.addEventListener("change", function () { errors.set.hidden = true; });

        formImage.addEventListener("change", function () {
            errors.image.hidden = true;
            var file = formImage.files && formImage.files[0];

            if (!file) { return; }

            var problem = null;

            if (IMAGE_TYPES.indexOf(file.type) === -1) {
                problem = "Choose a JPG, PNG or WEBP image.";
            } else if (file.size > MAX_IMAGE_BYTES) {
                problem = "The image must be 2 MB or smaller.";
            }

            if (problem) {
                formImage.value = "";
                errors.image.textContent = problem;
                errors.image.hidden = false;
                return;
            }

            revokePending();
            imageState.pendingUrl = URL.createObjectURL(file);
            imageState.removed = false;
            updateImageUi();
            updatePreview();
        });

        removeImageBtn.addEventListener("click", function () {
            revokePending();
            formImage.value = "";
            imageState.removed = true;
            updateImageUi();
            updatePreview();
            formImage.focus();
        });

        formImageAlt.addEventListener("input", function () {
            errors.alt.hidden = true;
            imagePreview.alt = formImageAlt.value.trim();
        });

        // Live preview and counters; clear a field's error as soon as the admin edits it.
        formText.addEventListener("input", function () { errors.text.hidden = true; updatePreview(); });
        formExplanation.addEventListener("input", function () { errors.explanation.hidden = true; updatePreview(); });
        optionInputs.forEach(function (input) {
            input.addEventListener("input", function () { errors.options.hidden = true; updatePreview(); });
        });
        Array.prototype.forEach.call(radios, function (radio) {
            radio.addEventListener("change", function () { errors.correct.hidden = true; updatePreview(); });
        });

        // Enter saves from the single-line fields (not from the text areas, where it adds a line).
        optionInputs.concat([formImageAlt]).forEach(function (input) {
            input.addEventListener("keydown", function (event) {
                if (event.key === "Enter") {
                    event.preventDefault();
                    saveBtn.click();
                }
            });
        });

        saveBtn.addEventListener("click", function () {
            clearErrors();

            var firstProblem = null;

            function problem(errorEl, message, field) {
                errorEl.textContent = message;
                errorEl.hidden = false;
                if (!firstProblem) { firstProblem = field; }
            }

            var text = formText.value.trim();
            var explanation = formExplanation.value.trim();
            var options = optionInputs.map(function (input) { return input.value.trim(); });
            var correctIndex = getCorrect();
            var image = effectiveImage();
            var alt = formImageAlt.value.trim();
            var setsInCategory = setsForCategory(formCategory.value);

            if (!formCategory.value) {
                problem(errors.category, "Please select a category.", formCategory);
            }

            if (!formSet.value) {
                problem(errors.set, setsInCategory.length === 0
                    ? "This category has no practice sets yet. Create one in Practice Sets first."
                    : "Please select a practice set.", formSet);
            }

            if (!text) {
                problem(errors.text, "Question is required.", formText);
            } else if (text.length > MAX_TEXT) {
                problem(errors.text, "Question must be " + MAX_TEXT + " characters or fewer.", formText);
            } else if (formSet.value && questions.some(function (q) {
                return q.id !== editingId && q.setId === formSet.value && q.text.trim().toLowerCase() === text.toLowerCase();
            })) {
                problem(errors.text, "This practice set already has a question with the same text.", formText);
            }

            if (image && !alt) {
                problem(errors.alt, "Describe the image for screen readers.", formImageAlt);
            }

            var emptyIndex = options.indexOf("");

            if (emptyIndex !== -1) {
                problem(errors.options, "All four options are required (Option " + LETTERS.charAt(emptyIndex) + " is empty).", optionInputs[emptyIndex]);
            } else {
                outer:
                for (var i = 0; i < options.length; i++) {
                    for (var j = i + 1; j < options.length; j++) {
                        if (options[i].toLowerCase() === options[j].toLowerCase()) {
                            problem(errors.options, "Option " + LETTERS.charAt(j) + " repeats Option " + LETTERS.charAt(i) + ". Each option must be different.", optionInputs[j]);
                            break outer;
                        }
                    }
                }
            }

            if (correctIndex === -1) {
                problem(errors.correct, "Choose which option is the correct answer.", radios[0]);
            }

            if (!explanation) {
                problem(errors.explanation, "Explanation is required.", formExplanation);
            } else if (explanation.length > MAX_EXPLANATION) {
                problem(errors.explanation, "Explanation must be " + MAX_EXPLANATION + " characters or fewer.", formExplanation);
            }

            if (firstProblem) {
                firstProblem.focus();
                return;
            }

            var target;

            if (editingId) {
                target = questions.filter(function (q) { return q.id === editingId; })[0];
            } else {
                target = { id: "demo-question-" + (nextLocalId++), status: "Active" };
                questions.push(target);
            }

            target.setId = formSet.value;
            target.categoryId = formCategory.value;
            target.text = text;
            target.options = options;
            target.correctIndex = correctIndex;
            target.explanation = explanation;

            if (image) {
                target.image = image.path;
                target.imageLocal = image.local;
                target.imageAlt = alt;
            } else {
                target.image = null;
                target.imageLocal = false;
                target.imageAlt = null;
            }

            // The chosen file now belongs to the saved question, so it must not be released with the form.
            imageState.pendingUrl = null;

            renderTable();
            backToList();
            showToast(toast, (editingId ? "Question updated." : "Question created.") + NOTE);
        });

        function clearFilters() {
            filterCategory.value = "";
            refreshFilterSetOptions();
            filterSet.value = "";
            filterStatus.value = "";
            filterSearch.value = "";
            renderTable();
        }

        clearFiltersBtn.addEventListener("click", clearFilters);
        emptyClearBtn.addEventListener("click", clearFilters);

        filterCategory.addEventListener("change", function () {
            refreshFilterSetOptions();
            renderTable();
        });
        filterSet.addEventListener("change", renderTable);
        filterStatus.addEventListener("change", renderTable);
        filterSearch.addEventListener("input", renderTable);

        // "View" / the question count on the Practice Sets page link here with ?category=...&set=... to show one set's questions.
        (function applyLinkFilter() {
            var query = new URLSearchParams(window.location.search);
            var categoryId = query.get("category");
            var setId = query.get("set");

            if (categoryId && categories.some(function (c) { return c.id === categoryId; })) {
                filterCategory.value = categoryId;
                refreshFilterSetOptions();

                if (setId && setsForCategory(categoryId).some(function (x) { return x.id === setId; })) {
                    filterSet.value = setId;
                }
            }
        })();

        renderTable();
    })();
})();
