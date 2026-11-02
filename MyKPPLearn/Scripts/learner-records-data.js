/*
 * Admin Learner Records - data module. Talks to the page methods of the current page
 * (Admin/LearnerRecords.aspx and Admin/LearnerRecordDetails.aspx code-behind) with JSON over POST,
 * and holds the small display helpers both pages share.
 *
 * List record:   { id, name, email, joined: "YYYY-MM-DD", status: "Active" | "Disabled" }
 * Detail record: the list fields plus
 *     chapters: [{ number, title, total, completed }],
 *     practice: [{ category, setNumber, total, correct, date }],
 *     mock:     [{ examType, setNumber, colourBlind, colourBlindTotal, theory, theoryTotal, passed, date }],
 *     games:    [{ setNumber, total, correct, date }]
 */
(function (global) {
    "use strict";

    var PAGE_SIZE = 10;

    var MONTHS = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

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
     * itself: if it now redirects (to Login), the session has ended and the page is reloaded so the admin can sign in.
     */
    function checkSignedIn() {
        return fetch(location.pathname, { credentials: "same-origin", redirect: "manual" }).then(function (r) {
            if (r.type === "opaqueredirect" || r.status === 302) {
                location.reload();
                throw new Error("Your session has ended. Please sign in again.");
            }
        }, function () { /* network problem: keep the original error */ });
    }

    function formatDate(isoDate, short) {
        var parts = String(isoDate).split("-");
        var month = MONTHS[parseInt(parts[1], 10) - 1] || "";
        return parseInt(parts[2], 10) + " " + (short ? month.slice(0, 3) : month) + " " + parts[0];
    }

    function initials(name) {
        var parts = String(name).trim().split(/\s+/).filter(Boolean);
        if (!parts.length) { return "?"; }
        if (parts.length === 1) { return parts[0].charAt(0).toUpperCase(); }
        return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
    }

    function percent(done, total) { return total > 0 ? Math.round(done * 100 / total) : 0; }

    function lessonsCompleted(learner) { return (learner.chapters || []).reduce(function (sum, c) { return sum + c.completed; }, 0); }
    function lessonsTotal(learner) { return (learner.chapters || []).reduce(function (sum, c) { return sum + c.total; }, 0); }

    global.LearnerRecordsData = {
        pageSize: PAGE_SIZE,

        // ---- server calls (each resolves with the page method's result) ----
        loadLearners: function () { return call("LoadLearners"); },
        create: function (name, email, password) { return call("Create", { name: name, email: email, password: password }); },
        update: function (id, name, email) { return call("Update", { id: id, name: name, email: email }); },
        setStatus: function (id, active) { return call("SetStatus", { id: id, active: active }); },
        remove: function (id) { return call("Delete", { id: id }); },
        getDetails: function (id) { return call("GetDetails", { id: id }); },

        // ---- display helpers ----
        lessonsCompleted: lessonsCompleted,
        lessonsTotal: lessonsTotal,
        formatDate: formatDate,
        initials: initials,
        percent: percent
    };
})(window);
