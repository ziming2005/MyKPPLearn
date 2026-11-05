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
        el._hideTimer = window.setTimeout(function () { el.hidden = true; }, 5000);
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
    var bank = [];          // { id, sectionId, text, image, status } from the Question Bank
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

    var formHeading = byId("mxFormHeading");
    var formType = byId("mxFormType");
    var typeHint = byId("mxTypeHint");
    var typeError = byId("mxTypeError");
    var numberInfo = byId("mxNumberInfo");
    var formRules = byId("mxFormRules");
    var saveBtn = byId("mxSaveBtn");
    var cancelBtn = byId("mxCancelBtn");
    var crumbLink = formView.querySelector(".breadcrumb a");

    var editingSet = null;         // the set being edited, or null when adding
    var questionSelection = {};    // Set-like object: questionId -> true
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

    /** Puts a set returned by the server into the local list (after save or toggle). */
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

    // Each label and value pair sits in its own <div> so the rules can be laid out as a grid.
    function appendDetail(dl, label, value) {
        var pair = document.createElement("div");
        var dt = document.createElement("dt");
        dt.textContent = label;
        var dd = document.createElement("dd");
        dd.textContent = value;
        pair.appendChild(dt);
        pair.appendChild(dd);
        dl.appendChild(pair);
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
            progress.title = isComplete(set) ? "All sections have the required number of active questions." : "Incomplete: Edit the set to finish the selection.";
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
            actions.appendChild(actionButton("Edit", function () { openEditor(set); }));
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

        if (set.learnerCount > 0) {
            // Learners' results keep the set: explain that instead of asking to confirm a delete the server refuses.
            button.addEventListener("click", function () {
                showToast(toast, set.learnerCount + (set.learnerCount === 1 ? " learner has" : " learners have")
                    + " taken this set, so it can't be deleted. Deactivate it instead to hide it from learners.", "error");
            });
            return button;
        }

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
                + selectedTotal(set) + " / " + MockExamData.totalQuestions(typeById(set.typeId)) + " selected). Edit the set to finish the selection.", "error");
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

    // ------------------------------------------------------------------
    // Add / Edit Mock Exam Set: the exam type and the questions, saved together.
    // One group per exam section (Colour Blind Test, Road Signs, Rules of the Road, KEJARA & Safety), each with its
    // own required count from the exam type. Section ids are the same in both types, so a type change keeps the ticks.
    // ------------------------------------------------------------------
    var qSection = byId("mxQSection");
    var qSearch = byId("mxQSearch");
    var qGroups = byId("mxQGroups");
    var qMessage = byId("mxQMessage");
    var countEls = {};

    function currentType() {
        return typeById(formType.value);
    }

    /** The lowest number not used by another set of the exam type (what the server gives a new or moved set). */
    function nextFreeNumber(typeId) {
        var used = sets
            .filter(function (s) { return s.typeId === typeId && (!editingSet || s.id !== editingSet.id); })
            .map(function (s) { return s.number; });
        var next = 1;
        while (used.indexOf(next) >= 0) { next++; }
        return next;
    }

    function renderNumberInfo() {
        var type = currentType();

        if (!editingSet || editingSet.typeId === type.id) {
            numberInfo.textContent = "";
            numberInfo.hidden = true;
        } else {
            numberInfo.hidden = false;
            var over = type.sections.some(function (section) { return selectedCount(section.id) > section.count; });
            numberInfo.textContent = "Saving moves this set to " + type.title + " as " + MockExamData.setLabel(nextFreeNumber(type.id))
                + "." + (over ? " Untick questions in the sections that now have too many." : "");
        }
    }

    function renderFormRules() {
        formRules.innerHTML = "";
        appendRules(formRules, currentType());
    }

    function requiredFor(sectionId) {
        var section = currentType().sections.filter(function (x) { return x.id === sectionId; })[0];
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
        currentType().sections.forEach(function (section) {
            if (countEls[section.id]) { setCount(countEls[section.id], selectedCount(section.id), section.count); }
        });

        Array.prototype.forEach.call(qGroups.querySelectorAll("input[type='checkbox']"), function (input) {
            var full = selectedCount(input._sectionId) >= requiredFor(input._sectionId);
            input.disabled = !input.checked && (full || input._inactive);
            input.parentNode.classList.toggle("mx-qitem--disabled", input.disabled);
        });

        renderNumberInfo();   // its "too many" note follows the ticks
    }

    function isDirty() {
        var saved = {};
        var savedIds = editingSet ? editingSet.selected : [];
        savedIds.forEach(function (id) { saved[id] = true; });
        var current = Object.keys(questionSelection);

        return (editingSet ? editingSet.typeId !== formType.value : current.length > 0)
            || current.length !== savedIds.length
            || current.some(function (id) { return !saved[id]; });
    }

    // Leaving with unsaved changes asks first, through the shared confirm dialog (it reads these attributes).
    function syncLeaveConfirm() {
        [cancelBtn, crumbLink].forEach(function (el) {
            if (isDirty()) {
                el.setAttribute("data-confirm", "You have unsaved changes to this Mock Exam Set. Leave without saving?");
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

                if (q.image) {
                    var imageBadge = document.createElement("button");
                    imageBadge.type = "button";
                    imageBadge.className = "badge mx-qitem__image";
                    imageBadge.textContent = "Image";
                    imageBadge.setAttribute("aria-label", "View the image of Q" + q.id);
                    imageBadge.addEventListener("click", function (event) {
                        event.preventDefault();   // inside the label: do not tick the question
                        openImage(q);
                    });
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
        var only = qSection.value;

        qGroups.innerHTML = "";
        countEls = {};

        currentType().sections.forEach(function (section) {
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

    function fillSectionFilter() {
        var selected = qSection.value;
        qSection.innerHTML = "";

        var all = document.createElement("option");
        all.value = "";
        all.textContent = "All Sections";
        qSection.appendChild(all);

        currentType().sections.forEach(function (section) {
            var option = document.createElement("option");
            option.value = section.id;
            option.textContent = section.label;
            qSection.appendChild(option);
        });

        qSection.value = selected;
        if (qSection.value !== selected) { qSection.value = ""; }
    }

    /** Shows the exam type, number, rules and question lists for the current exam type. */
    function renderEditor() {
        renderNumberInfo();
        renderFormRules();
        fillSectionFilter();
        renderQuestionLists();
        syncLeaveConfirm();
    }

    function openEditor(set) {
        editingSet = set || null;
        questionSelection = {};
        toast.hidden = true;
        if (set) { set.selected.forEach(function (id) { questionSelection[id] = true; }); }

        var heading = set ? "Edit Mock Exam Set" : "Add Mock Exam Set";
        formHeading.textContent = heading;
        setCrumb("mxFormCrumb", heading);

        // The type is fixed once learners have taken the set (their results would change meaning).
        var locked = !!(set && set.attemptCount > 0);
        formType.value = set ? set.typeId : (typeFilter.value || types[0].id);
        formType.disabled = locked;
        typeHint.hidden = !locked;
        typeHint.textContent = locked ? "The exam type can't be changed because learners have already taken this set." : "";
        typeError.hidden = true;

        qSection.value = "";
        qSearch.value = "";
        qMessage.hidden = true;

        renderEditor();
        showView("form");
        formType.focus();
    }

    function backToList() {
        showView("list");
        addBtn.focus();
    }

    formType.addEventListener("change", function () {
        typeError.hidden = true;
        qMessage.hidden = true;
        renderEditor();
    });

    qSection.addEventListener("change", renderQuestionLists);
    qSearch.addEventListener("input", renderQuestionLists);
    [formType, qSection, qSearch].forEach(function (field) {
        field.addEventListener("keydown", function (event) { if (event.key === "Enter") { event.preventDefault(); } });
    });

    saveBtn.addEventListener("click", function () {
        if (busy) { return; }

        var type = currentType();
        var over = type.sections.filter(function (section) { return selectedCount(section.id) > section.count; });
        var inactive = Object.keys(questionSelection).filter(function (id) { return !isActiveQuestion(id); }).length;

        if (over.length || inactive) {
            qMessage.textContent = over.length
                ? "Too many questions selected for " + type.title + ": " + over.map(function (section) {
                    return section.label + " " + selectedCount(section.id) + " of " + section.count;
                }).join("; ") + ". Untick some before saving."
                : "Untick the inactive questions (or activate them in the Question Bank) before saving.";
            qMessage.hidden = false;
            qMessage.scrollIntoView({ block: "center" });
            return;
        }

        // In section order, so the exam paper runs Colour Blind Test, Road Signs, Rules of the Road, KEJARA & Safety.
        var ordered = [];
        type.sections.forEach(function (section) {
            bankForSection(section.id).forEach(function (q) { if (questionSelection[q.id]) { ordered.push(q.id); } });
        });

        busy = true;
        saveBtn.disabled = true;

        call("Save", { id: editingSet ? editingSet.id : 0, typeId: type.id, questionIds: ordered }).then(function (result) {
            if (result.error) {
                if (result.field === "type") {
                    typeError.textContent = result.error;
                    typeError.hidden = false;
                    formType.focus();
                } else {
                    qMessage.textContent = result.error;
                    qMessage.hidden = false;
                    qMessage.scrollIntoView({ block: "center" });
                }
                return;
            }

            storeSet(result.set);
            renderTable();
            backToList();
            showToast(toast, result.notice, result.set.status === "Active" ? "success" : "warning");
        }).catch(function (e) {
            qMessage.textContent = e.message;
            qMessage.hidden = false;
        }).then(function () {
            busy = false;
            saveBtn.disabled = false;
        });
    });

    /**
     * With unsaved changes the shared confirm dialog asks first: the first click only opens it, and its confirm
     * repeats the click with data-confirmed="1". Without changes there is no data-confirm and the click leaves at once.
     */
    function confirmedLeave(el) {
        return !el.hasAttribute("data-confirm") || el.getAttribute("data-confirmed") === "1";
    }

    cancelBtn.addEventListener("click", function () {
        if (confirmedLeave(cancelBtn)) { backToList(); }
    });

    // The breadcrumb behaves like Cancel (no page reload).
    crumbLink.addEventListener("click", function (event) {
        event.preventDefault();
        if (confirmedLeave(crumbLink)) { backToList(); }
    });

    // ------------------------------------------------------------------
    // Image viewer for the Image badges.
    // ------------------------------------------------------------------
    var imageOverlay = byId("mxImageOverlay");
    var imagePicture = byId("mxImagePicture");
    var imageCloseBtn = byId("mxImageCloseBtn");
    var imageOpener = null;

    function openImage(q) {
        imageOpener = document.activeElement;
        byId("mxImageTitle").textContent = "Q" + q.id + ": " + q.text;
        imagePicture.src = q.image;
        imagePicture.alt = "Question image";
        imageOverlay.hidden = false;
        imageCloseBtn.focus();
    }

    function closeImage() {
        if (imageOverlay.hidden) { return; }
        imageOverlay.hidden = true;
        imagePicture.removeAttribute("src");
        if (imageOpener && imageOpener.focus) { imageOpener.focus(); }
    }

    imageCloseBtn.addEventListener("click", closeImage);
    imageOverlay.addEventListener("click", function (event) { if (event.target === imageOverlay) { closeImage(); } });
    document.addEventListener("keydown", function (event) { if (event.key === "Escape") { closeImage(); } });

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
    addBtn.addEventListener("click", function () { openEditor(null); });
    emptyAddBtn.addEventListener("click", function () { openEditor(null); });

    load();
})();
