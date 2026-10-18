/*
 * ============================================================================
 * Phase 6 frontend-only Admin UI prototype for Mock Exam Sets.
 *
 * // Phase 6 frontend-only Mock Examination data.
 * // Replace with database-backed Mock Exam data in a later phase.
 * // Demo content is not official JPJ/KPP examination content.
 *
 * This file performs NO server calls and NO database operations. Exam types,
 * the sets and the demo question bank come from window.MockExamData
 * (Scripts/mock-exam-demo.js, which must be loaded first), so the sets and
 * their questions here are the same ones the learner exam is built from.
 * The state below is local and in memory: Add/Edit/Delete, status toggles and
 * question selection are UI-only, and a page refresh resets everything back to
 * the original mock data.
 * ============================================================================
 */
(function () {
    "use strict";

    var root = document.getElementById("mxListView");

    if (!root || !window.MockExamData) { return; }

    var NOTE = " (Prototype only: not saved.)";

    function byId(id) { return document.getElementById(id); }

    function setCrumb(crumbId, text) {
        var crumb = byId(crumbId);
        if (crumb) { crumb.textContent = text; }
    }

    // With a message the toast is rewritten (type: "success" by default, "warning" or "error").
    function showToast(el, message, type) {
        el.textContent = message;
        el.className = "alert alert-" + (type || "success");
        el.hidden = false;
        window.clearTimeout(el._hideTimer);
        el._hideTimer = window.setTimeout(function () { el.hidden = true; }, 6000);
    }

    var types = MockExamData.getExamTypes();
    var bank = MockExamData.getQuestionBank();

    // The bank grouped by exam section: Colour Blind, then one list per theory topic.
    function sectionOf(q) { return q.section === "colourBlind" ? "colourBlind" : q.topic; }

    function bankForSection(sectionId) {
        return bank.filter(function (q) { return sectionOf(q) === sectionId; });
    }

    var sectionById = {};
    bank.forEach(function (q) { sectionById[q.id] = sectionOf(q); });

    function questionPrefix(q) {
        return q.section === "colourBlind" ? "CB-" : "TH-";
    }

    function typeById(id) {
        return types.filter(function (t) { return t.id === id; })[0];
    }

    function typeIndex(id) {
        return types.indexOf(typeById(id));
    }

    // ---- mock sets (local mutable state) ----
    // Same sets and statuses the learner side uses, each with the questions the learner exam is actually built from.
    var sets = [];
    var nextLocalId = 1;

    types.forEach(function (type) {
        MockExamData.getSets(type.id).forEach(function (seed) {
            sets.push({
                id: "demo-mock-" + (nextLocalId++),
                typeId: type.id,
                number: seed.setNumber,
                status: seed.status,
                selected: MockExamData.buildExam(type.id, seed.setNumber).map(function (q) { return q.id; })
            });
        });
    });

    // ---- completeness of a set's question selection ----
    function sectionCounts(set) {
        var counts = {};
        set.selected.forEach(function (id) {
            var sectionId = sectionById[id];
            if (sectionId) { counts[sectionId] = (counts[sectionId] || 0) + 1; }
        });
        return counts;
    }

    function selectedTotal(set) {
        var counts = sectionCounts(set);
        return typeById(set.typeId).sections.reduce(function (sum, section) {
            return sum + Math.min(counts[section.id] || 0, section.count);
        }, 0);
    }

    function isComplete(set) {
        var counts = sectionCounts(set);
        return typeById(set.typeId).sections.every(function (section) { return (counts[section.id] || 0) === section.count; });
    }

    function setLabel(set) { return MockExamData.setLabel(set.number); }

    function findSet(id) {
        return sets.filter(function (s) { return s.id === id; })[0];
    }

    var typeFilter = byId("mxTypeFilter");
    var statusFilter = byId("mxStatusFilter");
    var clearFiltersBtn = byId("mxClearFilters");
    var tableWrap = byId("mxTableWrap");
    var tableBody = byId("mxTableBody");
    var empty = byId("mxEmpty");
    var emptyTitle = byId("mxEmptyTitle");
    var emptyAddBtn = byId("mxEmptyAdd");
    var emptyClearBtn = byId("mxEmptyClear");
    var toast = byId("mxToast");
    var addBtn = byId("mxAddBtn");

    var listView = byId("mxListView");
    var formView = byId("mxFormView");
    var questionsView = byId("mxQuestionsView");

    var formHeading = byId("mxFormHeading");
    var formType = byId("mxFormType");
    var typeChangeHint = byId("mxTypeChangeHint");
    var typeError = byId("mxTypeError");
    var formNumber = byId("mxFormNumber");
    var numberError = byId("mxNumberError");
    var usedNumbers = byId("mxUsedNumbers");
    var formRules = byId("mxFormRules");
    var statusHint = byId("mxStatusHint");
    var saveBtn = byId("mxSaveBtn");
    var cancelBtn = byId("mxCancelBtn");

    var pvType = byId("pvType");
    var pvSetTitle = byId("pvSetTitle");
    var pvMeta = byId("pvMeta");

    var editingId = null;
    var lastAuto = null;           // the number suggested automatically (so typing your own is never overwritten)
    var questionSet = null;
    var questionSelection = null;  // Set-like object: id -> true

    function populateSelect(select, items, valueField, textField) {
        select.innerHTML = "";
        items.forEach(function (item) {
            var option = document.createElement("option");
            option.value = item[valueField];
            option.textContent = item[textField];
            select.appendChild(option);
        });
    }

    populateSelect(typeFilter, [{ id: "", title: "All Exam Types" }].concat(types), "id", "title");
    populateSelect(formType, types, "id", "title");

    function showView(name) {
        listView.hidden = name !== "list";
        formView.hidden = name !== "form";
        questionsView.hidden = name !== "questions";
    }

    // ---- shared helpers ----
    function statusButton(set) {
        var button = document.createElement("button");
        button.type = "button";
        var isActive = set.status === "Active";
        button.textContent = isActive ? "Active" : "Inactive";
        button.className = "btn btn-secondary btn-small btn-status " + (isActive ? "btn-status--active" : "btn-status--inactive");
        button.setAttribute("aria-label", button.textContent + ". Click to " + (isActive ? "deactivate" : "activate") + ".");
        button.addEventListener("click", function () { toggleStatus(set); });
        return button;
    }

    function actionButton(label, handler) {
        var button = document.createElement("button");
        button.type = "button";
        button.className = "btn btn-secondary btn-small";
        button.textContent = label;
        button.addEventListener("click", handler);
        return button;
    }

    function appendDetail(dl, label, value) {
        var dt = document.createElement("dt");
        dt.textContent = label;
        var dd = document.createElement("dd");
        dd.textContent = value;
        dl.appendChild(dt);
        dl.appendChild(dd);
    }

    function appendRules(dl, type) {
        appendDetail(dl, "Colour Blind Questions", String(type.colourBlindCount));
        appendDetail(dl, "Theory Questions", String(type.theoryCount));
        appendDetail(dl, "Total Questions", String(MockExamData.totalQuestions(type)));
        appendDetail(dl, "Time Limit", type.minutes + " minutes");
        appendDetail(dl, "Colour Blind Passing Score", type.colourBlindPass + " / " + type.colourBlindCount);
        appendDetail(dl, "Theory Passing Score", type.theoryPass + " / " + type.theoryCount);
    }

    function isFiltering() {
        return !!(typeFilter.value || statusFilter.value);
    }

    // ---- table ----
    function renderTable() {
        var filtered = sets
            .filter(function (s) { return (!typeFilter.value || s.typeId === typeFilter.value) && (!statusFilter.value || s.status === statusFilter.value); })
            .sort(function (a, b) { return typeIndex(a.typeId) - typeIndex(b.typeId) || a.number - b.number; });

        tableBody.innerHTML = "";
        tableWrap.hidden = filtered.length === 0;
        empty.hidden = filtered.length !== 0;
        clearFiltersBtn.hidden = !isFiltering();

        // Nothing created yet: offer "Add". Nothing matches the filters: offer "Clear Filters".
        emptyTitle.textContent = sets.length === 0 ? "No mock exam sets have been created yet." : "No mock exam sets match the selected filters.";
        emptyAddBtn.hidden = sets.length !== 0;
        emptyClearBtn.hidden = sets.length === 0;

        filtered.forEach(function (set) {
            var type = typeById(set.typeId);
            var row = document.createElement("tr");
            if (set.status !== "Active") { row.className = "row--inactive"; }

            var setCell = document.createElement("td");
            setCell.className = "data-table__nowrap";
            var strong = document.createElement("strong");
            strong.textContent = String(set.number);
            setCell.appendChild(strong);
            row.appendChild(setCell);

            var typeCell = document.createElement("td");
            typeCell.textContent = type.title;
            row.appendChild(typeCell);

            // Questions actually chosen for this set, against what the exam type requires.
            var total = MockExamData.totalQuestions(type);
            var chosen = selectedTotal(set);
            var questionsCell = document.createElement("td");
            questionsCell.className = "data-table__nowrap";
            var progress = document.createElement("span");
            progress.className = "mx-qcount" + (isComplete(set) ? " mx-qcount--ok" : "");
            progress.textContent = chosen + " / " + total;
            progress.title = isComplete(set) ? "All sections have the required number of questions." : "Incomplete: open Manage Questions to finish the selection.";
            questionsCell.appendChild(progress);
            row.appendChild(questionsCell);

            var timeCell = document.createElement("td");
            timeCell.textContent = type.minutes + " min";
            row.appendChild(timeCell);

            var statusCell = document.createElement("td");
            statusCell.appendChild(statusButton(set));
            row.appendChild(statusCell);

            var actionsCell = document.createElement("td");
            var actions = document.createElement("div");
            actions.className = "row-actions";
            actions.appendChild(actionButton("Edit", function () { openForm(set); }));
            actions.appendChild(actionButton("Questions", function () { openQuestions(set); }));
            actions.appendChild(deleteButton(set));
            actionsCell.appendChild(actions);
            row.appendChild(actionsCell);

            tableBody.appendChild(row);
        });
    }

    // The shared confirm dialog (confirm-modal.js) asks first; the click handler only runs once it is confirmed.
    function deleteButton(set) {
        var button = document.createElement("button");
        button.type = "button";
        button.className = "btn btn-secondary btn-small btn-danger";
        button.textContent = "Delete";
        button.setAttribute("data-confirm", "Delete " + setLabel(set) + " (" + typeById(set.typeId).title + ")?"
            + (set.status === "Active" ? " It is Active, so learners will no longer be able to start it." : ""));
        button.setAttribute("data-confirm-title", "Delete Mock Exam Set?");
        button.addEventListener("click", function () {
            var wasActive = set.status === "Active";
            sets = sets.filter(function (s) { return s.id !== set.id; });
            renderTable();

            if (wasActive && noActiveSetsLeft(set.typeId)) {
                showToast(toast, setLabel(set) + " deleted. " + typeById(set.typeId).title + " has no active sets now, so learners can't start this exam." + NOTE, "warning");
            } else {
                showToast(toast, "Mock exam set deleted." + NOTE);
            }
        });
        return button;
    }

    function noActiveSetsLeft(typeId) {
        return !sets.some(function (s) { return s.typeId === typeId && s.status === "Active"; });
    }

    function toggleStatus(set) {
        var label = setLabel(set);

        if (set.status === "Active") {
            set.status = "Inactive";
            renderTable();

            if (noActiveSetsLeft(set.typeId)) {
                showToast(toast, label + " deactivated. " + typeById(set.typeId).title + " has no active sets now, so learners can't start this exam." + NOTE, "warning");
            } else {
                showToast(toast, label + " deactivated." + NOTE);
            }
            return;
        }

        // An incomplete set would give learners an exam with missing questions.
        if (!isComplete(set)) {
            showToast(toast, label + " can't be activated until every section has the required number of questions ("
                + selectedTotal(set) + " / " + MockExamData.totalQuestions(typeById(set.typeId)) + " selected). Use Manage Questions to finish the selection.", "error");
            return;
        }

        set.status = "Active";
        renderTable();
        showToast(toast, label + " activated." + NOTE);
    }

    // ---- add / edit ----
    function usedInType() {
        return sets.filter(function (s) { return s.typeId === formType.value; }).map(function (s) { return s.number; });
    }

    // "Used Numbers" for the CURRENT Exam Type only; informational, the duplicate check below is unchanged.
    function renderUsedNumbers() {
        if (!usedNumbers || !window.UsedNumbers) { return; }
        usedNumbers.textContent = UsedNumbers.format(usedInType());
    }

    function renderFormRules() {
        formRules.innerHTML = "";
        appendRules(formRules, typeById(formType.value));
    }

    // Adding: suggest the next free number for the chosen exam type, unless the admin typed their own.
    function suggestNumber() {
        if (editingId) { return; }
        if (formNumber.value !== "" && formNumber.value !== lastAuto) { return; }

        var max = 0;
        usedInType().forEach(function (n) { if (n > max) { max = n; } });
        lastAuto = String(max + 1);
        formNumber.value = lastAuto;
    }

    // Same look as the learner's mock exam set card; only the type and the number are live.
    function updatePreview() {
        var type = typeById(formType.value);
        var raw = formNumber.value.trim();
        var valid = /^[1-9][0-9]{0,8}$/.test(raw);

        pvType.textContent = type.title;
        pvSetTitle.textContent = valid ? MockExamData.setLabel(parseInt(raw, 10)) : "Mock Exam Set";
        pvMeta.textContent = MockExamData.totalQuestions(type) + " Questions · " + type.minutes + " minutes";
    }

    function updateTypeChangeHint() {
        var original = editingId ? findSet(editingId) : null;
        typeChangeHint.hidden = !(original && original.typeId !== formType.value);
    }

    function clearErrors() {
        typeError.hidden = true;
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
            formHeading.textContent = "Edit Mock Exam Set";
            setCrumb("mxFormCrumb", "Edit Mock Exam Set");
            formType.value = set.typeId;
            formNumber.value = String(set.number);
            statusHint.hidden = true;
        } else {
            formHeading.textContent = "Add Mock Exam Set";
            setCrumb("mxFormCrumb", "Add Mock Exam Set");
            formType.value = typeFilter.value || types[0].id;       // start in the exam type being filtered, if any
            formNumber.value = "";
            statusHint.hidden = false;
            suggestNumber();
        }

        renderFormRules();
        renderUsedNumbers();
        updateTypeChangeHint();
        updatePreview();
        showView("form");
        formType.focus();
    }

    function backToList() {
        showView("list");
        addBtn.focus();
    }

    formType.addEventListener("change", function () {
        clearErrors();
        renderFormRules();
        renderUsedNumbers();
        suggestNumber();
        updateTypeChangeHint();
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

    function trimSelection(ids, type) {
        var used = {};

        return ids.filter(function (id) {
            var sectionId = sectionById[id];
            var section = type.sections.filter(function (x) { return x.id === sectionId; })[0];

            if (!section) { return false; }

            used[sectionId] = (used[sectionId] || 0) + 1;
            return used[sectionId] <= section.count;
        });
    }

    saveBtn.addEventListener("click", function () {
        clearErrors();

        var raw = formNumber.value.trim();

        if (!formType.value) {
            fail(typeError, "Please select an exam type.", formType);
            return;
        }

        if (!/^[1-9][0-9]{0,8}$/.test(raw)) {
            fail(numberError, "Mock Exam Set number must be a whole number greater than 0 (for example 1, 2, 3).", formNumber);
            return;
        }

        var number = parseInt(raw, 10);

        var duplicate = sets.some(function (s) {
            return s.id !== editingId && s.typeId === formType.value && s.number === number;
        });

        if (duplicate) {
            fail(numberError, typeById(formType.value).title + " already has " + MockExamData.setLabel(number) + ". Please choose a different number.", formNumber);
            return;
        }

        var message;
        var type = "success";

        if (editingId) {
            var existing = findSet(editingId);
            message = "Mock exam set updated.";

            if (existing.typeId !== formType.value) {
                // Different exam type means different required counts: keep what still fits, and
                // never leave a half-filled set Active.
                existing.selected = trimSelection(existing.selected, typeById(formType.value));
                existing.typeId = formType.value;

                if (!isComplete(existing)) {
                    var wasActive = existing.status === "Active";
                    existing.status = "Inactive";
                    message = "Exam type changed. Questions that no longer fit were removed" + (wasActive ? " and the set is now Inactive" : "")
                        + ". Use Manage Questions to complete the selection.";
                    type = "warning";
                }
            }

            existing.number = number;
        } else {
            // Inactive until its questions are selected (see the hint on the form).
            sets.push({
                id: "demo-mock-" + (nextLocalId++),
                typeId: formType.value,
                number: number,
                status: "Inactive",
                selected: []
            });
            message = "Mock exam set created as Inactive. Use Manage Questions to select its questions, then activate it.";
        }

        renderTable();
        backToList();
        showToast(toast, message + NOTE, type);
    });

    cancelBtn.addEventListener("click", backToList);

    // The breadcrumb behaves like Cancel (a normal link would reload the page and throw away every unsaved mock edit).
    formView.querySelector(".breadcrumb a").addEventListener("click", function (event) {
        event.preventDefault();
        backToList();
    });

    // ---- manage questions (selection UI only) ----
    // One group per exam section (Colour Blind Test, Road Signs, Rules of the Road, KEJARA & Safety),
    // each with its own required count, taken from the exam type.
    var qSection = byId("mxQSection");
    var qSearch = byId("mxQSearch");
    var qGroups = byId("mxQGroups");
    var qMessage = byId("mxQMessage");
    var qBackBtn = byId("mxQBackBtn");
    var qCrumbLink = questionsView.querySelector(".breadcrumb a");
    var countEls = {};

    function requiredFor(sectionId) {
        var section = typeById(questionSet.typeId).sections.filter(function (x) { return x.id === sectionId; })[0];
        return section ? section.count : 0;
    }

    function selectedCount(sectionId) {
        return bankForSection(sectionId).filter(function (q) { return questionSelection[q.id]; }).length;
    }

    function setCount(el, selected, required) {
        el.textContent = "Selected: " + selected + " / " + required;
        el.className = "mx-qcount" + (selected === required ? " mx-qcount--ok" : (selected > required ? " mx-qcount--over" : ""));
    }

    // A full section cannot take more questions until one is unticked.
    function updateCounts() {
        var type = typeById(questionSet.typeId);

        type.sections.forEach(function (section) {
            if (countEls[section.id]) { setCount(countEls[section.id], selectedCount(section.id), section.count); }
        });

        Array.prototype.forEach.call(qGroups.querySelectorAll("input[type='checkbox']"), function (input) {
            var full = selectedCount(input._sectionId) >= requiredFor(input._sectionId);
            input.disabled = !input.checked && full;
            input.parentNode.classList.toggle("mx-qitem--disabled", input.disabled);
        });
    }

    function isDirty() {
        var saved = {};
        questionSet.selected.forEach(function (id) { saved[id] = true; });
        var current = Object.keys(questionSelection);

        return current.length !== questionSet.selected.length || current.some(function (id) { return !saved[id]; });
    }

    // Leaving with unsaved changes asks first, through the shared confirm dialog (it reads these attributes).
    function syncLeaveConfirm() {
        [qBackBtn, qCrumbLink].forEach(function (el) {
            if (isDirty()) {
                el.setAttribute("data-confirm", "You have unsaved changes to the question selection. Leave without saving?");
                el.setAttribute("data-confirm-title", "Discard changes?");
                el.setAttribute("data-confirm-label", "Discard");
            } else {
                el.removeAttribute("data-confirm");
                el.removeAttribute("data-confirm-title");
                el.removeAttribute("data-confirm-label");
            }
        });
    }

    function renderQuestionList(container, sectionId) {
        var term = qSearch.value.trim().toLowerCase();
        container.innerHTML = "";

        bankForSection(sectionId).filter(function (q) { return !term || q.text.toLowerCase().indexOf(term) !== -1; }).forEach(function (q) {
            var label = document.createElement("label");
            label.className = "mx-qitem";

            var input = document.createElement("input");
            input.type = "checkbox";
            input.checked = !!questionSelection[q.id];
            input._sectionId = sectionId;
            input.addEventListener("change", function () {
                if (input.checked) { questionSelection[q.id] = true; } else { delete questionSelection[q.id]; }
                qMessage.hidden = true;
                updateCounts();
                syncLeaveConfirm();
            });
            label.appendChild(input);

            var num = document.createElement("span");
            num.className = "mx-qitem__id";
            num.textContent = questionPrefix(q) + q.id.split("-")[1];
            label.appendChild(num);

            // Colour test images are small inline pictures; other images are only flagged.
            if (q.image && q.image.indexOf("data:") === 0) {
                var thumb = document.createElement("img");
                thumb.className = "mx-qitem__thumb";
                thumb.src = q.image;
                thumb.alt = "";
                label.appendChild(thumb);
            }

            var text = document.createElement("span");
            text.className = "mx-qitem__text";
            text.textContent = q.text;
            label.appendChild(text);

            if (q.image && q.image.indexOf("data:") !== 0) {
                var imageBadge = document.createElement("span");
                imageBadge.className = "badge";
                imageBadge.textContent = "Image";
                label.appendChild(imageBadge);
            }

            container.appendChild(label);
        });

        if (!container.children.length) {
            var none = document.createElement("p");
            none.className = "form-hint";
            none.textContent = "No questions match the search.";
            container.appendChild(none);
        }
    }

    function renderQuestionLists() {
        var type = typeById(questionSet.typeId);
        var only = qSection.value;

        qGroups.innerHTML = "";
        countEls = {};

        type.sections.forEach(function (section) {
            if (only && only !== section.id) { return; }

            var group = document.createElement("section");
            group.className = "mx-qgroup";

            var head = document.createElement("div");
            head.className = "mx-qgroup__head";

            var title = document.createElement("h3");
            title.textContent = section.label;
            head.appendChild(title);

            var tools = document.createElement("div");
            tools.className = "mx-qgroup__tools";

            var clear = document.createElement("button");
            clear.type = "button";
            clear.className = "btn btn-secondary btn-small";
            clear.textContent = "Clear";
            clear.setAttribute("aria-label", "Clear the selection in " + section.label);
            clear.addEventListener("click", function () {
                bankForSection(section.id).forEach(function (q) { delete questionSelection[q.id]; });
                qMessage.hidden = true;
                renderQuestionLists();
                syncLeaveConfirm();
            });
            tools.appendChild(clear);

            var count = document.createElement("span");
            count.className = "mx-qcount";
            tools.appendChild(count);
            countEls[section.id] = count;

            head.appendChild(tools);

            var list = document.createElement("div");
            list.className = "mx-qlist";

            group.appendChild(head);
            group.appendChild(list);
            qGroups.appendChild(group);

            renderQuestionList(list, section.id);
        });

        updateCounts();
    }

    function fillSectionFilter(type) {
        qSection.innerHTML = "";

        var all = document.createElement("option");
        all.value = "";
        all.textContent = "All Sections";
        qSection.appendChild(all);

        type.sections.forEach(function (section) {
            var option = document.createElement("option");
            option.value = section.id;
            option.textContent = section.label;
            qSection.appendChild(option);
        });
    }

    function openQuestions(set) {
        questionSet = set;
        questionSelection = {};
        set.selected.forEach(function (id) { questionSelection[id] = true; });

        var type = typeById(set.typeId);
        byId("mxQuestionsSubtitle").textContent = type.title + " — " + setLabel(set);
        fillSectionFilter(type);
        qSection.value = "";
        qSearch.value = "";
        qMessage.hidden = true;

        renderQuestionLists();
        syncLeaveConfirm();
        showView("questions");
        qSection.focus();
    }

    qSection.addEventListener("change", renderQuestionLists);
    qSearch.addEventListener("input", renderQuestionLists);

    byId("mxQSaveBtn").addEventListener("click", function () {
        var type = typeById(questionSet.typeId);
        var problems = [];

        type.sections.forEach(function (section) {
            var chosen = selectedCount(section.id);

            if (chosen !== section.count) {
                problems.push(section.label + ": " + chosen + " of " + section.count);
            }
        });

        if (problems.length) {
            qMessage.textContent = "Selection incomplete. Select exactly the required number in every section — " + problems.join("; ") + ".";
            qMessage.hidden = false;
            return;
        }

        // Prototype: kept in memory only so the screen stays consistent until refresh.
        questionSet.selected = Object.keys(questionSelection);
        renderTable();
        showView("list");
        addBtn.focus();

        showToast(toast, "Questions saved for " + setLabel(questionSet) + ". The selection is complete"
            + (questionSet.status === "Active" ? "." : ", so you can activate the set from the list.") + NOTE);
    });

    function leaveQuestions() {
        showView("list");
        addBtn.focus();
    }

    qBackBtn.addEventListener("click", leaveQuestions);

    // The breadcrumb behaves like Back (a normal link would reload the page and throw away every unsaved mock edit).
    qCrumbLink.addEventListener("click", function (event) {
        event.preventDefault();
        leaveQuestions();
    });

    // ---- wiring ----
    function clearFilters() {
        typeFilter.value = "";
        statusFilter.value = "";
        renderTable();
    }

    typeFilter.addEventListener("change", renderTable);
    statusFilter.addEventListener("change", renderTable);
    clearFiltersBtn.addEventListener("click", clearFilters);
    emptyClearBtn.addEventListener("click", clearFilters);
    addBtn.addEventListener("click", function () { openForm(null); });
    emptyAddBtn.addEventListener("click", function () { openForm(null); });

    renderTable();
})();
