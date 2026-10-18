/*
 * Admin Learner Records - MOCK DATA ONLY (UI prototype). Nothing here reads the database: the learners, their lesson progress,
 * Practice / Mock Exam / Hidden Road Sign Game history are all generated locally from fixed seed values, so the list page and the
 * detail page always show exactly the same people and numbers (the list columns are COUNTED from each learner's detail records).
 *
 * To make this real later, replace the LearnerRecordsData functions with calls to a repository.
 * Edit / Disable / Enable / Delete only change the in-memory copy (nothing is saved anywhere).
 */
(function (global) {
    "use strict";

    var PAGE_SIZE = 10;
    var TODAY = Date.UTC(2026, 9, 10);          // 10 October 2026 (the most recent date any record can have)
    var DAY = 24 * 60 * 60 * 1000;

    var CHAPTERS = [
        { number: 1, title: "Driving Fundamentals", lessons: 3 },
        { number: 2, title: "Road Rules", lessons: 4 },
        { number: 3, title: "Safe Driving", lessons: 5 }
    ];

    var PRACTICE_CATEGORIES = ["Colour Recognition", "Road Signs", "Rules of the Road", "KEJARA & Safety"];

    var EXAM_TYPES = [
        { name: "KPP Standard", colourBlindTotal: 8, theoryTotal: 50, colourBlindPass: 8, theoryPass: 42 },
        { name: "KPP Combined", colourBlindTotal: 8, theoryTotal: 70, colourBlindPass: 8, theoryPass: 56 }
    ];

    var NAMES = [
        "Ong Zi Ming", "Alicia Tan", "Daniel Lee", "Nur Aisyah", "Jason Lim", "Siti Hajar", "Marcus Wong", "Emily Chen",
        "Amir Hakim", "Rachel Goh", "Kevin Tan", "Farah Nabila", "Benjamin Ong", "Priya Nair", "Hafiz Rahman", "Chloe Lim",
        "Vincent Ng", "Aina Sofea", "Darren Koh", "Mei Ling Chong", "Arjun Menon", "Nadia Karim", "Ethan Yap", "Sharifah Zainab"
    ];

    // Some learners deliberately have nothing in one area so the empty states can be seen.
    var NO_PRACTICE = { 5: true, 21: true };
    var NO_MOCK = { 8: true, 11: true, 21: true };
    var NO_GAMES = { 3: true, 17: true, 21: true };
    var NO_LESSONS = { 21: true };

    // A few learners are Disabled so that state can be seen (MOCK ONLY: there is no status field in the database).
    var DISABLED = { 4: true, 9: true, 15: true, 22: true };

    // Small seeded generator: the same numbers on every load and on both pages.
    function rng(seed) {
        var state = seed % 2147483647;
        if (state <= 0) { state += 2147483646; }
        return function () {
            state = (state * 16807) % 2147483647;
            return (state - 1) / 2147483646;
        };
    }

    function pick(rand, list) { return list[Math.floor(rand() * list.length)]; }
    function between(rand, min, max) { return min + Math.floor(rand() * (max - min + 1)); }

    function iso(ms) { return new Date(ms).toISOString().slice(0, 10); }

    // newest first
    function byDateDesc(a, b) { return a.date < b.date ? 1 : (a.date > b.date ? -1 : 0); }

    function dateBetween(rand, fromMs) {
        var span = Math.max(0, Math.floor((TODAY - fromMs) / DAY));
        return iso(fromMs + between(rand, 0, span) * DAY);
    }

    function buildLearner(index) {
        var id = index + 1;
        var rand = rng(1000 + id * 97);
        var name = NAMES[index];
        var joinedMs = TODAY - between(rand, 3, 200) * DAY;

        // Lesson progress per chapter (never more than the chapter has).
        var chapters = CHAPTERS.map(function (chapter) {
            return {
                number: chapter.number,
                title: chapter.title,
                total: chapter.lessons,
                completed: NO_LESSONS[id] ? 0 : between(rand, 0, chapter.lessons)
            };
        });

        var practice = [];
        if (!NO_PRACTICE[id]) {
            for (var p = between(rand, 1, 9); p > 0; p--) {
                practice.push({
                    category: pick(rand, PRACTICE_CATEGORIES),
                    setNumber: between(rand, 1, 3),
                    total: 10,
                    correct: between(rand, 4, 10),
                    date: dateBetween(rand, joinedMs)
                });
            }
        }

        var mock = [];
        if (!NO_MOCK[id]) {
            for (var m = between(rand, 1, 5); m > 0; m--) {
                var type = pick(rand, EXAM_TYPES);
                var colourBlind = between(rand, 6, type.colourBlindTotal);
                var theory = between(rand, Math.round(type.theoryTotal * 0.6), type.theoryTotal);

                mock.push({
                    examType: type.name,
                    setNumber: between(rand, 1, 3),
                    colourBlind: colourBlind,
                    colourBlindTotal: type.colourBlindTotal,
                    theory: theory,
                    theoryTotal: type.theoryTotal,
                    // PASS / FAIL follows the exam's own pass marks, so it can never contradict the scores shown.
                    passed: colourBlind >= type.colourBlindPass && theory >= type.theoryPass,
                    date: dateBetween(rand, joinedMs)
                });
            }
        }

        var games = [];
        if (!NO_GAMES[id]) {
            for (var g = between(rand, 1, 7); g > 0; g--) {
                var total = between(rand, 2, 4);
                games.push({ setNumber: between(rand, 1, 4), total: total, correct: between(rand, 0, total), date: dateBetween(rand, joinedMs) });
            }
        }

        practice.sort(byDateDesc);
        mock.sort(byDateDesc);
        games.sort(byDateDesc);

        var parts = name.toLowerCase().split(" ");

        return {
            id: String(id),
            name: name,
            email: parts.join(".") + "@example.com",
            joined: iso(joinedMs),
            status: DISABLED[id] ? "Disabled" : "Active",
            chapters: chapters,
            practice: practice,
            mock: mock,
            games: games
        };
    }

    var learners = NAMES.map(function (n, i) { return buildLearner(i); });

    function lessonsCompleted(learner) { return learner.chapters.reduce(function (sum, c) { return sum + c.completed; }, 0); }
    function lessonsTotal(learner) { return learner.chapters.reduce(function (sum, c) { return sum + c.total; }, 0); }

    // The list shows the most recently joined learners first.
    var ordered = learners.slice().sort(function (a, b) { return a.joined < b.joined ? 1 : (a.joined > b.joined ? -1 : 0); });

    var MONTHS = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

    function formatDate(isoDate, short) {
        var parts = isoDate.split("-");
        var month = MONTHS[parseInt(parts[1], 10) - 1];
        return parts[2] + " " + (short ? month.slice(0, 3) : month) + " " + parts[0];
    }

    function initials(name) {
        var parts = name.trim().split(/\s+/).filter(Boolean);
        if (!parts.length) { return "?"; }
        if (parts.length === 1) { return parts[0].charAt(0).toUpperCase(); }
        return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
    }

    function percent(done, total) { return total > 0 ? Math.round(done * 100 / total) : 0; }

    global.LearnerRecordsData = {
        pageSize: PAGE_SIZE,
        getAll: function () { return ordered.slice(); },
        getById: function (id) {
            for (var i = 0; i < learners.length; i++) {
                if (learners[i].id === String(id)) { return learners[i]; }
            }
            return null;
        },
        // ---- prototype actions: they change this in-memory copy only (it is rebuilt from the seed values on every page load) ----
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
            var found = false;
            [learners, ordered].forEach(function (list) {
                for (var i = 0; i < list.length; i++) {
                    if (list[i].id === String(id)) { list.splice(i, 1); found = true; break; }
                }
            });
            return found;
        },
        lessonsCompleted: lessonsCompleted,
        lessonsTotal: lessonsTotal,
        formatDate: formatDate,
        initials: initials,
        percent: percent
    };
})(window);
