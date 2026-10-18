/*
 * Admin Learner Records - frontend data module (UI prototype). Nothing here reads the database.
 *
 * No learner records are preloaded: the collection below starts EMPTY, so the list shows its "No learners found." state and
 * the detail page sends the browser back to the list for any id. A later phase replaces the functions in LearnerRecordsData
 * with calls to a repository; the list and detail pages (learner-records-admin.js) only talk to this object.
 *
 * Record shape the pages expect (so a future data source can supply the same fields):
 *   { id, name, email, joined: "YYYY-MM-DD", status: "Active" | "Disabled",
 *     chapters: [{ number, title, total, completed }],
 *     practice: [{ category, setNumber, total, correct, date }],
 *     mock:     [{ examType, setNumber, colourBlind, colourBlindTotal, theory, theoryTotal, passed, date }],
 *     games:    [{ setNumber, total, correct, date }] }
 *
 * Edit / Disable / Enable / Delete only change the in-memory copy (nothing is saved anywhere).
 */
(function (global) {
    "use strict";

    var PAGE_SIZE = 10;

    var learners = [];

    var MONTHS = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

    // The list shows the most recently joined learners first.
    function ordered() {
        return learners.slice().sort(function (a, b) { return a.joined < b.joined ? 1 : (a.joined > b.joined ? -1 : 0); });
    }

    function lessonsCompleted(learner) { return (learner.chapters || []).reduce(function (sum, c) { return sum + c.completed; }, 0); }
    function lessonsTotal(learner) { return (learner.chapters || []).reduce(function (sum, c) { return sum + c.total; }, 0); }

    function formatDate(isoDate, short) {
        var parts = String(isoDate).split("-");
        var month = MONTHS[parseInt(parts[1], 10) - 1] || "";
        return parts[2] + " " + (short ? month.slice(0, 3) : month) + " " + parts[0];
    }

    function initials(name) {
        var parts = String(name).trim().split(/\s+/).filter(Boolean);
        if (!parts.length) { return "?"; }
        if (parts.length === 1) { return parts[0].charAt(0).toUpperCase(); }
        return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
    }

    function percent(done, total) { return total > 0 ? Math.round(done * 100 / total) : 0; }

    global.LearnerRecordsData = {
        pageSize: PAGE_SIZE,
        getAll: function () { return ordered(); },
        getById: function (id) {
            for (var i = 0; i < learners.length; i++) {
                if (learners[i].id === String(id)) { return learners[i]; }
            }
            return null;
        },
        // ---- prototype actions: they change this in-memory copy only ----
        updateLearner: function (id, name, email) {
            var learner = this.getById(id);
            if (!learner) { return false; }
            learner.name = name;
            learner.email = email;
            return true;
        },
        setStatus: function (id, status) {
            var learner = this.getById(id);
            if (!learner) { return false; }
            learner.status = status === "Disabled" ? "Disabled" : "Active";
            return true;
        },
        remove: function (id) {
            for (var i = 0; i < learners.length; i++) {
                if (learners[i].id === String(id)) { learners.splice(i, 1); return true; }
            }
            return false;
        },
        lessonsCompleted: lessonsCompleted,
        lessonsTotal: lessonsTotal,
        formatDate: formatDate,
        initials: initials,
        percent: percent
    };
})(window);
