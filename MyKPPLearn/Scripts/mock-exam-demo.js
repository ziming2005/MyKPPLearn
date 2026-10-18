/*
 * ============================================================================
 * Phase 6 frontend-only Mock Examination data and Learner UI logic.
 *
 * // Phase 6 frontend-only Mock Examination data.
 * // Replace with database-backed Mock Exam data in a later phase.
 * // Demo content is not official JPJ/KPP examination content.
 *
 * Nothing here is persisted: there are no Mock Exam tables, repositories or
 * attempt records. All exam state (answers, flags, timer, result) lives in
 * memory in this script and is discarded on refresh.
 *
 * window.MockExamData exposes the exam-type rules and the generated demo
 * question bank to the Learner attempt UI (this file) and to the Admin
 * prototype (mock-exam-admin-demo.js).
 * ============================================================================
 */
(function (global) {
    "use strict";

    var DEMO_IMAGE = "Content/images/illustrations/driving-theory.svg";
    var DEMO_IMAGE_ALT = "Illustrative driving theory graphic (demo placeholder, not an official road sign image).";

    // ------------------------------------------------------------------
    // Exam types: exactly two. Car Only and Motorcycle Only share the
    // "KPP Standard" structure, so there is no Car/Motorcycle split here.
    // ------------------------------------------------------------------
    var EXAM_TYPES = [
        {
            id: "combined",
            title: "KPP Combined (Car & Motorcycle)",
            name: "KPP Combined",
            description: "Car & Motorcycle",
            subtitle: "",
            sections: [
                { id: "colourBlind", label: "Colour Blind Test", count: 8 },
                { id: "roadSigns", label: "Road Signs", count: 21 },
                { id: "rulesOfTheRoad", label: "Rules of the Road", count: 35 },
                { id: "kejaraSafety", label: "KEJARA & Safety", count: 14 }
            ],
            colourBlindCount: 8,
            theoryCount: 70,
            minutes: 75,
            colourBlindPass: 8,
            theoryPass: 56
        },
        {
            id: "kpp-standard",
            title: "KPP Standard (Car or Motorcycle Only)",
            name: "KPP Standard",
            description: "Car or Motorcycle Only",
            subtitle: "",
            sections: [
                { id: "colourBlind", label: "Colour Blind Test", count: 8 },
                { id: "roadSigns", label: "Road Signs", count: 15 },
                { id: "rulesOfTheRoad", label: "Rules of the Road", count: 25 },
                { id: "kejaraSafety", label: "KEJARA & Safety", count: 10 }
            ],
            colourBlindCount: 8,
            theoryCount: 50,
            minutes: 45,
            colourBlindPass: 8,
            theoryPass: 42
        }
    ];

    // ------------------------------------------------------------------
    // Mock Exam Sets per exam type. setNumber is the source of truth; the
    // display name ("Mock Exam Set 2") is generated from it. Every set of a
    // type shares that type's fixed structure; only the demo questions differ.
    // Only "Active" sets are shown to learners.
    // ------------------------------------------------------------------
    var EXAM_SETS = {
        "combined": [
            { setNumber: 1, status: "Active" },
            { setNumber: 2, status: "Active" },
            { setNumber: 3, status: "Inactive" }
        ],
        "kpp-standard": [
            { setNumber: 1, status: "Active" },
            { setNumber: 2, status: "Active" },
            { setNumber: 3, status: "Inactive" }
        ]
    };

    // ------------------------------------------------------------------
    // Colour Blind demo questions: a plain coloured circle drawn as an inline
    // SVG data URI (4 options, A-D). Not a real Ishihara plate.
    // ------------------------------------------------------------------
    var COLOURS = [
        { name: "Red", hex: "#DC2626" },
        { name: "Green", hex: "#16A34A" },
        { name: "Blue", hex: "#2563EB" },
        { name: "Yellow", hex: "#FACC15" },
        { name: "Orange", hex: "#EA580C" },
        { name: "Purple", hex: "#7E22CE" },
        { name: "Black", hex: "#0F172A" },
        { name: "Brown", hex: "#78350F" }
    ];

    function colourImage(hex) {
        var svg = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 160 160'><rect width='160' height='160' fill='%23F1F5F9'/><circle cx='80' cy='80' r='54' fill='" + hex.replace("#", "%23") + "'/></svg>";
        return "data:image/svg+xml;utf8," + svg;
    }

    function buildColourBlindQuestion(n) {
        var correct = COLOURS[(n - 1) % COLOURS.length];
        var options = [correct.name];
        var offset = 1;

        while (options.length < 4) {
            options.push(COLOURS[((n - 1) + offset * 3) % COLOURS.length].name);
            offset++;
        }

        // Rotate so the correct answer lands on a different letter per question.
        var correctIndex = (n - 1) % 4;
        var arranged = options.slice(1);
        arranged.splice(correctIndex, 0, correct.name);

        return {
            id: "cb-" + n,
            section: "colourBlind",
            text: "Demo colour test " + n + ": which colour is the circle in the image?",
            image: colourImage(correct.hex),
            imageAlt: "Demo colour test image: a single coloured circle.",
            options: arranged,
            correctIndex: correctIndex,
            explanation: "The circle in the demo image is " + correct.name.toLowerCase() + "."
        };
    }

    // ------------------------------------------------------------------
    // Theory demo questions (3 options, A-C). A few neutral hand-written
    // examples, then generated placeholders for the remaining indexes.
    // ------------------------------------------------------------------
    var THEORY_SAMPLES = [
        { text: "Before changing lanes, a driver should:", options: ["Check mirrors and signal first", "Change immediately", "Sound the horn only"], correctIndex: 0, explanation: "Checking mirrors and signalling lets other road users know your intention." },
        { text: "A red traffic light generally means a driver should:", options: ["Slow down and continue", "Stop and wait", "Speed up to cross"], correctIndex: 1, explanation: "A red traffic light is the signal to stop." },
        { text: "Seatbelts should generally be worn by:", options: ["The driver only", "Only on highways", "All occupants where fitted"], correctIndex: 2, explanation: "Wearing seatbelts reduces the risk of injury for every occupant." },
        { text: "A safe following distance mainly helps a driver to:", options: ["React in time to stop safely", "Save fuel only", "Avoid using mirrors"], correctIndex: 0, explanation: "A larger gap gives more time to react and brake." },
        { text: "Using a handheld mobile phone while driving is generally:", options: ["Safe", "Unsafe and distracting", "Required"], correctIndex: 1, explanation: "Handheld phone use takes attention away from the road." },
        { text: "When approaching a pedestrian crossing with people waiting, a driver should:", options: ["Accelerate past", "Sound the horn", "Slow down and be prepared to stop"], correctIndex: 2, explanation: "Pedestrians should be given a safe opportunity to cross." },
        { text: "Before overtaking, a driver should ensure:", options: ["The road ahead is clear and it is safe to do so", "The vehicle ahead is slow", "The horn is working"], correctIndex: 0, explanation: "Overtaking is only safe when the road ahead is clear." },
        { text: "Regular vehicle maintenance mainly helps to:", options: ["Increase emissions", "Maintain roadworthiness and safety", "Reduce visibility"], correctIndex: 1, explanation: "Maintained brakes, tyres and lights keep the vehicle safe." },
        { text: "Driving while tired is:", options: ["Acceptable on short trips", "Recommended at night", "A road safety risk"], correctIndex: 2, explanation: "Fatigue slows reactions and judgement." },
        { text: "A triangular warning sign with a red border generally indicates:", options: ["A warning", "A mandatory action", "General information"], correctIndex: 0, explanation: "Triangular signs with a red border are generally warning signs." }
    ];

    // Each theory question belongs to one of the three theory topics, which are also the exam sections.
    // The ten hand-written samples are tagged by what they are about; the generated ones are spread
    // across the topics so every topic has plenty of questions for a set to draw from.
    var SAMPLE_TOPICS = ["rulesOfTheRoad", "rulesOfTheRoad", "kejaraSafety", "rulesOfTheRoad", "kejaraSafety",
        "rulesOfTheRoad", "rulesOfTheRoad", "kejaraSafety", "kejaraSafety", "roadSigns"];

    function theoryTopic(n) {
        if (n <= SAMPLE_TOPICS.length) { return SAMPLE_TOPICS[n - 1]; }

        var slot = n % 12;                       // 4/12 road signs, 5/12 rules, 3/12 KEJARA & safety
        return slot < 4 ? "roadSigns" : (slot < 9 ? "rulesOfTheRoad" : "kejaraSafety");
    }

    function buildTheoryQuestion(n) {
        var sample = n <= THEORY_SAMPLES.length ? THEORY_SAMPLES[n - 1] : null;
        var question;

        if (sample) {
            question = {
                text: sample.text,
                options: sample.options.slice(),
                correctIndex: sample.correctIndex,
                explanation: sample.explanation
            };
        } else {
            var correctIndex = (n * 7) % 3;
            question = {
                text: "Demo theory question " + n + ": select the option marked as the demo answer.",
                options: [
                    "Demo answer A for question " + n,
                    "Demo answer B for question " + n,
                    "Demo answer C for question " + n
                ],
                correctIndex: correctIndex,
                explanation: "Demo explanation: the demo answer for question " + n + " is option " + String.fromCharCode(65 + correctIndex) + ". This is placeholder content, not official examination material."
            };
        }

        question.id = "th-" + n;
        question.section = "theory";
        question.topic = theoryTopic(n);
        question.image = null;
        question.imageAlt = null;

        // A few theory questions carry an optional image; most are text-only.
        if (n % 9 === 0) {
            question.image = DEMO_IMAGE;
            question.imageAlt = DEMO_IMAGE_ALT;
        }

        return question;
    }

    // Demo question bank used by both the exam builder and the Admin
    // "Manage Questions" selection UI (bank is larger than any single exam).
    var BANK_COLOUR_BLIND = 16;
    var BANK_THEORY = 120;
    var BANK = [];
    var i;

    for (i = 1; i <= BANK_COLOUR_BLIND; i++) { BANK.push(buildColourBlindQuestion(i)); }
    for (i = 1; i <= BANK_THEORY; i++) { BANK.push(buildTheoryQuestion(i)); }

    function clone(value) {
        return JSON.parse(JSON.stringify(value));
    }

    function findType(id) {
        for (var t = 0; t < EXAM_TYPES.length; t++) {
            if (EXAM_TYPES[t].id === id) { return EXAM_TYPES[t]; }
        }
        return null;
    }

    function bankBySection(section) {
        return BANK.filter(function (q) { return q.section === section; });
    }

    function bankByTopic(topic) {
        return BANK.filter(function (q) { return q.topic === topic; });
    }

    /** Takes `count` items starting at `offset`, wrapping around the end of the array. */
    function takeCircular(source, offset, count) {
        var picked = [];

        for (var k = 0; k < count; k++) {
            picked.push(source[(offset + k) % source.length]);
        }

        return picked;
    }

    function findSet(typeId, setNumber) {
        var list = EXAM_SETS[typeId] || [];
        var number = parseInt(setNumber, 10);

        for (var k = 0; k < list.length; k++) {
            if (list[k].setNumber === number) { return list[k]; }
        }
        return null;
    }

    var MockExamData = {
        getExamTypes: function () { return clone(EXAM_TYPES); },
        getExamType: function (id) {
            var type = findType(id);
            return type ? clone(type) : null;
        },
        isKnownType: function (id) { return findType(id) !== null; },
        totalQuestions: function (type) {
            return type.sections.reduce(function (sum, section) { return sum + section.count; }, 0);
        },
        setLabel: function (number) { return "Mock Exam Set " + number; },
        /** All sets of an exam type, or [] for an unknown type. */
        getSets: function (typeId) { return clone(EXAM_SETS[typeId] || []); },
        /** Only the Active sets of an exam type (what learners may see). */
        getActiveSets: function (typeId) {
            return clone((EXAM_SETS[typeId] || []).filter(function (set) { return set.status === "Active"; }));
        },
        /** The set, or null when unknown for that type. Does not check status. */
        getSet: function (typeId, setNumber) {
            var set = findSet(typeId, setNumber);
            return set ? clone(set) : null;
        },
        getQuestionBank: function () { return clone(BANK); },
        /**
         * Ordered exam questions for a type with continuous numbering (1..total).
         * setNumber (default 1) picks which slice of the demo bank the set uses, so
         * different sets show different demo questions with the same structure.
         */
        buildExam: function (typeId, setNumber) {
            var type = findType(typeId);
            if (!type) { return null; }

            var setIndex = Math.max(1, parseInt(setNumber, 10) || 1) - 1;
            var colourBlindBank = bankBySection("colourBlind");
            var colourBlindOffset = (setIndex * 8) % colourBlindBank.length;
            var questions = [];

            type.sections.forEach(function (section) {
                var source;

                if (section.id === "colourBlind") {
                    source = takeCircular(colourBlindBank, colourBlindOffset, section.count);
                } else {
                    // Theory sections draw only from their own topic, so "Road Signs" holds road sign questions.
                    var topicBank = bankByTopic(section.id);
                    source = takeCircular(topicBank, (setIndex * section.count) % topicBank.length, section.count);
                }

                source = clone(source);
                source.forEach(function (q) {
                    q.section = section.id;
                    q.sectionLabel = section.label;
                    questions.push(q);
                });
            });

            questions.forEach(function (q, index) { q.number = index + 1; });
            return questions;
        }
    };

    global.MockExamData = MockExamData;

    // ========================================================================
    // Learner UI: Mock Exam attempt
    // ========================================================================
    function letterFor(index) {
        return String.fromCharCode(65 + index);
    }

    function formatClock(totalSeconds) {
        var minutes = Math.floor(totalSeconds / 60);
        var seconds = totalSeconds % 60;
        return (minutes < 10 ? "0" : "") + minutes + ":" + (seconds < 10 ? "0" : "") + seconds;
    }

    function questionCountLabel(count) {
        return count + (count === 1 ? " Question" : " Questions");
    }

    function initAttemptPage() {
        var root = document.getElementById("mockExamAttemptRoot");

        if (!root) { return; }

        var type = MockExamData.getExamType(root.getAttribute("data-type"));
        var appRoot = root.getAttribute("data-app-root") || "";

        if (!type) { return; }

        var setsUrl = appRoot + "Learner/MockExamSets.aspx?type=" + encodeURIComponent(type.id);
        var setInfo = MockExamData.getSet(type.id, root.getAttribute("data-set"));

        // The code-behind can only check the query string shape; this catches a set that
        // does not exist for the type or is not Active, and never starts a broken exam.
        if (!setInfo || setInfo.status !== "Active") {
            window.location.replace(setsUrl);
            return;
        }

        var setName = MockExamData.setLabel(setInfo.setNumber);
        var examLabel = type.name + " \u2014 " + setName;
        var questions = MockExamData.buildExam(type.id, setInfo.setNumber);
        var total = questions.length;

        // ---- attempt state (frontend only) ----
        var state = null;
        var timerHandle = null;
        var endTime = 0;
        var announced = {};

        function byId(id) { return document.getElementById(id); }

        var viewInstructions = byId("mxViewInstructions");
        var viewAttempt = byId("mxViewAttempt");
        var viewResult = byId("mxViewResult");
        var viewReview = byId("mxViewReview");

        var examTitle = byId("mxExamTitle");
        var timerEl = byId("mxTimer");
        var timerWrap = byId("mxTimerWrap");
        var announceEl = byId("mxAnnounce");
        var positionEl = byId("mxPosition");
        var sectionEl = byId("mxSection");
        var progressFill = byId("mxProgressFill");
        var progressBar = byId("mxProgressBar");

        var imageWrap = byId("mxImageWrap");
        var imageEl = byId("mxImage");
        var textEl = byId("mxQuestionText");
        var optionsEl = byId("mxOptions");
        var flagBtn = byId("mxFlagBtn");
        var prevBtn = byId("mxPrevBtn");
        var nextBtn = byId("mxNextBtn");
        var finalSubmitBtn = byId("mxFinalSubmitBtn");

        var navToggle = byId("mxNavToggle");
        var navPanel = byId("mxNavPanel");
        var navSectionsEl = byId("mxNavSections");
        var navSubmitBtn = byId("mxNavSubmitBtn");
        var navSummary = byId("mxNavSummary");

        var confirmOverlay = byId("mxConfirmOverlay");
        var confirmText = byId("mxConfirmText");
        var confirmContinueBtn = byId("mxConfirmContinueBtn");
        var confirmSubmitBtn = byId("mxConfirmSubmitBtn");

        var navButtons = [];

        // ---- instructions ----
        byId("mxInstrTitle").textContent = type.name;
        byId("mxInstrSubtitle").textContent = setName;
        byId("mxInstrSubtitle").hidden = false;
        Array.prototype.forEach.call(document.querySelectorAll(".mx-back-to-sets"), function (link) {
            link.setAttribute("href", setsUrl);
        });
        byId("mxInstrQuestions").textContent = total;
        byId("mxInstrMinutes").textContent = type.minutes;
        byId("mxInstrSections").innerHTML = "";
        type.sections.forEach(function (section, index) {
            var item = document.createElement("li");
            item.className = "mx-intro__section";

            var number = document.createElement("span");
            number.className = "mx-intro__section-no";
            number.setAttribute("aria-hidden", "true");
            number.textContent = index + 1;
            item.appendChild(number);

            var text = document.createElement("span");
            text.className = "mx-intro__section-text";
            var label = document.createElement("strong");
            label.textContent = section.label;
            text.appendChild(label);
            var count = document.createElement("span");
            count.textContent = questionCountLabel(section.count);
            text.appendChild(count);
            item.appendChild(text);

            byId("mxInstrSections").appendChild(item);
        });
        byId("mxInstrPassCb").textContent = type.colourBlindPass + " / " + type.colourBlindCount;
        byId("mxInstrPassTh").textContent = type.theoryPass + " / " + type.theoryCount;
        examTitle.textContent = examLabel;

        function sectionRanges() {
            var start = 1;

            return type.sections.map(function (section) {
                var end = start + section.count - 1;
                var range = {
                    id: section.id,
                    label: section.label,
                    start: start,
                    end: end
                };
                start = end + 1;
                return range;
            });
        }

        var currentViewName = "instructions";

        function setView(name) {
            currentViewName = name;
            viewInstructions.hidden = name !== "instructions";
            viewAttempt.hidden = name !== "attempt";
            viewResult.hidden = name !== "result";
            viewReview.hidden = name !== "review";
            window.scrollTo(0, 0);
        }

        function freshState() {
            return {
                currentIndex: 0,
                answers: questions.map(function () { return null; }),
                flags: questions.map(function () { return false; }),
                remainingSeconds: type.minutes * 60,
                isSubmitted: false,
                autoSubmitted: false,
                result: null
            };
        }

        function unansweredCount() {
            return state.answers.filter(function (a) { return a === null; }).length;
        }

        // ---- timer (whole exam; never paused or reset between sections) ----
        function renderTimer() {
            timerEl.textContent = formatClock(state.remainingSeconds);
            timerWrap.classList.toggle("mx-timer--warning", state.remainingSeconds <= 300 && state.remainingSeconds > 60);
            timerWrap.classList.toggle("mx-timer--critical", state.remainingSeconds <= 60);
            timerEl.setAttribute("aria-label", Math.floor(state.remainingSeconds / 60) + " minutes " + (state.remainingSeconds % 60) + " seconds remaining");
        }

        function announceMilestones() {
            var milestones = [
                { at: 600, text: "10 minutes remaining" },
                { at: 300, text: "5 minutes remaining" },
                { at: 60, text: "1 minute remaining" }
            ];

            milestones.forEach(function (m) {
                if (state.remainingSeconds <= m.at && !announced[m.at] && type.minutes * 60 > m.at) {
                    announced[m.at] = true;
                    announceEl.textContent = m.text;
                }
            });
        }

        function tick() {
            state.remainingSeconds = Math.max(0, Math.ceil((endTime - Date.now()) / 1000));
            renderTimer();
            announceMilestones();

            if (state.remainingSeconds <= 0) {
                finalizeExam(true);
            }
        }

        function startTimer() {
            stopTimer();
            announced = {};
            announceEl.textContent = "";
            endTime = Date.now() + state.remainingSeconds * 1000;
            timerHandle = window.setInterval(tick, 1000);
        }

        function stopTimer() {
            if (timerHandle !== null) {
                window.clearInterval(timerHandle);
                timerHandle = null;
            }
        }

        // ---- navigator ----
        function buildNavigator() {
            navSectionsEl.innerHTML = "";
            navButtons = [];

            sectionRanges().forEach(function (section) {
                var heading = document.createElement("h4");
                heading.className = "mx-nav__group";
                heading.textContent = section.label;
                navSectionsEl.appendChild(heading);

                var grid = document.createElement("div");
                grid.className = "mx-grid";
                navSectionsEl.appendChild(grid);

                questions.forEach(function (q, index) {
                    if (q.section !== section.id) { return; }

                    var button = document.createElement("button");
                    button.type = "button";
                    button.className = "mx-q";

                    var num = document.createElement("span");
                    num.className = "mx-q__num";
                    num.textContent = String(q.number);
                    button.appendChild(num);

                    button.addEventListener("click", function () {
                        goTo(index, true);
                    });

                    grid.appendChild(button);
                    navButtons[index] = button;
                });
            });
        }

        function updateNavigator() {
            navButtons.forEach(function (button, index) {
                var isCurrent = index === state.currentIndex;
                var isFlagged = state.flags[index];
                var isAnswered = state.answers[index] !== null;
                var labelParts = [];

                button.classList.toggle("mx-q--current", isCurrent);
                button.classList.toggle("mx-q--flagged", isFlagged);
                button.classList.toggle("mx-q--answered", isAnswered && !isFlagged);

                if (isCurrent) { labelParts.push("current"); }
                if (isFlagged) { labelParts.push("flagged"); }
                labelParts.push(isAnswered ? "answered" : "unanswered");

                button.setAttribute("aria-label", "Question " + (index + 1) + ", " + labelParts.join(", "));
                button.title = "Question " + (index + 1) + ": " + labelParts.join(", ");

                if (isCurrent) {
                    button.setAttribute("aria-current", "true");
                } else {
                    button.removeAttribute("aria-current");
                }
            });

            var answered = total - unansweredCount();
            var flagged = state.flags.filter(Boolean).length;
            navSummary.textContent = answered + " of " + total + " answered" + (flagged ? " · " + flagged + " flagged" : "");
        }

        // ---- question rendering ----
        function setImage(wrap, img, question) {
            if (question.image) {
                img.src = question.image.indexOf("data:") === 0 ? question.image : appRoot + question.image;
                img.alt = question.imageAlt || "";
                wrap.hidden = false;
            } else {
                wrap.hidden = true;
                img.removeAttribute("src");
                img.alt = "";
            }
        }

        function renderOptions(question, index) {
            optionsEl.innerHTML = "";

            question.options.forEach(function (text, optionIndex) {
                var button = document.createElement("button");
                button.type = "button";
                button.className = "option-row";
                button.setAttribute("role", "radio");

                var isSelected = state.answers[index] === optionIndex;
                button.setAttribute("aria-checked", isSelected ? "true" : "false");

                if (isSelected) {
                    button.classList.add("option-row--selected");
                }

                var letter = document.createElement("span");
                letter.className = "option-row__letter";
                letter.textContent = letterFor(optionIndex);
                button.appendChild(letter);

                var textSpan = document.createElement("span");
                textSpan.className = "option-row__text";
                textSpan.textContent = text;
                button.appendChild(textSpan);

                // Answers stay editable until submission; only "selected" is shown, never correctness.
                button.addEventListener("click", function () {
                    if (state.isSubmitted) { return; }
                    state.answers[index] = optionIndex;
                    renderQuestion();
                });

                optionsEl.appendChild(button);
            });
        }

        function renderQuestion() {
            var index = state.currentIndex;
            var question = questions[index];
            var isLast = index === total - 1;

            positionEl.textContent = "Question " + question.number + " of " + total;
            sectionEl.textContent = question.sectionLabel;
            sectionEl.className = "badge mx-section-badge" + (question.section === "colourBlind" ? "" : " mx-section-badge--theory");

            progressFill.style.width = Math.round((question.number / total) * 100) + "%";
            progressBar.setAttribute("aria-valuemax", String(total));
            progressBar.setAttribute("aria-valuenow", String(question.number));

            setImage(imageWrap, imageEl, question);
            textEl.textContent = question.text;
            renderOptions(question, index);

            var flagged = state.flags[index];
            flagBtn.setAttribute("aria-pressed", flagged ? "true" : "false");
            flagBtn.textContent = flagged ? "⚑ Unflag" : "⚐ Flag";
            flagBtn.classList.toggle("mx-flag--on", flagged);

            prevBtn.disabled = index === 0;
            nextBtn.hidden = isLast;
            nextBtn.style.display = isLast ? "none" : "";
            finalSubmitBtn.hidden = !isLast;
            finalSubmitBtn.style.display = isLast ? "" : "none";

            updateNavigator();
        }

        function goTo(index, fromNavigator) {
            if (index < 0 || index >= total) { return; }
            state.currentIndex = index;
            renderQuestion();

            if (fromNavigator) {
                setNavOpen(false);
                if (window.matchMedia("(max-width: 991px)").matches) {
                    byId("mxQuestionCard").scrollIntoView({ block: "start" });
                }
            }
        }

        function setNavOpen(open) {
            navPanel.classList.toggle("is-open", open);
            navToggle.setAttribute("aria-expanded", open ? "true" : "false");
        }

        // ---- submit (single flow shared by every Submit Exam entry point) ----
        function requestSubmit() {
            if (state.isSubmitted) { return; }

            var unanswered = unansweredCount();

            // Only unanswered questions trigger a warning; flagged questions never do.
            if (unanswered > 0) {
                var flagged = state.flags.filter(function (f) { return f; }).length;

                confirmText.textContent = "You still have " + unanswered + " unanswered " + (unanswered === 1 ? "question" : "questions") + "." +
                    (flagged ? " " + flagged + " flagged " + (flagged === 1 ? "question is" : "questions are") + " also marked for review." : "");
                confirmOverlay.hidden = false;
                confirmContinueBtn.focus();
                return;
            }

            finalizeExam(false);
        }

        function gradeExam() {
            var cb = 0;
            var th = 0;
            var answered = 0;
            var sectionCorrect = {};

            questions.forEach(function (q, index) {
                var answer = state.answers[index];
                if (answer !== null) { answered++; }
                if (answer === q.correctIndex) {
                    sectionCorrect[q.section] = (sectionCorrect[q.section] || 0) + 1;
                    if (q.section === "colourBlind") { cb++; } else { th++; }
                }
            });

            var cbPass = cb >= type.colourBlindPass;
            var thPass = th >= type.theoryPass;

            return {
                colourBlindScore: cb,
                theoryScore: th,
                colourBlindPass: cbPass,
                theoryPass: thPass,
                overallPass: cbPass && thPass,
                answered: answered,
                unanswered: total - answered,
                sections: type.sections.map(function (section) {
                    return { id: section.id, label: section.label, score: sectionCorrect[section.id] || 0, total: section.count };
                })
            };
        }

        function finalizeExam(auto) {
            if (state.isSubmitted) { return; }

            stopTimer();
            confirmOverlay.hidden = true;
            state.isSubmitted = true;
            state.autoSubmitted = !!auto;
            state.result = gradeExam();
            renderResult();
            setView("result");
        }

        // ---- result ----
        function renderResult() {
            var r = state.result;

            byId("mxResultMeta").textContent = type.name + " · " + setName;

            var autoMessage = byId("mxResultAutoMsg");
            autoMessage.hidden = !state.autoSubmitted;

            var overall = byId("mxResultOverall");
            overall.textContent = (r.overallPass ? "PASS" : "FAIL");
            overall.className = "mx-result__overall " + (r.overallPass ? "mx-result__overall--pass" : "mx-result__overall--fail");

            byId("mxResultCbScore").textContent = r.colourBlindScore + " / " + type.colourBlindCount;
            setPassBadge(byId("mxResultCbStatus"), r.colourBlindPass);
            byId("mxResultCbReq").textContent = "Required: " + type.colourBlindPass + " / " + type.colourBlindCount;

            byId("mxResultThScore").textContent = r.theoryScore + " / " + type.theoryCount;
            setPassBadge(byId("mxResultThStatus"), r.theoryPass);
            byId("mxResultThReq").textContent = "Required: " + type.theoryPass + " / " + type.theoryCount;

            var scoreList = byId("mxResultSectionScores");
            scoreList.innerHTML = "";
            r.sections.filter(function (x) { return x.id !== "colourBlind"; }).forEach(function (section) {
                var item = document.createElement("li");
                item.className = "mx-result__score";

                var head = document.createElement("div");
                head.className = "mx-result__score-head";
                var name = document.createElement("span");
                name.className = "mx-result__score-name";
                name.textContent = section.label;
                head.appendChild(name);
                var value = document.createElement("strong");
                value.textContent = section.score + " / " + section.total;
                head.appendChild(value);
                item.appendChild(head);

                var track = document.createElement("div");
                track.className = "progress-track";
                track.setAttribute("role", "progressbar");
                track.setAttribute("aria-label", section.label + " score");
                track.setAttribute("aria-valuemin", "0");
                track.setAttribute("aria-valuemax", String(section.total));
                track.setAttribute("aria-valuenow", String(section.score));
                var fill = document.createElement("div");
                fill.className = "progress-track__fill";
                fill.style.width = (section.total === 0 ? 0 : (section.score / section.total) * 100) + "%";
                track.appendChild(fill);
                item.appendChild(track);

                scoreList.appendChild(item);
            });

            // Time taken (a timed-out exam used all of its time).
            var elapsed = state.autoSubmitted ? type.minutes * 60 : (type.minutes * 60 - state.remainingSeconds);
            byId("mxResultTime").textContent = "Time taken: " + formatClock(elapsed) + " of " + type.minutes + " minutes";

            // Where the most points were lost.
            var weakest = null;
            r.sections.forEach(function (section) {
                var ratio = section.total === 0 ? 1 : section.score / section.total;

                if (ratio < 1 && (weakest === null || ratio < weakest.ratio)) {
                    weakest = { ratio: ratio, section: section };
                }
            });

            var weakEl = byId("mxResultWeak");

            if (weakest) {
                weakEl.textContent = "Most points lost in: " + weakest.section.label + " (" + weakest.section.score + " / " + weakest.section.total + ")";
                weakEl.hidden = false;
            } else {
                weakEl.hidden = true;
            }

            // "Next Set" leads when there is one; otherwise Review Answers does.
            var nextSet = MockExamData.getActiveSets(type.id).filter(function (x) { return x.setNumber > setInfo.setNumber; })[0];
            var nextLink = byId("mxNextSetLink");

            if (nextSet) {
                nextLink.href = appRoot + "Learner/MockExamAttempt.aspx?type=" + encodeURIComponent(type.id) + "&set=" + encodeURIComponent(nextSet.setNumber);
                nextLink.hidden = false;
            } else {
                nextLink.hidden = true;
            }

            byId("mxReviewBtn").className = "btn " + (nextSet ? "btn-secondary" : "btn-primary");
        }

        function setPassBadge(el, passed) {
            el.textContent = passed ? "Pass" : "Fail";
            el.className = "mx-pass-badge " + (passed ? "mx-pass-badge--pass" : "mx-pass-badge--fail");
        }

        // ---- review ----
        function renderReview() {
            var list = byId("mxReviewList");
            var onlyMistakes = byId("mxMistakesOnly").checked;

            var correctCount = questions.filter(function (q, i) { return state.answers[i] === q.correctIndex; }).length;
            byId("mxReviewSub").textContent = type.name + " · " + setName + " · " + correctCount + " / " + total + " correct";
            list.innerHTML = "";

            questions.forEach(function (q, index) {
                var answer = state.answers[index];
                var outcome = answer === null ? "unanswered" : (answer === q.correctIndex ? "correct" : "incorrect");

                if (onlyMistakes && outcome === "correct") { return; }

                var item = document.createElement("article");
                item.className = "card practice-review__item mx-review-item mx-review-item--" + outcome;

                var head = document.createElement("div");
                head.className = "mx-review-item__head";

                var h = document.createElement("h3");
                h.textContent = "Question " + q.number;
                head.appendChild(h);

                var section = document.createElement("span");
                section.className = "badge" + (q.section === "colourBlind" ? "" : " mx-section-badge--theory");
                section.textContent = q.sectionLabel;
                head.appendChild(section);

                if (state.flags[index]) {
                    var flag = document.createElement("span");
                    flag.className = "mx-review-flag";
                    flag.textContent = "\u2691 Flagged";
                    head.appendChild(flag);
                }

                var status = document.createElement("span");
                status.className = "mx-outcome mx-outcome--" + outcome;
                status.textContent = outcome === "correct" ? "✓ Correct" : (outcome === "incorrect" ? "✕ Incorrect" : "Unanswered");
                head.appendChild(status);

                item.appendChild(head);

                if (q.image) {
                    var img = document.createElement("img");
                    img.className = "lesson-image";
                    img.src = q.image.indexOf("data:") === 0 ? q.image : appRoot + q.image;
                    img.alt = q.imageAlt || "";
                    item.appendChild(img);
                }

                var text = document.createElement("p");
                text.className = "practice-review__question-text";
                text.textContent = q.text;
                item.appendChild(text);

                addLine(item, "Your Answer", answer === null ? "Not answered" : letterFor(answer) + ". " + q.options[answer], outcome === "correct" ? "practice-review__answer--correct" : "");

                if (outcome !== "correct") {
                    addLine(item, "Correct Answer", letterFor(q.correctIndex) + ". " + q.options[q.correctIndex], "practice-review__answer--correct");
                    addLine(item, "Explanation", q.explanation, "practice-review__explanation");
                }

                list.appendChild(item);
            });

            if (!list.children.length) {
                var none = document.createElement("p");
                none.className = "form-hint";
                none.textContent = "No mistakes to show. Every answer was correct.";
                list.appendChild(none);
            }
        }

        function addLine(parent, label, value, valueClass) {
            var labelEl = document.createElement("p");
            labelEl.className = "practice-review__label";
            labelEl.textContent = label + ":";
            parent.appendChild(labelEl);

            var valueEl = document.createElement("p");
            valueEl.className = "practice-review__answer" + (valueClass ? " " + valueClass : "");
            valueEl.textContent = value;
            parent.appendChild(valueEl);
        }

        // ---- lifecycle ----
        function showInstructions() {
            stopTimer();
            state = freshState();
            confirmOverlay.hidden = true;
            setNavOpen(false);
            renderTimer();
            setView("instructions");
        }

        function startExam() {
            state = freshState();
            buildNavigator();
            renderTimer();
            renderQuestion();
            startTimer();
            setView("attempt");
        }

        byId("mxStartBtn").addEventListener("click", startExam);
        prevBtn.addEventListener("click", function () { goTo(state.currentIndex - 1, false); });
        nextBtn.addEventListener("click", function () { goTo(state.currentIndex + 1, false); });
        flagBtn.addEventListener("click", function () {
            state.flags[state.currentIndex] = !state.flags[state.currentIndex];
            renderQuestion();
        });
        finalSubmitBtn.addEventListener("click", requestSubmit);
        navSubmitBtn.addEventListener("click", requestSubmit);
        navToggle.addEventListener("click", function () {
            setNavOpen(navToggle.getAttribute("aria-expanded") !== "true");
        });
        confirmContinueBtn.addEventListener("click", function () {
            confirmOverlay.hidden = true;

            for (var i = 0; i < state.answers.length; i++) {
                if (state.answers[i] === null) {
                    goTo(i, false);
                    break;
                }
            }
        });
        confirmSubmitBtn.addEventListener("click", function () { finalizeExam(false); });

        byId("mxReviewBtn").addEventListener("click", function () {
            byId("mxMistakesOnly").checked = false;
            renderReview();
            setView("review");
        });
        byId("mxMistakesOnly").addEventListener("change", renderReview);

        // Leaving mid-exam (sidebar, back button, closing the tab) loses every answer; ask first.
        window.addEventListener("beforeunload", function (event) {
            if (currentViewName === "attempt" && state && !state.isSubmitted) {
                event.preventDefault();
                event.returnValue = "";
            }
        });
        byId("mxBackToResultBtn").addEventListener("click", function () { setView("result"); });
        byId("mxBackToResultTopBtn").addEventListener("click", function () { setView("result"); });
        byId("mxTryAgainBtn").addEventListener("click", showInstructions);

        state = freshState();
        setView("instructions");
        window.scrollTo(0, 0);
    }

    // ========================================================================
    // Learner UI: Mock Exam Sets list (Learner/MockExamSets.aspx)
    // ========================================================================
    function initSetsPage() {
        var container = document.getElementById("mockExamSetsContainer");

        if (!container) { return; }

        var type = MockExamData.getExamType(container.getAttribute("data-type"));

        if (!type) { return; }

        var appRoot = container.getAttribute("data-app-root") || "";
        var sets = MockExamData.getActiveSets(type.id);
        var attemptUrl = appRoot + "Learner/MockExamAttempt.aspx";

        container.innerHTML = "";

        if (sets.length === 0) {
            var empty = document.createElement("div");
            empty.className = "empty-state";
            var emptyTitle = document.createElement("div");
            emptyTitle.className = "empty-state__title";
            emptyTitle.textContent = "No active Mock Exam Sets are currently available.";
            empty.appendChild(emptyTitle);
            var back = document.createElement("a");
            back.className = "btn btn-secondary";
            back.href = appRoot + "Learner/MockExam.aspx";
            back.textContent = "Back to Mock Exam";
            empty.appendChild(back);
            container.className = "";
            container.appendChild(empty);
            return;
        }

        sets.forEach(function (set) {
            var card = document.createElement("div");
            card.className = "card mx-set-card";

            var title = document.createElement("h3");
            title.textContent = "Set " + set.setNumber;
            card.appendChild(title);

            var link = document.createElement("a");
            link.className = "btn btn-primary";
            link.href = attemptUrl + "?type=" + encodeURIComponent(type.id) + "&set=" + encodeURIComponent(set.setNumber);
            link.textContent = "Start Mock Exam";
            card.appendChild(link);

            container.appendChild(card);
        });
    }

    function init() {
        initSetsPage();
        initAttemptPage();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})(window);
