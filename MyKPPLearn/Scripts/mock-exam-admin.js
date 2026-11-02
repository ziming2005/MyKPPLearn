/*
 * ============================================================================
 * Admin Mock Exam: Mock Exam Sets and their questions (Admin/MockExam.aspx).
 *
 * Data comes from the database through the page's page methods (Admin/MockExam.aspx.cs). The two exam types and their
 * section counts, time limits and pass marks come from window.MockExamData (Scripts/mock-exam.js, loaded first); the
 * server enforces the same rules (MockExamRules). Questions are chosen from the Question Bank: a section's questions
 * come from its category (the Colour Blind Test uses Colour Recognition). Only ACTIVE questions count towards a section.
 * ============================================================================
 */
(function () {
    "use strict";

    var root = document.getElementById("mxListView");

    if (!root || !window.MockExamData) { return; }

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
        el._hideTimer = window.setTimeout(function () { el.hidden = true; }, type === "error" ? 9000 : 6000);
        window.scrollTo(0, 0);
    }

    /** Calls a page method on this page and resolves with its return value ("d"). */
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

    /** After a failed call: if the page itself now redirects (to Login), the session has ended, so reload. */
    function checkSignedIn() {
        return fetch(location.pathname, { credentials: "same-origin", redirect: "manual" }).then(function (r) {
            if (r.type === "opaqueredirect" || r.status === 302) {
                location.reload();
                throw new Error("Your session has ended. Please sign in again.");
            }
        }, function () { /* network problem: keep the original error */ });
    }

    var types = MockExamData.getExamTypes();
    var bank = [];          // { id, sectionId, text, hasImage, status } from the Question Bank
    var bankById = {};
    var sets = [];          // { id, typeId, number, status, attemptCount, selected: [questionId] }

    function bankForSection(sectionId) {
        return bank.filter(function (q) { return q.sectionId === sectionId; });
    }

    function isActiveQuestion(id) {
        return !!bankById[id] && bankById[id].status === "Active";
    }

    function typeById(id) {
        return types.filter(function (t) { return t.id === id; })[0];
    }

    function typeIndex(id) {
        return types.indexOf(typeById(id));
    }

    // ---- completeness of a set's question selection (active questions only, as on the server) ----
    function sectionCounts(set) {
        var counts = {};
        set.selected.forEach(function (id) {
            if (isActiveQuestion(id)) {
                var sectionId = bankById[id].sectionId;
                counts[sectionId] = (counts[sectionId] || 0) + 1;
            }
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
        return set.selected.length === MockExamData.totalQuestions(typeById(set.typeId))
            && typeById(set.typeId).sections.every(function (section) { return (counts[section.id] || 0) === section.count; });
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

    var editingId = null;
    var lastAuto = null;           // the number suggested automatically (so typing your own is never overwritten)
    var questionSet = null;
    var questionSelection = null;  // Set-like object: id -> true
    var busy = false;

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
        window.scrollTo(0, 0);
    }

    /** Loads (or reloads) sets and the question bank from the database, then redraws the list. */
    function load() {
        return call("LoadData").then(function (data) {
            bank = data.bank || [];
            bankById = {};
            bank.forEach(function (q) { bankById[q.id] = q; });
            sets = data.sets || [];
            renderTable();
        }).catch(function (e) { showToast(toast, e.message, "error"); });
    }

    /** Puts a set returned by the server into the local list (after save, toggle or a new selection). */
    function storeSet(set) {
        if (!set) { return; }
        var existing = findSet(set.id);
        if (existing) { sets[sets.indexOf(existing)] = set; } else { sets.push(set); }
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
            setCell.textContent = String(set.number);
            row.appendChild(setCell);

            var typeCell = document.createElement("td");
            typeCell.textContent = type.title;
            row.appendChild(typeCell);

            // Active questions chosen for this set, against what the exam type requires.
            var total = MockExamData.totalQuestions(type);
            var chosen = selectedTotal(set);
            var questionsCell = document.createElement("td");
            questionsCell.className = "data-table__nowrap";
            var progress = document.createElement("span");
            progress.className = "mx-qcount" + (isComplete(set) ? " mx-qcount--ok" : "");
            progress.textContent = chosen + " / " + total;
            progress.title = isComplete(set) ? "All sections have the required number of active questions." : "Incomplete: open Questions to finish the selection.";
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

    /**
     * Delete uses the shared confirm dialog (confirm-modal.js): the first click only opens it; its confirm repeats the
     * click with data-confirmed="1", and only that click deletes.
     */
    function deleteButton(set) {
        var button = document.createElement("button");
        button.type = "button";
        button.className = "btn btn-secondary btn-small btn-danger";
        button.textContent = "Delete";
        button.setAttribute("data-confirm", "Delete " + setLabel(set) + " (" + typeById(set.typeId).title + ")?"
            + (set.status === "Active" ? " It is Active, so learners will no longer be able to start it." : ""));
        button.setAttribute("data-confirm-title", "Delete Mock Exam Set?");
        button.addEventListener("click", function () {
            if (button.getAttribute("data-confirmed") !== "1") { return; }

            var wasActive = set.status === "Active";

            call("Delete", { id: set.id }).then(function (message) {
                if (message !== "Deleted") { throw new Error(message); }

                sets = sets.filter(function (s) { return s.id !== set.id; });
                renderTable();

                if (wasActive && noActiveSetsLeft(set.typeId)) {
                    showToast(toast, setLabel(set) + " deleted. " + typeById(set.typeId).title + " has no active sets now, so learners can't start this exam.", "warning");
                } else {
                    showToast(toast, "Mock exam set deleted.");
                }
            }).catch(function (e) { showToast(toast, e.message, "error"); });
        });
        return button;
    }

    function noActiveSetsLeft(typeId) {
        return !sets.some(function (s) { return s.typeId === typeId && s.status === "Active"; });
    }

    function toggleStatus(set) {
        var label = setLabel(set);
        var activate = set.status !== "Active";

        // An incomplete set would give learners an exam with missing questions (the server refuses it too).
        if (activate && !isComplete(set)) {
            showToast(toast, label + " can't be activated until every section has the required number of active questions ("
                + selectedTotal(set) + " / " + MockExamData.totalQuestions(typeById(set.typeId)) + " selected). Use Questions to finish the selection.", "error");
            return;
        }

        call("Toggle", { id: set.id, active: activate }).then(function (result) {
            if (result.error) { throw new Error(result.error); }

            storeSet(result.set);
            renderTable();

            if (!activate && noActiveSetsLeft(set.typeId)) {
                showToast(toast, label + " deactivated. " + typeById(set.typeId).title + " has no active sets now, so learners can't start this exam.", "warning");
            } else {
                showToast(toast, label + (activate ? " activated." : " deactivated."));
            }
        }).catch(function (e) { showToast(toast, e.message, "error"); });
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

    function updateTypeChangeHint() {
        var original = editingId ? findSet(editingId) : null;
        typeChangeHint.hidden = !(original && original.typeId !== formType.value);
    }

    function clearErrors() {
        typeError.hidden = true;
        numberError.hidden = true;
        formType.removeAttribute("aria-invalid");
        formNumber.removeAttribute("aria-invalid");
    }

    function fail(errorEl, message, field) {
        errorEl.textContent = message;
        errorEl.hidden = false;
        field.setAttribute("aria-invalid", "true");
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
    });

    formNumber.addEventListener("input", function () {
        numberError.hidden = true;
        formNumber.removeAttribute("aria-invalid");
    });

    // Enter saves the form instead of posting the whole ASP.NET page back.
    [formType, formNumber].forEach(function (field) {
        field.addEventListener("keydown", function (event) {
            if (event.key === "Enter") {
                event.preventDefault();
                saveBtn.click();
            }
        });
    });

    saveBtn.addEventListener("click", function () {
        if (busy) { return; }
        clearErrors();

        var raw = formNumber.value.trim();

        if (!formType.value) {
            fail(typeError, "Please select an exam type.", formType);
            return;
        }

        if (!/^[1-9][0-9]{0,8}$/.test(raw)) {
            fail(numberError, raw ? "Mock Exam Set number must be a whole number greater than 0 (for example 1, 2, 3)." : "Mock Exam Set number is required.", formNumber);
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

        busy = true;
        saveBtn.disabled = true;

        call("Save", { id: editingId || 0, typeId: formType.value, number: number }).then(function (result) {
            if (result.error) {
                if (result.field === "type") { fail(typeError, result.error, formType); }
                else if (result.field === "number") { fail(numberError, result.error, formNumber); }
                else { showToast(toast, result.error, "error"); }
                return;
            }

            storeSet(result.set);
            renderTable();
            backToList();
            showToast(toast, result.notice || "Mock exam set updated.", result.notice && editingId ? "warning" : "success");
        }).catch(function (e) {
            showToast(toast, e.message, "error");
        }).then(function () {
            busy = false;
            saveBtn.disabled = false;
        });
    });

    cancelBtn.addEventListener("click", backToList);

    // The breadcrumb behaves like Cancel (no page reload).
    formView.querySelector(".breadcrumb a").addEventListener("click", function (event) {
        event.preventDefault();
        backToList();
    });

    // ---- manage questions ----
    // One group per exam section (Colour Blind Test, Road Signs, Rules of the Road, KEJARA & Safety),
    // each with its own required count, taken from the exam type.
    var qSection = byId("mxQSection");
    var qSearch = byId("mxQSearch");
    var qGroups = byId("mxQGroups");
    var qMessage = byId("mxQMessage");
    var qBackBtn = byId("mxQBackBtn");
    var qSaveBtn = byId("mxQSaveBtn");
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

    // A full section cannot take more questions until one is unticked. Inactive questions can only be unticked.
    function updateCounts() {
        var type = typeById(questionSet.typeId);

        type.sections.forEach(function (section) {
            if (countEls[section.id]) { setCount(countEls[section.id], selectedCount(section.id), section.count); }
        });

        Array.prototype.forEach.call(qGroups.querySelectorAll("input[type='checkbox']"), function (input) {
            var full = selectedCount(input._sectionId) >= requiredFor(input._sectionId);
            input.disabled = !input.checked && (full || input._inactive);
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

        // Inactive questions are listed only when already chosen, so they can be unticked.
        bankForSection(sectionId)
            .filter(function (q) { return q.status === "Active" || questionSelection[q.id]; })
            .filter(function (q) { return !term || q.text.toLowerCase().indexOf(term) !== -1; })
            .forEach(function (q) {
                var label = document.createElement("label");
                label.className = "mx-qitem";

                var input = document.createElement("input");
                input.type = "checkbox";
                input.checked = !!questionSelection[q.id];
                input._sectionId = sectionId;
                input._inactive = q.status !== "Active";
                input.addEventListener("change", function () {
                    if (input.checked) { questionSelection[q.id] = true; } else { delete questionSelection[q.id]; }
                    qMessage.hidden = true;
                    updateCounts();
                    syncLeaveConfirm();
                });
                label.appendChild(input);

                var num = document.createElement("span");
                num.className = "mx-qitem__id";
                num.textContent = "Q" + q.id;
                label.appendChild(num);

                var text = document.createElement("span");
                text.className = "mx-qitem__text";
                text.textContent = q.text;
                label.appendChild(text);

                if (q.hasImage) {
                    var imageBadge = document.createElement("span");
                    imageBadge.className = "badge";
                    imageBadge.textContent = "Image";
                    label.appendChild(imageBadge);
                }

                if (q.status !== "Active") {
                    var inactiveBadge = document.createElement("span");
                    inactiveBadge.className = "badge";
                    inactiveBadge.textContent = "Inactive";
                    label.appendChild(inactiveBadge);
                }

                container.appendChild(label);
            });

        if (!container.children.length) {
            var none = document.createElement("p");
            none.className = "form-hint";
            none.textContent = bankForSection(sectionId).length ? "No questions match the search." : "No questions are available in this section yet. Add them in the Question Bank.";
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
    qSearch.addEventListener("keydown", function (event) { if (event.key === "Enter") { event.preventDefault(); } });

    qSaveBtn.addEventListener("click", function () {
        if (busy) { return; }

        var type = typeById(questionSet.typeId);
        var problems = [];

        type.sections.forEach(function (section) {
            var chosen = selectedCount(section.id);

            if (chosen !== section.count) {
                problems.push(section.label + ": " + chosen + " of " + section.count);
            }
        });

        var inactive = Object.keys(questionSelection).filter(function (id) { return !isActiveQuestion(id); }).length;

        if (problems.length || inactive) {
            qMessage.textContent = problems.length
                ? "Selection incomplete. Select exactly the required number in every section: " + problems.join("; ") + "."
                : "Untick the inactive questions (or activate them in the Question Bank) before saving.";
            qMessage.hidden = false;
            return;
        }

        // In section order, so the exam paper runs Colour Blind Test, Road Signs, Rules of the Road, KEJARA & Safety.
        var ordered = [];
        type.sections.forEach(function (section) {
            bankForSection(section.id).forEach(function (q) { if (questionSelection[q.id]) { ordered.push(q.id); } });
        });

        busy = true;
        qSaveBtn.disabled = true;

        call("SaveQuestions", { id: questionSet.id, questionIds: ordered }).then(function (result) {
            if (result.error) {
                qMessage.textContent = result.error;
                qMessage.hidden = false;
                return;
            }

            storeSet(result.set);
            renderTable();
            showView("list");
            addBtn.focus();

            showToast(toast, "Questions saved for " + setLabel(result.set) + ". The selection is complete"
                + (result.set.status === "Active" ? "." : ", so you can activate the set from the list."));
        }).catch(function (e) {
            qMessage.textContent = e.message;
            qMessage.hidden = false;
        }).then(function () {
            busy = false;
            qSaveBtn.disabled = false;
        });
    });

    function leaveQuestions() {
        showView("list");
        addBtn.focus();
    }

    /**
     * With unsaved changes the shared confirm dialog asks first: the first click only opens it, and its confirm
     * repeats the click with data-confirmed="1". Without changes there is no data-confirm and the click leaves at once.
     */
    function confirmedLeave(el) {
        return !el.hasAttribute("data-confirm") || el.getAttribute("data-confirmed") === "1";
    }

    qBackBtn.addEventListener("click", function () {
        if (confirmedLeave(qBackBtn)) { leaveQuestions(); }
    });

    // The breadcrumb behaves like Back (no page reload).
    qCrumbLink.addEventListener("click", function (event) {
        event.preventDefault();
        if (confirmedLeave(qCrumbLink)) { leaveQuestions(); }
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

    load();
})();
