/*
 * Database-backed Admin Practice Sets and Question Bank.
 * Both pages are shells; this script calls the page's own page methods (Admin/PracticeSets.aspx and
 * Admin/QuestionBank.aspx code-behind) with JSON over POST and redraws the table from the result.
 * A method reports a validation problem as { error: "..." }; a server failure becomes a rejected promise.
 */
(function () {
    "use strict";

    var MOCK_EXAM_ONLY = "No practice set (mock exam only)";

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
                    throw new Error((json && json.Message) || "The request could not be completed. Please try again.");
                }

                return json.d;
            });
        });
    }

    /** Rejects when a page method returned { error }, so every caller can share one .catch. */
    function check(result) {
        if (result && result.error) { throw new Error(result.error); }
        return result;
    }

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
        el._timer = window.setTimeout(function () { el.hidden = true; }, 4500);
    }

    function addCell(tr, value) {
        var td = document.createElement("td");
        setText(td, value);
        tr.appendChild(td);
    }

    var setsPage = document.getElementById("psListView");
    var questionsPage = document.getElementById("qbListView");

    if (setsPage) { initPracticeSets(); }
    if (questionsPage) { initQuestionBank(); }

    // ------------------------------------------------------------------
    // Admin Practice Sets
    // ------------------------------------------------------------------
    function initPracticeSets() {
        var data = { categories: [], sets: [] };
        var editingId = 0;
        var toast = document.getElementById("psToast");
        var filter = document.getElementById("psCategoryFilter");
        var body = document.getElementById("psTableBody");
        var tableWrap = document.getElementById("psTableWrap");
        var empty = document.getElementById("psEmpty");
        var form = document.getElementById("psFormView");
        var formCategory = document.getElementById("psFormCategory");
        var formNumber = document.getElementById("psFormNumber");

        function fail(e) { showMessage(toast, e.message, true); }

        function load() {
            call("LoadData").then(function (result) {
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
            setText(document.getElementById("psEmptyTitle"), data.sets.length === 0
                ? "No practice sets have been created yet."
                : "No practice sets match the selected filter.");
            document.getElementById("psEmptyAdd").hidden = data.sets.length > 0;
            document.getElementById("psEmptyClear").hidden = data.sets.length === 0;
            document.getElementById("psClearFilter").hidden = !category;

            rows.forEach(function (set) {
                var tr = document.createElement("tr");
                if (set.status !== "Active") { tr.className = "row--inactive"; }

                addCell(tr, "Practice Set " + set.number);
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

        function openForm(set) {
            editingId = set ? set.id : 0;
            formCategory.value = set ? set.categoryId : (toInt(filter.value) || (data.categories[0] && data.categories[0].id));
            formNumber.value = set ? set.number : "";
            // A set's category cannot change while it has questions (the server enforces this too).
            formCategory.disabled = !!(set && set.questionCount);
            setText(document.getElementById("psFormHeading"), set ? "Edit Practice Set" : "Add Practice Set");
            form.hidden = false;
            setsPage.hidden = true;
        }

        function closeForm() {
            form.hidden = true;
            setsPage.hidden = false;
        }

        document.getElementById("psAddBtn").onclick = function () { openForm(null); };
        document.getElementById("psEmptyAdd").onclick = function () { openForm(null); };
        document.getElementById("psCancelBtn").onclick = closeForm;
        filter.onchange = render;

        ["psClearFilter", "psEmptyClear"].forEach(function (id) {
            var clear = document.getElementById(id);
            if (clear) { clear.onclick = function () { filter.value = ""; render(); }; }
        });

        document.getElementById("psSaveBtn").onclick = function () {
            call("Save", { id: editingId, categoryId: toInt(formCategory.value), number: toInt(formNumber.value) })
                .then(check)
                .then(function () {
                    closeForm();
                    load();
                    showMessage(toast, "Practice Set saved.");
                })
                .catch(fail);
        };

        load();
    }

    // ------------------------------------------------------------------
    // Admin Question Bank
    // ------------------------------------------------------------------
    function initQuestionBank() {
        var data = { categories: [], sets: [], questions: [] };
        var editingId = 0;
        var image = null;      // null, the question's stored path, or a new upload as a data URL
        var toast = document.getElementById("qbToast");
        var body = document.getElementById("qbTableBody");
        var form = document.getElementById("qbFormView");
        var filterCategory = document.getElementById("qbCategoryFilter");
        var filterSet = document.getElementById("qbSetFilter");
        var filterStatus = document.getElementById("qbStatusFilter");
        var search = document.getElementById("qbSearch");
        var formCategory = document.getElementById("qbFormCategory");
        var formSet = document.getElementById("qbFormSet");
        var imageInput = document.getElementById("qbFormImage");
        var altGroup = document.getElementById("qbAltGroup");
        var letters = ["A", "B", "C"];

        function fail(e) { showMessage(toast, e.message, true); }

        function load() {
            call("LoadData").then(function (result) {
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

        function fillSetFilter() {
            var category = toInt(filterCategory.value);
            var sets = category ? setsIn(category) : data.sets;
            var selected = filterSet.value;

            fillSelect(filterSet, [{ id: "none", title: "Mock exam only" }].concat(sets.map(function (s) {
                return { id: s.id, title: "Practice Set " + s.number };
            })), "All Practice Sets");

            filterSet.value = selected;
        }

        /** The form's set list: "mock exam only" first, then this category's sets. */
        function fillFormSets(selectedSetId) {
            fillSelect(formSet, setsIn(toInt(formCategory.value)).map(function (s) {
                return { id: s.id, title: "Practice Set " + s.number + (s.status === "Inactive" ? " (Inactive)" : "") };
            }), MOCK_EXAM_ONLY);

            formSet.value = selectedSetId ? String(selectedSetId) : "";
        }

        function categoryTitle(id) {
            var match = data.categories.filter(function (c) { return c.id === id; })[0];
            return match ? match.title : "";
        }

        function setTitle(id) {
            var match = data.sets.filter(function (s) { return s.id === id; })[0];
            return match ? "Practice Set " + match.number : "Mock exam only";
        }

        function render() {
            var category = toInt(filterCategory.value);
            var set = filterSet.value;
            var status = filterStatus.value;
            var term = search.value.trim().toLowerCase();

            var rows = data.questions.filter(function (q) {
                return (!category || q.categoryId === category)
                    && (!set || (set === "none" ? q.setId == null : q.setId === toInt(set)))
                    && (!status || q.status === status)
                    && (!term || q.text.toLowerCase().indexOf(term) >= 0);
            });

            body.innerHTML = "";
            document.getElementById("qbTableWrap").hidden = rows.length === 0;
            document.getElementById("qbEmpty").hidden = rows.length > 0;

            // Nothing created yet: offer "Add". Nothing matches the filters: offer "Clear Filters".
            setText(document.getElementById("qbEmptyTitle"), data.questions.length === 0
                ? "No questions have been created yet."
                : "No questions match the selected filters.");
            document.getElementById("qbEmptyAdd").hidden = data.questions.length > 0;
            document.getElementById("qbEmptyClear").hidden = data.questions.length === 0;
            document.getElementById("qbClearFilters").hidden = !(category || set || status || term);

            rows.forEach(function (q) {
                var tr = document.createElement("tr");
                if (q.status !== "Active") { tr.className = "row--inactive"; }

                addCell(tr, q.text);
                addCell(tr, categoryTitle(q.categoryId));
                addCell(tr, setTitle(q.setId));

                var statusCell = document.createElement("td");
                statusCell.appendChild(makeStatusButton(q.status, function () {
                    call("Toggle", { id: q.id, active: q.status !== "Active" }).then(check).then(function () {
                        load();
                        showMessage(toast, "Question updated.");
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
                        if (message !== "Deleted") { throw new Error(message); }
                        load();
                        showMessage(toast, "Question deleted.");
                    }).catch(fail);
                }));
                actionsCell.appendChild(actions);
                tr.appendChild(actionsCell);

                body.appendChild(tr);
            });
        }

        function openForm(q) {
            editingId = q ? q.id : 0;
            formCategory.value = q ? q.categoryId : (toInt(filterCategory.value) || (data.categories[0] && data.categories[0].id));
            fillFormSets(q ? q.setId : null);

            document.getElementById("qbFormText").value = q ? q.text : "";
            document.getElementById("qbFormExplanation").value = q ? q.explanation : "";

            var radios = document.querySelectorAll("input[name='qbCorrectAnswer']");
            letters.forEach(function (letter, i) {
                document.getElementById("qbFormOption" + letter).value = q ? (q.options[i] || "") : "";
                radios[i].checked = !!q && q.correctIndex === i;
            });

            document.getElementById("qbFormImageAlt").value = q ? (q.imageAlt || "") : "";
            image = q ? q.image : null;
            imageInput.value = "";
            altGroup.hidden = !image;

            setText(document.getElementById("qbFormHeading"), q ? "Edit Question" : "Add Question");
            form.hidden = false;
            questionsPage.hidden = true;
        }

        function closeForm() {
            form.hidden = true;
            questionsPage.hidden = false;
        }

        function save() {
            var correct = -1;
            Array.prototype.forEach.call(document.querySelectorAll("input[name='qbCorrectAnswer']"), function (radio, i) {
                if (radio.checked) { correct = i; }
            });

            var question = {
                id: editingId,
                categoryId: toInt(formCategory.value),
                setId: toInt(formSet.value) || null,
                text: document.getElementById("qbFormText").value,
                explanation: document.getElementById("qbFormExplanation").value,
                image: image,
                imageAlt: document.getElementById("qbFormImageAlt").value,
                options: letters.map(function (letter) { return document.getElementById("qbFormOption" + letter).value; }),
                correctIndex: correct
            };

            call("Save", { question: question }).then(check).then(function () {
                closeForm();
                load();
                showMessage(toast, "Question saved.");
            }).catch(fail);
        }

        document.getElementById("qbAddBtn").onclick = function () { openForm(null); };
        document.getElementById("qbEmptyAdd").onclick = function () { openForm(null); };
        document.getElementById("qbCancelBtn").onclick = closeForm;
        document.getElementById("qbSaveBtn").onclick = save;
        filterCategory.onchange = function () { fillSetFilter(); render(); };
        filterSet.onchange = render;
        filterStatus.onchange = render;
        search.oninput = render;
        formCategory.onchange = function () { fillFormSets(null); };

        ["qbClearFilters", "qbEmptyClear"].forEach(function (id) {
            var clear = document.getElementById(id);
            if (!clear) { return; }

            clear.onclick = function () {
                filterCategory.value = "";
                fillSetFilter();
                filterSet.value = "";
                filterStatus.value = "";
                search.value = "";
                render();
            };
        });

        document.getElementById("qbRemoveImage").onclick = function () {
            image = null;
            imageInput.value = "";
            altGroup.hidden = true;
        };

        imageInput.onchange = function () {
            var file = imageInput.files[0];
            if (!file) { return; }

            if (file.size > 2 * 1024 * 1024 || ["image/jpeg", "image/png", "image/webp"].indexOf(file.type) < 0) {
                showMessage(toast, "Choose a JPG, PNG or WEBP image up to 2 MB.", true);
                imageInput.value = "";
                return;
            }

            var reader = new FileReader();
            reader.onload = function () {
                image = reader.result;
                altGroup.hidden = false;
            };
            reader.readAsDataURL(file);
        };

        load();
    }
})();
