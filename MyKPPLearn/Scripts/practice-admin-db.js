/*
 * Database-backed Admin Practice Sets and Question Bank.
 * Both pages are shells; this script calls the page's own page methods (Admin/PracticeSets.aspx and
 * Admin/QuestionBank.aspx code-behind) with JSON over POST and redraws the table from the result.
 *
 * Form validation runs twice: here, so mistakes show under the field straight away, and again on the server
 * (PracticeSets.Save / QuestionRepository.Validate), which is the rule that counts. A page method reports a
 * validation problem as { error: "..." }; a server failure becomes a rejected promise.
 */
(function () {
    "use strict";

    var MOCK_EXAM_ONLY = "No practice set (mock exam only)";
    var MAX_TEXT = 500;
    var MAX_OPTION = 200;
    var MAX_ALT = 200;
    var MAX_IMAGE_BYTES = 2 * 1024 * 1024;
    var IMAGE_TYPES = ["image/jpeg", "image/png", "image/webp"];
    var QUESTIONS_PER_PAGE = 20;

    /** Calls a page method on the current page and resolves with its return value ("d"). */
    function call(name, data) {
        return fetch(location.pathname + "/" + name, {
            method: "POST",
            credentials: "same-origin",
            headers: { "Content-Type": "application/json; charset=utf-8" },
            body: JSON.stringify(data || {})
        }).then(function (response) {
            return response.text().then(function (raw) {
                var json = null;
                try { json = JSON.parse(raw); } catch (ignore) { }

                if (!response.ok) {
                    return checkSignedIn().then(function () {
                        throw new Error((json && json.Message) || "The request could not be completed. Please try again.");
                    });
                }

                return json.d;
            });
        });
    }

    /**
     * Page methods report an expired session only as a generic server error, so after a failure we ask for the page
     * itself: if it now redirects (to Login), the session has ended and the page is reloaded so the user can sign in.
     */
    function checkSignedIn() {
        return fetch(location.pathname, { credentials: "same-origin", redirect: "manual" }).then(function (r) {
            if (r.type === "opaqueredirect" || r.status === 302) {
                location.reload();
                throw new Error("Your session has ended. Please sign in again.");
            }
        }, function () { /* network problem: keep the original error */ });
    }

    /** Runs one save at a time: the button is disabled until the request finishes (stops double submits). */
    function guarded(button, run) {
        return function () {
            if (button.disabled) { return; }
            var result = run();
            if (!result || typeof result.then !== "function") { return; }
            button.disabled = true;
            result.then(function () { button.disabled = false; }, function () { button.disabled = false; });
        };
    }

    /** Enter in a single-line field saves the form instead of posting the whole ASP.NET page back. */
    function saveOnEnter(container, save) {
        container.addEventListener("keydown", function (e) {
            var tag = e.target.tagName;
            if (e.key === "Enter" && (tag === "INPUT" || tag === "SELECT") && e.target.type !== "file") {
                e.preventDefault();
                save();
            }
        });
    }

    /** Rejects when a page method returned { error }, so every caller can share one .catch. */
    function check(result) {
        if (result && result.error) { throw new Error(result.error); }
        return result;
    }

    function byId(id) { return document.getElementById(id); }

    function setText(el, value) { el.textContent = value == null ? "" : String(value); }

    function toInt(value) { return parseInt(value, 10) || 0; }

    function makeButton(label, className, onClick) {
        var b = document.createElement("button");
        b.type = "button";
        b.className = className || "btn btn-secondary btn-small";
        b.textContent = label;
        b.addEventListener("click", onClick);
        return b;
    }

    /** The Active/Inactive toggle switch (.btn-status in components.css). */
    function makeStatusButton(status, onToggle) {
        var isActive = status === "Active";
        var b = makeButton(isActive ? "Active" : "Inactive",
            "btn btn-secondary btn-small btn-status " + (isActive ? "btn-status--active" : "btn-status--inactive"),
            onToggle);
        b.setAttribute("aria-label", b.textContent + ". Click to " + (isActive ? "deactivate" : "activate") + ".");
        return b;
    }

    /**
     * A Delete button that uses the site's confirm modal (Scripts/confirm-modal.js): the first click only opens
     * the modal; the modal's confirm repeats the click with data-confirmed="1", and only that click runs onConfirm.
     */
    function makeDeleteButton(message, onConfirm) {
        var b = makeButton("Delete", "btn btn-secondary btn-small btn-danger", function () {
            if (b.getAttribute("data-confirmed") === "1") { onConfirm(); }
        });
        b.setAttribute("data-confirm", message);
        return b;
    }

    function fillSelect(select, rows, blankLabel) {
        select.innerHTML = "";

        if (blankLabel) {
            var blank = document.createElement("option");
            blank.value = "";
            blank.textContent = blankLabel;
            select.appendChild(blank);
        }

        rows.forEach(function (row) {
            var option = document.createElement("option");
            option.value = row.id;
            option.textContent = row.title;
            select.appendChild(option);
        });
    }

    function showMessage(el, text, isError) {
        el.hidden = false;
        el.className = "alert alert-" + (isError ? "error" : "success");
        setText(el, text);
        window.clearTimeout(el._timer);
        el._timer = window.setTimeout(function () { el.hidden = true; }, isError ? 8000 : 4500);
    }

    function addCell(tr, value) {
        var td = document.createElement("td");
        setText(td, value);
        tr.appendChild(td);
    }

    // ------------------------------------------------------------------
    // Inline field errors: <p class="field-error"> under each field.
    // ------------------------------------------------------------------

    /** Collects field errors for one form, shows them under their fields and focuses the first one. */
    function createErrors() {
        var shown = [];

        return {
            add: function (errorId, message, input) {
                var el = byId(errorId);
                if (el) { setText(el, message); el.hidden = false; }
                if (input) { input.setAttribute("aria-invalid", "true"); }
                shown.push({ el: el, input: input });
            },
            any: function () { return shown.length > 0; },
            focusFirst: function () {
                var first = shown[0];
                if (!first) { return; }
                var target = first.input || first.el;
                if (target && target.scrollIntoView) { target.scrollIntoView({ block: "center" }); }
                if (first.input && first.input.focus) { first.input.focus({ preventScroll: true }); }
            }
        };
    }

    function clearErrors(errorIds, inputs) {
        errorIds.forEach(function (id) {
            var el = byId(id);
            if (el) { el.hidden = true; setText(el, ""); }
        });
        inputs.forEach(function (input) { if (input) { input.removeAttribute("aria-invalid"); } });
    }

    /** Clears one field's error as soon as the user edits that field. */
    function clearOnInput(input, errorId) {
        if (!input) { return; }
        var handler = function () {
            input.removeAttribute("aria-invalid");
            var el = byId(errorId);
            if (el) { el.hidden = true; }
        };
        input.addEventListener("input", handler);
        input.addEventListener("change", handler);
    }

    var setsPage = byId("psListView");
    var questionsPage = byId("qbListView");

    if (setsPage) { initPracticeSets(); }
    if (questionsPage) { initQuestionBank(); }

    // ------------------------------------------------------------------
    // Admin Practice Sets
    // ------------------------------------------------------------------
    function initPracticeSets() {
        var data = { categories: [], sets: [] };
        var editingId = 0;
        var toast = byId("psToast");
        var filter = byId("psCategoryFilter");
        var body = byId("psTableBody");
        var tableWrap = byId("psTableWrap");
        var empty = byId("psEmpty");
        var form = byId("psFormView");
        var formCategory = byId("psFormCategory");
        var formNumber = byId("psFormNumber");
        var categoryHint = byId("psCategoryHint");
        var usedNumbers = byId("psUsedNumbers");
        var errorIds = ["psCategoryError", "psNumberError"];

        function fail(e) { showMessage(toast, e.message, true); }

        function load() {
            return call("LoadData").then(function (result) {
                data = result;
                var selected = filter.value;
                fillSelect(filter, data.categories, "All Categories");
                filter.value = selected;
                fillSelect(formCategory, data.categories);
                render();
            }).catch(fail);
        }

        function categoryTitle(id) {
            var match = data.categories.filter(function (c) { return c.id === id; })[0];
            return match ? match.title : "Category";
        }

        function render() {
            var category = toInt(filter.value);
            var rows = data.sets.filter(function (s) { return !category || s.categoryId === category; });

            body.innerHTML = "";
            tableWrap.hidden = rows.length === 0;
            empty.hidden = rows.length > 0;

            // Nothing created yet: offer "Add". Nothing matches the filter: offer "Clear Filter".
            setText(byId("psEmptyTitle"), data.sets.length === 0
                ? "No practice sets have been created yet."
                : "No practice sets match the selected filter.");
            byId("psEmptyAdd").hidden = data.sets.length > 0;
            byId("psEmptyClear").hidden = data.sets.length === 0;
            byId("psClearFilter").hidden = !category;

            rows.forEach(function (set) {
                var tr = document.createElement("tr");
                if (set.status !== "Active") { tr.className = "row--inactive"; }

                addCell(tr, String(set.number));
                addCell(tr, categoryTitle(set.categoryId));
                addCell(tr, set.activeQuestionCount + " active / " + set.questionCount);

                var statusCell = document.createElement("td");
                statusCell.appendChild(makeStatusButton(set.status, function () {
                    call("Toggle", { id: set.id, active: set.status !== "Active" }).then(check).then(function () {
                        load();
                        showMessage(toast, "Practice Set updated.");
                    }).catch(fail);
                }));
                tr.appendChild(statusCell);

                var actionsCell = document.createElement("td");
                var actions = document.createElement("div");
                actions.className = "row-actions";
                actions.appendChild(makeButton("Edit", null, function () { openForm(set); }));
                actions.appendChild(makeDeleteButton("Delete Practice Set " + set.number + " (" + categoryTitle(set.categoryId) + ")?", function () {
                    call("Delete", { id: set.id }).then(function (message) {
                        if (message !== "Deleted") { throw new Error(message); }
                        load();
                        showMessage(toast, "Practice Set deleted.");
                    }).catch(fail);
                }));
                actionsCell.appendChild(actions);
                tr.appendChild(actionsCell);

                body.appendChild(tr);
            });
        }

        /** "Used Numbers: 1, 2, 4" for the selected category, as on the Chapters form. */
        function showUsedNumbers() {
            if (!usedNumbers || !window.UsedNumbers) { return; }
            var category = toInt(formCategory.value);
            var numbers = data.sets
                .filter(function (s) { return s.categoryId === category; })
                .map(function (s) { return s.number; });
            setText(usedNumbers, window.UsedNumbers.format(numbers));
        }

        function openForm(set) {
            editingId = set ? set.id : 0;
            clearErrors(errorIds, [formCategory, formNumber]);

            formCategory.value = set ? set.categoryId : (toInt(filter.value) || (data.categories[0] && data.categories[0].id));
            formNumber.value = set ? set.number : "";

            // A set's category cannot change while it has questions (the server enforces this too).
            var locked = !!(set && set.questionCount);
            formCategory.disabled = locked;
            categoryHint.hidden = !locked;

            var heading = set ? "Edit Practice Set" : "Add Practice Set";
            setText(byId("psFormHeading"), heading);
            setText(byId("psFormCrumb"), heading);
            byId("psStatusHint").hidden = !!set;
            showUsedNumbers();

            form.hidden = false;
            setsPage.hidden = true;
            window.scrollTo(0, 0);
            formNumber.focus();
        }

        function closeForm() {
            form.hidden = true;
            setsPage.hidden = false;
        }

        function validate() {
            var errors = createErrors();
            var category = toInt(formCategory.value);
            var number = formNumber.value.trim();

            clearErrors(errorIds, [formCategory, formNumber]);

            if (!category) {
                errors.add("psCategoryError", "Select a category.", formCategory);
            }

            if (!/^[1-9][0-9]{0,8}$/.test(number)) {
                errors.add("psNumberError", number ? "Enter a whole number from 1 upwards." : "Practice Set number is required.", formNumber);
            } else if (category && data.sets.some(function (s) { return s.id !== editingId && s.categoryId === category && s.number === toInt(number); })) {
                errors.add("psNumberError", categoryTitle(category) + " already has Practice Set " + toInt(number) + ". Please choose a different number.", formNumber);
            }

            errors.focusFirst();
            return !errors.any();
        }

        function save() {
            if (!validate()) { return null; }

            return call("Save", { id: editingId, categoryId: toInt(formCategory.value), number: toInt(formNumber.value) })
                .then(function (result) {
                    if (result && result.error) {
                        // The server's answer belongs to a field: a duplicate/number problem or the category lock.
                        var errors = createErrors();
                        var isCategory = /category/i.test(result.error) && !/already has/i.test(result.error);
                        errors.add(isCategory ? "psCategoryError" : "psNumberError", result.error, isCategory ? formCategory : formNumber);
                        errors.focusFirst();
                        return;
                    }

                    closeForm();
                    load();
                    showMessage(toast, "Practice Set saved.");
                })
                .catch(fail);
        }

        byId("psAddBtn").onclick = function () { openForm(null); };
        byId("psEmptyAdd").onclick = function () { openForm(null); };
        byId("psCancelBtn").onclick = closeForm;
        var saveSet = guarded(byId("psSaveBtn"), save);
        byId("psSaveBtn").onclick = saveSet;
        saveOnEnter(form, saveSet);
        filter.onchange = render;
        formCategory.addEventListener("change", showUsedNumbers);
        clearOnInput(formCategory, "psCategoryError");
        clearOnInput(formNumber, "psNumberError");

        ["psClearFilter", "psEmptyClear"].forEach(function (id) {
            var clear = byId(id);
            if (clear) { clear.onclick = function () { filter.value = ""; render(); }; }
        });

        load();
    }

    // ------------------------------------------------------------------
    // Admin Question Bank
    // ------------------------------------------------------------------
    function initQuestionBank() {
        var data = { categories: [], sets: [], questions: [] };
        var editingId = 0;
        var page = 1;
        var image = null;      // null, the question's stored path, or a new upload as a data URL
        var appRoot = questionsPage.getAttribute("data-app-root") || "/";
        var toast = byId("qbToast");
        var body = byId("qbTableBody");
        var form = byId("qbFormView");
        var filterCategory = byId("qbCategoryFilter");
        var filterSet = byId("qbSetFilter");
        var filterStatus = byId("qbStatusFilter");
        var search = byId("qbSearch");
        var formCategory = byId("qbFormCategory");
        var formSet = byId("qbFormSet");
        var formText = byId("qbFormText");
        var formExplanation = byId("qbFormExplanation");
        var formAlt = byId("qbFormImageAlt");
        var imageInput = byId("qbFormImage");
        var altGroup = byId("qbAltGroup");
        var previewWrap = byId("qbImagePreviewWrap");
        var previewImg = byId("qbImagePreview");
        var imageTools = byId("qbImageTools");
        var letters = ["A", "B", "C"];
        var optionInputs = letters.map(function (letter) { return byId("qbFormOption" + letter); });
        var errorIds = ["qbCategoryError", "qbSetError", "qbTextError", "qbImageError", "qbAltError", "qbOptionsError", "qbCorrectError", "qbExplanationError"];

        function radios() { return document.querySelectorAll("input[name='qbCorrectAnswer']"); }

        function fail(e) { showMessage(toast, e.message, true); }

        function load() {
            return call("LoadData").then(function (result) {
                data = result;
                var selected = filterCategory.value;
                fillSelect(filterCategory, data.categories, "All Categories");
                filterCategory.value = selected;
                fillSelect(formCategory, data.categories);
                fillSetFilter();
                render();
            }).catch(fail);
        }

        function setsIn(categoryId) {
            return data.sets.filter(function (s) { return s.categoryId === categoryId; });
        }

        /**
         * The Practice Set filter lists each set number once ("Practice Set 1" covers Practice Set 1 of every category);
         * the Category filter narrows it to one category. Values are set numbers, plus "none" for mock-exam-only questions.
         */
        function fillSetFilter() {
            var category = toInt(filterCategory.value);
            var sets = category ? setsIn(category) : data.sets;
            var selected = filterSet.value;
            var numbers = [];

            sets.forEach(function (s) { if (numbers.indexOf(s.number) < 0) { numbers.push(s.number); } });
            numbers.sort(function (a, b) { return a - b; });

            fillSelect(filterSet, [{ id: "none", title: "Mock exam only" }].concat(numbers.map(function (n) {
                return { id: n, title: "Practice Set " + n };
            })), "All Practice Sets");

            filterSet.value = selected;
            if (filterSet.value !== selected) { filterSet.value = ""; }
        }

        /** The form's set list: "mock exam only" first, then this category's sets. */
        function fillFormSets(selectedSetId) {
            var sets = setsIn(toInt(formCategory.value));

            fillSelect(formSet, sets.map(function (s) {
                return { id: s.id, title: "Practice Set " + s.number + (s.status === "Inactive" ? " (Inactive)" : "") };
            }), MOCK_EXAM_ONLY);

            formSet.value = selectedSetId ? String(selectedSetId) : "";
            byId("qbNoSets").hidden = sets.length > 0;
        }

        function categoryTitle(id) {
            var match = data.categories.filter(function (c) { return c.id === id; })[0];
            return match ? match.title : "";
        }

        function setNumber(id) {
            var match = data.sets.filter(function (s) { return s.id === id; })[0];
            return match ? match.number : 0;
        }

        function setTitle(id) {
            var match = data.sets.filter(function (s) { return s.id === id; })[0];
            return match ? String(match.number) : "Mock exam only";
        }

        function render() {
            var category = toInt(filterCategory.value);
            var set = filterSet.value;
            var status = filterStatus.value;
            var term = search.value.trim().toLowerCase();

            var rows = data.questions.filter(function (q) {
                return (!category || q.categoryId === category)
                    && (!set || (set === "none" ? q.setId == null : setNumber(q.setId) === toInt(set)))
                    && (!status || q.status === status)
                    && (!term || q.text.toLowerCase().indexOf(term) >= 0);
            });

            body.innerHTML = "";
            byId("qbTableWrap").hidden = rows.length === 0;
            byId("qbEmpty").hidden = rows.length > 0;
            byId("qbFooter").hidden = rows.length === 0;

            var pages = Math.max(1, Math.ceil(rows.length / QUESTIONS_PER_PAGE));
            page = Math.min(Math.max(1, page), pages);
            var start = (page - 1) * QUESTIONS_PER_PAGE;
            var pageRows = rows.slice(start, start + QUESTIONS_PER_PAGE);

            setText(byId("qbSummary"), rows.length
                ? "Showing " + (start + 1) + "\u2013" + (start + pageRows.length) + " of " + rows.length + (rows.length === 1 ? " question" : " questions")
                : "");
            renderPager(pages);

            // Nothing created yet: offer "Add". Nothing matches the filters: offer "Clear Filters".
            setText(byId("qbEmptyTitle"), data.questions.length === 0
                ? "No questions have been created yet."
                : "No questions match the selected filters.");
            byId("qbEmptyAdd").hidden = data.questions.length > 0;
            byId("qbEmptyClear").hidden = data.questions.length === 0;
            byId("qbClearFilters").hidden = !(category || set || status || term);

            pageRows.forEach(function (q) {
                var tr = document.createElement("tr");
                if (q.status !== "Active") { tr.className = "row--inactive"; }

                addCell(tr, q.text);
                addCell(tr, categoryTitle(q.categoryId));
                addCell(tr, setTitle(q.setId));

                var statusCell = document.createElement("td");
                statusCell.appendChild(makeStatusButton(q.status, function () {
                    call("Toggle", { id: q.id, active: q.status !== "Active" }).then(check).then(function (result) {
                        load();
                        showMessage(toast, result.notice ? "Question updated. " + result.notice : "Question updated.");
                    }).catch(fail);
                }));
                tr.appendChild(statusCell);

                var actionsCell = document.createElement("td");
                var actions = document.createElement("div");
                actions.className = "row-actions";
                actions.appendChild(makeButton("Edit", null, function () {
                    call("Get", { id: q.id }).then(check).then(openForm).catch(fail);
                }));
                actions.appendChild(makeDeleteButton("Delete this question?", function () {
                    call("Delete", { id: q.id }).then(function (message) {
                        if (message.indexOf("Deleted") !== 0) { throw new Error(message); }
                        load();
                        var notice = message.substring("Deleted".length).trim();
                        showMessage(toast, notice ? "Question deleted. " + notice : "Question deleted.");
                    }).catch(fail);
                }));
                actionsCell.appendChild(actions);
                tr.appendChild(actionsCell);

                body.appendChild(tr);
            });
        }

        /** Previous, page numbers and Next, styled like the Learner Records pager (.lr-pager). */
        function renderPager(pages) {
            var pager = byId("qbPager");
            pager.textContent = "";
            if (pages <= 1) { return; }

            function add(label, target, options) {
                var b = document.createElement("button");
                b.type = "button";
                b.className = "lr-pager__btn" + (options.active ? " is-active" : "");
                b.textContent = label;
                if (options.disabled) { b.disabled = true; }
                if (options.active) { b.setAttribute("aria-current", "page"); }
                if (options.ariaLabel) { b.setAttribute("aria-label", options.ariaLabel); }
                b.addEventListener("click", function () {
                    page = target;
                    render();
                    byId("qbTableWrap").scrollIntoView({ block: "start" });
                });
                pager.appendChild(b);
            }

            add("Previous", page - 1, { disabled: page === 1 });
            for (var i = 1; i <= pages; i++) {
                add(String(i), i, { active: i === page, ariaLabel: "Page " + i });
            }
            add("Next", page + 1, { disabled: page === pages });
        }

        /** Any filter change starts again from the first page. */
        function filterChanged() {
            page = 1;
            render();
        }

        /** Shows the current or newly chosen image with its Remove button, or hides both when there is none. */
        function showImage() {
            var hasImage = !!image;
            previewWrap.hidden = !hasImage;
            imageTools.hidden = !hasImage;
            altGroup.hidden = !hasImage;

            if (hasImage) {
                previewImg.src = image.indexOf("data:") === 0 ? image : appRoot + image;
                previewImg.alt = formAlt.value || "Question image";
            } else {
                previewImg.removeAttribute("src");
            }
        }

        /** Live "n / 500" counters under the question and explanation boxes. */
        function updateCounts() {
            [[formText, "qbTextCount"], [formExplanation, "qbExplanationCount"]].forEach(function (pair) {
                var count = byId(pair[1]);
                if (!pair[0] || !count) { return; }
                var length = pair[0].value.trim().length;
                count.textContent = String(length);
                if (count.parentNode) { count.parentNode.classList.toggle("char-counter--over", length > MAX_TEXT); }
            });
        }

        function allInputs() {
            return [formCategory, formSet, formText, imageInput, formAlt, formExplanation].concat(optionInputs);
        }

        function openForm(q) {
            editingId = q ? q.id : 0;
            clearErrors(errorIds, allInputs());

            formCategory.value = q ? q.categoryId : (toInt(filterCategory.value) || (data.categories[0] && data.categories[0].id));
            fillFormSets(q ? q.setId : null);

            formText.value = q ? q.text : "";
            formExplanation.value = q ? q.explanation : "";

            var buttons = radios();
            optionInputs.forEach(function (input, i) {
                input.value = q ? (q.options[i] || "") : "";
                buttons[i].checked = !!q && q.correctIndex === i;
            });

            formAlt.value = q ? (q.imageAlt || "") : "";
            image = q ? q.image : null;
            imageInput.value = "";
            showImage();
            updateCounts();

            var heading = q ? "Edit Question" : "Add Question";
            setText(byId("qbFormHeading"), heading);
            setText(byId("qbFormCrumb"), heading);
            byId("qbStatusHint").hidden = !!q;

            form.hidden = false;
            questionsPage.hidden = true;
            window.scrollTo(0, 0);
            formText.focus();
        }

        function closeForm() {
            form.hidden = true;
            questionsPage.hidden = false;
        }

        function correctIndex() {
            var correct = -1;
            Array.prototype.forEach.call(radios(), function (radio, i) { if (radio.checked) { correct = i; } });
            return correct;
        }

        /** Mirrors QuestionRepository.Validate, with each message under its own field. */
        function validate() {
            var errors = createErrors();
            var text = formText.value.trim();
            var explanation = formExplanation.value.trim();
            var options = optionInputs.map(function (input) { return input.value.trim(); });

            clearErrors(errorIds, allInputs());

            if (!toInt(formCategory.value)) {
                errors.add("qbCategoryError", "Select a category.", formCategory);
            }

            if (!text) {
                errors.add("qbTextError", "Question is required.", formText);
            } else if (text.length > MAX_TEXT) {
                errors.add("qbTextError", "Question must be 500 characters or fewer.", formText);
            }

            if (image && !formAlt.value.trim()) {
                errors.add("qbAltError", "Describe the image for screen reader users.", formAlt);
            } else if (formAlt.value.trim().length > MAX_ALT) {
                errors.add("qbAltError", "Image description must be 200 characters or fewer.", formAlt);
            }

            var emptyIndex = options.indexOf("");
            var longIndex = -1;
            options.forEach(function (o, i) { if (longIndex < 0 && o.length > MAX_OPTION) { longIndex = i; } });
            var lower = options.map(function (o) { return o.toLowerCase(); });
            var duplicate = lower.some(function (o, i) { return o && lower.indexOf(o) !== i; });

            if (emptyIndex >= 0) {
                errors.add("qbOptionsError", "Options A, B and C are all required.", optionInputs[emptyIndex]);
            } else if (longIndex >= 0) {
                errors.add("qbOptionsError", "Each option must be 200 characters or fewer.", optionInputs[longIndex]);
            } else if (duplicate) {
                errors.add("qbOptionsError", "Each option must be different.", optionInputs[0]);
            }

            if (correctIndex() < 0) {
                errors.add("qbCorrectError", "Select the correct answer.", radios()[0]);
            }

            if (!explanation) {
                errors.add("qbExplanationError", "Explanation is required.", formExplanation);
            } else if (explanation.length > MAX_TEXT) {
                errors.add("qbExplanationError", "Explanation must be 500 characters or fewer.", formExplanation);
            }

            errors.focusFirst();
            return !errors.any();
        }

        /** Places a server message under the field it is about; anything else goes to the message box. */
        function showServerError(message) {
            var rules = [
                [/not found|try again|question data/i, null, null],
                [/^select a category|category can't change/i, "qbCategoryError", formCategory],
                [/practice set/i, "qbSetError", formSet],
                [/^question/i, "qbTextError", formText],
                [/image description/i, "qbAltError", formAlt],
                [/image|jpg|png|webp|2 mb/i, "qbImageError", imageInput],
                [/option/i, "qbOptionsError", optionInputs[0]],
                [/correct answer/i, "qbCorrectError", radios()[0]],
                [/explanation/i, "qbExplanationError", formExplanation]
            ];
            var match = rules.filter(function (r) { return r[0].test(message); })[0];
            var errors = createErrors();

            if (match && match[1]) {
                errors.add(match[1], message, match[2]);
                errors.focusFirst();
            } else {
                fail(new Error(message));
                window.scrollTo(0, 0);
            }
        }

        function save() {
            if (!validate()) { return null; }

            var question = {
                id: editingId,
                categoryId: toInt(formCategory.value),
                setId: toInt(formSet.value) || null,
                text: formText.value,
                explanation: formExplanation.value,
                image: image,
                imageAlt: image ? formAlt.value : null,
                options: optionInputs.map(function (input) { return input.value; }),
                correctIndex: correctIndex()
            };

            return call("Save", { question: question }).then(function (result) {
                if (result && result.error) {
                    showServerError(result.error);
                    return;
                }

                closeForm();
                load();
                showMessage(toast, result.notice ? "Question saved. " + result.notice : "Question saved.");
            }).catch(fail);
        }

        byId("qbAddBtn").onclick = function () { openForm(null); };
        byId("qbEmptyAdd").onclick = function () { openForm(null); };
        byId("qbCancelBtn").onclick = closeForm;
        var saveQuestion = guarded(byId("qbSaveBtn"), save);
        byId("qbSaveBtn").onclick = saveQuestion;
        saveOnEnter(form, saveQuestion);
        filterCategory.onchange = function () { fillSetFilter(); filterChanged(); };
        filterSet.onchange = filterChanged;
        filterStatus.onchange = filterChanged;
        search.oninput = filterChanged;
        formCategory.addEventListener("change", function () { fillFormSets(null); });
        formText.addEventListener("input", updateCounts);
        formExplanation.addEventListener("input", updateCounts);

        clearOnInput(formCategory, "qbCategoryError");
        clearOnInput(formSet, "qbSetError");
        clearOnInput(formText, "qbTextError");
        clearOnInput(formAlt, "qbAltError");
        clearOnInput(formExplanation, "qbExplanationError");
        optionInputs.forEach(function (input) { clearOnInput(input, "qbOptionsError"); });
        Array.prototype.forEach.call(radios(), function (radio) { clearOnInput(radio, "qbCorrectError"); });

        ["qbClearFilters", "qbEmptyClear"].forEach(function (id) {
            var clear = byId(id);
            if (!clear) { return; }

            clear.onclick = function () {
                filterCategory.value = "";
                fillSetFilter();
                filterSet.value = "";
                filterStatus.value = "";
                search.value = "";
                filterChanged();
            };
        });

        byId("qbRemoveImage").onclick = function () {
            image = null;
            imageInput.value = "";
            formAlt.value = "";
            showImage();
        };

        imageInput.onchange = function () {
            var file = imageInput.files[0];
            var errors = createErrors();

            clearErrors(["qbImageError"], [imageInput]);
            if (!file) { return; }

            if (IMAGE_TYPES.indexOf(file.type) < 0) {
                errors.add("qbImageError", "Choose a JPG, PNG or WEBP image.", imageInput);
            } else if (file.size > MAX_IMAGE_BYTES) {
                errors.add("qbImageError", "Image must be 2 MB or smaller.", imageInput);
            }

            if (errors.any()) {
                imageInput.value = "";
                return;
            }

            var reader = new FileReader();
            reader.onload = function () {
                image = reader.result;
                showImage();
                formAlt.focus();
            };
            reader.readAsDataURL(file);
        };

        load();
    }
})();
