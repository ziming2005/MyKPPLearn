/*
 * ============================================================================
 * Standard Practice: category configuration and Learner UI logic (UI prototype).
 *
 * Nothing here is persisted to SQL Server and no sample content is preloaded: the practice set and question collections
 * below start EMPTY. The Learner pages therefore show their existing empty states, and the Admin pages start with no rows.
 * A later phase replaces the SETS / QUESTIONS collections with database-backed data; the UI functions only talk to the
 * PracticeDemoData object, so they stay unchanged.
 *
 * What is real configuration here (and must stay): the four question categories (ids, titles, descriptions, icons, colours).
 *
 * window.PracticeDemoData exposes read access to this data for both the Learner pages (this file) and the Admin prototype
 * (practice-admin.js).
 * ============================================================================
 */
(function (global) {
    "use strict";

    // ------------------------------------------------------------------
    // Categories (exactly four; Hidden Road Sign is a separate module)
    // ------------------------------------------------------------------
    var CATEGORY_COLOURS = {"colour-recognition": "red", "road-signs": "blue", "rules-of-the-road": "green", "kejara-safety": "purple"};

    var CATEGORIES = [
        {
            id: "colour-recognition",
            title: "Colour Recognition",
            description: "Practice identifying the meaning of key road and signal colours.",
            iconSvg: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="13.5" cy="6.5" r="1" /><circle cx="17.5" cy="10.5" r="1" /><circle cx="8.5" cy="7.5" r="1" /><circle cx="6.5" cy="12.5" r="1" /><path d="M12 2C6.5 2 2 6.5 2 12s4.5 10 10 10c.93 0 1.65-.75 1.65-1.69 0-.44-.18-.83-.44-1.12-.29-.29-.44-.65-.44-1.13a1.64 1.64 0 0 1 1.67-1.67h2c3.05 0 5.55-2.5 5.55-5.55C21.97 6.01 17.46 2 12 2z" /></svg>'
        },
        {
            id: "road-signs",
            title: "Road Signs",
            description: "Test your knowledge of warning, regulatory and information signs.",
            iconSvg: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M10.3 3.9 1.8 18a2 2 0 0 0 1.7 3h17a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0z" /><path d="M12 9v4M12 17h.01" /></svg>'
        },
        {
            id: "rules-of-the-road",
            title: "Rules of the Road",
            description: "Review common driving scenarios and correct road behaviour.",
            iconSvg: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 22 8 2M20 22 16 2M12 5v3M12 11v3M12 17v3" /></svg>'
        },
        {
            id: "kejara-safety",
            title: "KEJARA & Safety",
            description: "Practice questions on the demerit points system and road safety.",
            iconSvg: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" /><path d="m9 12 2 2 4-4" /></svg>'
        }
    ];

    // ------------------------------------------------------------------
    // Practice sets and questions: none are preloaded. Set shape: { id, categoryId, number, questionCount, status }.
    // Question shape: { id, setId, categoryId, text, options[], correctIndex, explanation, image, imageAlt, status }.
    // ------------------------------------------------------------------
    var SETS = [];
    var QUESTIONS = [];

    /** Single source of truth for a set's display name: derived from its number. */
    function setLabel(number) {
        return "Practice Set " + number;
    }

    function clone(value) {
        return JSON.parse(JSON.stringify(value));
    }

    function findCategory(id) {
        for (var i = 0; i < CATEGORIES.length; i++) {
            if (CATEGORIES[i].id === id) { return CATEGORIES[i]; }
        }
        return null;
    }

    var PracticeDemoData = {
        setLabel: setLabel,
        /** Colour name used for a category on the learner cards (red / blue / green / purple), or "" if unknown. */
        getCategoryColour: function (categoryId) {
            return CATEGORY_COLOURS[categoryId] || "";
        },
        getCategories: function () {
            return clone(CATEGORIES);
        },
        getCategoryById: function (categoryId) {
            var category = findCategory(categoryId);
            return category ? clone(category) : null;
        },
        isKnownCategory: function (categoryId) {
            return findCategory(categoryId) !== null;
        },
        /** All sets for a category (both Active and Inactive) — admin use. */
        getAllSets: function (categoryId) {
            return clone(SETS.filter(function (s) { return !categoryId || s.categoryId === categoryId; }));
        },
        /** Active sets only for a category, in set-number order — learner use. */
        getActiveSets: function (categoryId) {
            return clone(SETS
                .filter(function (s) { return s.categoryId === categoryId && s.status === "Active"; })
                .sort(function (a, b) { return a.number - b.number; }));
        },
        getSet: function (categoryId, number) {
            var n = parseInt(number, 10);
            var match = SETS.filter(function (s) { return s.categoryId === categoryId && s.number === n; })[0];
            return match ? clone(match) : null;
        },
        /** All questions belonging to a set, in authored order. */
        getQuestions: function (setId) {
            return clone(QUESTIONS.filter(function (q) { return q.setId === setId; }));
        },
        getAllQuestions: function () {
            return clone(QUESTIONS);
        }
    };

    global.PracticeDemoData = PracticeDemoData;

    // ========================================================================
    // Learner UI: category grid / practice sets list / practice attempt.
    // Each init function no-ops when its page container is not on the page.
    // ========================================================================

    // Every question has exactly three options: A, B and C. Anything beyond the third is never shown or accepted.
    var OPTION_LETTERS = "ABC";

    function letterFor(index) {
        return OPTION_LETTERS.charAt(index);
    }

    function optionsOf(question) {
        return (question.options || []).slice(0, OPTION_LETTERS.length);
    }

    var MAX_SET_BUTTONS = 4;

    function initCategoryPage() {
        var grid = document.getElementById("practiceCategoryGrid");

        if (!grid) { return; }

        var categories = PracticeDemoData.getCategories();
        grid.innerHTML = "";

        categories.forEach(function (category) {
            var activeSets = PracticeDemoData.getActiveSets(category.id);

            var card = document.createElement("div");
            card.className = "card practice-card" + (CATEGORY_COLOURS[category.id] ? " practice-card--" + CATEGORY_COLOURS[category.id] : "");

            // Icon and title side by side.
            var head = document.createElement("div");
            head.className = "practice-card__head";

            var iconWrap = document.createElement("div");
            iconWrap.className = "card-icon";
            // Fixed constant markup from our own mock data module above, not user input.
            iconWrap.innerHTML = category.iconSvg;
            head.appendChild(iconWrap);

            var heading = document.createElement("h3");
            heading.textContent = category.title;
            head.appendChild(heading);

            card.appendChild(head);

            var description = document.createElement("p");
            description.textContent = category.description;
            card.appendChild(description);

            var count = document.createElement("p");
            count.className = "practice-card__count";

            var questionTotal = activeSets.reduce(function (sum, set) { return sum + set.questionCount; }, 0);

            if (activeSets.length === 0) {
                count.textContent = "Coming soon";
            } else {
                count.textContent = activeSets.length + (activeSets.length === 1 ? " set" : " sets") +
                    " \u00B7 " + questionTotal + (questionTotal === 1 ? " question" : " questions");
            }

            card.appendChild(count);

            var footer = document.createElement("div");
            footer.className = "practice-card__footer";

            // One button per set, so a practice starts in a single click.
            var setRow = document.createElement("div");
            setRow.className = "practice-card__sets";

            activeSets.slice(0, MAX_SET_BUTTONS).forEach(function (set) {
                var link = document.createElement("a");
                link.className = "btn btn-primary";
                link.href = "PracticeAttempt.aspx?category=" + encodeURIComponent(category.id) + "&set=" + encodeURIComponent(set.number);
                link.textContent = "Set " + set.number;
                link.setAttribute("aria-label", "Start " + category.title + " practice, set " + set.number);
                setRow.appendChild(link);
            });

            if (activeSets.length === 0) {
                var soon = document.createElement("span");
                soon.className = "btn btn-secondary practice-card__soon";
                soon.setAttribute("aria-disabled", "true");
                soon.textContent = "No sets yet";
                setRow.appendChild(soon);
            }

            footer.appendChild(setRow);

            // Longer lists are browsed on the Practice Sets page instead of filling the card.
            if (activeSets.length > MAX_SET_BUTTONS) {
                var all = document.createElement("a");
                all.className = "practice-card__all";
                all.href = "PracticeSets.aspx?category=" + encodeURIComponent(category.id);
                all.textContent = "View all " + activeSets.length + " sets";
                footer.appendChild(all);
            }

            card.appendChild(footer);
            grid.appendChild(card);
        });
    }

    function initSetsPage() {
        var container = document.getElementById("practiceSetsContainer");

        if (!container) { return; }

        var categoryId = container.getAttribute("data-category");
        var sets = PracticeDemoData.getActiveSets(categoryId);

        container.innerHTML = "";

        if (sets.length === 0) {
            var empty = document.createElement("div");
            empty.className = "empty-state";
            var emptyTitle = document.createElement("div");
            emptyTitle.className = "empty-state__title";
            emptyTitle.textContent = "No practice sets are available in this category yet.";
            empty.appendChild(emptyTitle);
            container.appendChild(empty);
            return;
        }

        sets.forEach(function (set) {
            var card = document.createElement("div");
            card.className = "card practice-set-card";

            var title = document.createElement("h3");
            title.textContent = "Set " + set.number;
            card.appendChild(title);

            var count = document.createElement("p");
            count.className = "practice-set-card__count";
            count.textContent = set.questionCount + (set.questionCount === 1 ? " Question" : " Questions");
            card.appendChild(count);

            var link = document.createElement("a");
            link.className = "btn btn-primary btn-block";
            link.href = "PracticeAttempt.aspx?category=" + encodeURIComponent(categoryId) + "&set=" + encodeURIComponent(set.number);
            link.textContent = "Start Practice";
            card.appendChild(link);

            container.appendChild(card);
        });
    }

    function initAttemptPage() {
        var root = document.getElementById("practiceAttemptRoot");

        if (!root) { return; }

        var categoryId = root.getAttribute("data-category");
        var setNumber = root.getAttribute("data-set");
        var appRoot = root.getAttribute("data-app-root") || "";
        var category = PracticeDemoData.getCategoryById(categoryId);
        var setInfo = PracticeDemoData.getSet(categoryId, setNumber);

        // The ASPX code-behind already validates the category slug and that "set" is a
        // positive integer; it cannot also validate against the frontend-only mock data,
        // so this catches a set number that simply doesn't exist for the category.
        if (!category) { return; }

        if (!setInfo) {
            window.location.href = "PracticeSets.aspx?category=" + encodeURIComponent(categoryId);
            return;
        }

        var questions = PracticeDemoData.getQuestions(setInfo.id);

        // A set without questions cannot be practised (there is nothing to show): send the learner back to the set list.
        if (!questions.length) {
            window.location.href = "PracticeSets.aspx?category=" + encodeURIComponent(categoryId);
            return;
        }

        var total = questions.length;
        var answers = questions.map(function () { return { selectedIndex: null }; });
        var currentIndex = 0;

        var viewInstructions = document.getElementById("viewInstructions");
        var viewAttempt = document.getElementById("viewAttempt");
        var viewResult = document.getElementById("viewResult");
        var viewReview = document.getElementById("viewReview");

        var instrTitle = document.getElementById("instrTitle");
        var instrCount = document.getElementById("instrCount");
        var instrSubtitle = document.getElementById("instrSubtitle");
        var btnStartPractice = document.getElementById("btnStartPractice");

        var attemptCategoryTitle = document.getElementById("attemptCategoryTitle");
        var attemptAnsweredText = document.getElementById("attemptAnsweredText");
        var progressBar = document.getElementById("attemptProgressBar");
        var progressFill = document.getElementById("attemptProgressFill");

        var questionImageWrap = document.getElementById("questionImageWrap");
        var questionImage = document.getElementById("questionImage");
        var questionText = document.getElementById("questionText");
        var questionOptions = document.getElementById("questionOptions");
        var questionFeedback = document.getElementById("questionFeedback");

        var btnPrevQuestion = document.getElementById("btnPrevQuestion");
        var btnNextQuestion = document.getElementById("btnNextQuestion");
        var btnSubmitPractice = document.getElementById("btnSubmitPractice");

        var submitConfirmOverlay = document.getElementById("submitConfirmOverlay");
        var submitConfirmText = document.getElementById("submitConfirmText");
        var btnContinuePractice = document.getElementById("btnContinuePractice");
        var btnSubmitAnyway = document.getElementById("btnSubmitAnyway");

        var resultMeta = document.getElementById("resultMeta");
        var resultScore = document.getElementById("resultScore");
        var resultPercentage = document.getElementById("resultPercentage");
        var resultMessage = document.getElementById("resultMessage");
        var resCorrect = document.getElementById("resCorrect");
        var resIncorrect = document.getElementById("resIncorrect");
        var resUnanswered = document.getElementById("resUnanswered");
        var lnkNextSet = document.getElementById("lnkNextSet");
        var practiceCrumb = document.getElementById("practiceCrumb");
        var chkMistakesOnly = document.getElementById("chkMistakesOnly");
        var currentViewName = "instructions";
        var btnReviewAnswers = document.getElementById("btnReviewAnswers");
        var btnTryAgain = document.getElementById("btnTryAgain");

        var reviewList = document.getElementById("reviewList");
        var btnBackToResult = document.getElementById("btnBackToResult");

        instrTitle.textContent = category.title;
        instrSubtitle.textContent = setLabel(setInfo.number);
        instrCount.textContent = total + (total === 1 ? " question" : " questions");

        (function buildCrumb() {
            function crumbLink(text, href) {
                var a = document.createElement("a");
                a.href = href;
                a.textContent = text;
                return a;
            }

            function sep() {
                var span = document.createElement("span");
                span.setAttribute("aria-hidden", "true");
                span.textContent = "/";
                return span;
            }

            var current = document.createElement("span");
            current.setAttribute("aria-current", "page");
            current.textContent = setLabel(setInfo.number);

            practiceCrumb.appendChild(crumbLink("Practice", "Practice.aspx"));
            practiceCrumb.appendChild(sep());
            practiceCrumb.appendChild(crumbLink(category.title, "PracticeSets.aspx?category=" + encodeURIComponent(categoryId)));
            practiceCrumb.appendChild(sep());
            practiceCrumb.appendChild(current);
        })();
        attemptCategoryTitle.textContent = category.title + " — " + setLabel(setInfo.number);

        function setView(name) {
            currentViewName = name;
            practiceCrumb.hidden = name !== "instructions";
            viewInstructions.hidden = name !== "instructions";
            viewAttempt.hidden = name !== "attempt";
            viewResult.hidden = name !== "result";
            viewReview.hidden = name !== "review";
        }

        function answeredCount() {
            return answers.filter(function (a) { return a.selectedIndex !== null; }).length;
        }

        // Progress reflects the question currently being viewed (current / total), not how
        // many have been answered. Answering a question never changes this on its own;
        // only moving to a different question (Previous/Next/Start) does.
        function updateProgress() {
            var currentNumber = currentIndex + 1;
            attemptAnsweredText.textContent = "Question " + currentNumber + " of " + total;

            var pct = total === 0 ? 0 : Math.round((currentNumber / total) * 100);
            progressFill.style.width = pct + "%";
            progressBar.setAttribute("aria-valuemin", "1");
            progressBar.setAttribute("aria-valuemax", String(total));
            progressBar.setAttribute("aria-valuenow", String(currentNumber));
        }

        function renderOptions(question, state) {
            questionOptions.innerHTML = "";
            var answered = state.selectedIndex !== null;

            optionsOf(question).forEach(function (text, index) {
                var button = document.createElement("button");
                button.type = "button";
                button.className = "option-row";
                button.setAttribute("role", "radio");

                var isSelected = state.selectedIndex === index;
                var isCorrectOption = index === question.correctIndex;
                button.setAttribute("aria-checked", isSelected ? "true" : "false");

                var letter = document.createElement("span");
                letter.className = "option-row__letter";
                letter.textContent = letterFor(index);
                button.appendChild(letter);

                var textSpan = document.createElement("span");
                textSpan.className = "option-row__text";
                textSpan.textContent = text;
                button.appendChild(textSpan);

                if (answered) {
                    button.disabled = true;

                    // Visible state is colour + the option still reads its own text; a
                    // screen-reader-only aria-label carries the same "your answer" /
                    // "correct answer" context without a label shown on screen.
                    var stateLabel = "";

                    // A ✓ / ✕ mark at the end of the row repeats the colour (decorative; the aria-label carries the meaning).
                    var mark = null;

                    if (isSelected && isCorrectOption) {
                        button.classList.add("option-row--correct");
                        stateLabel = " — your answer, correct";
                        mark = "✓";
                    } else if (isSelected) {
                        button.classList.add("option-row--incorrect");
                        stateLabel = " — your answer, incorrect";
                        mark = "✕";
                    } else if (isCorrectOption) {
                        button.classList.add("option-row--correct");
                        stateLabel = " — correct answer";
                        mark = "✓";
                    } else {
                        button.classList.add("option-row--muted");
                    }

                    if (stateLabel) {
                        button.setAttribute("aria-label", letterFor(index) + ". " + text + stateLabel);
                    }

                    if (mark) {
                        var markSpan = document.createElement("span");
                        markSpan.className = "option-row__mark";
                        markSpan.setAttribute("aria-hidden", "true");
                        markSpan.textContent = mark;
                        button.appendChild(markSpan);
                    }
                } else {
                    button.addEventListener("click", function (event) {
                        event.preventDefault();
                        state.selectedIndex = index;
                        // Answering does not move to a different question, so progress
                        // (current / total) is unchanged here; just re-render the locked state.
                        renderQuestion(true);
                    });
                }

                questionOptions.appendChild(button);
            });
        }

        function renderFeedback(question, state) {
            questionFeedback.innerHTML = "";

            // Correct answers need no feedback panel at all: the green selected option is
            // enough on its own, and leaving the panel empty/hidden avoids a blank gap.
            var correct = state.selectedIndex !== null && state.selectedIndex === question.correctIndex;

            if (state.selectedIndex === null || correct) {
                questionFeedback.hidden = true;
                questionFeedback.className = "question-feedback";
                return;
            }

            questionFeedback.hidden = false;
            questionFeedback.className = "question-feedback question-feedback--incorrect";

            var title = document.createElement("p");
            title.className = "question-feedback__title";
            title.textContent = "✕ Incorrect";
            questionFeedback.appendChild(title);

            // The correct option is already shown in green among the choices above, so the
            // Correct Answer line here would just repeat it.
            var explLabel = document.createElement("p");
            explLabel.className = "question-feedback__explanation-label";
            explLabel.textContent = "Explanation";
            questionFeedback.appendChild(explLabel);

            var explanation = document.createElement("p");
            explanation.className = "question-feedback__explanation";
            explanation.textContent = question.explanation;
            questionFeedback.appendChild(explanation);
        }

        function setPracticeNavButtonHidden(button, hidden) {
            button.hidden = hidden;
            button.style.display = hidden ? "none" : "";
        }

        function renderQuestion(afterAnswer) {
            var question = questions[currentIndex];
            var state = answers[currentIndex];

            // Single source of truth for progress: always follows the question being shown.
            updateProgress();

            if (question.image) {
                questionImage.src = appRoot + question.image;
                questionImage.alt = question.imageAlt || "";
                questionImageWrap.hidden = false;
            } else {
                questionImageWrap.hidden = true;
                questionImage.removeAttribute("src");
                questionImage.alt = "";
            }

            questionText.textContent = question.text;

            renderOptions(question, state);
            renderFeedback(question, state);

            btnPrevQuestion.disabled = currentIndex === 0;

            var isLast = currentIndex === total - 1;
            setPracticeNavButtonHidden(btnNextQuestion, isLast);
            setPracticeNavButtonHidden(btnSubmitPractice, !isLast);

            // Once this question is answered, Next becomes the obvious next step.
            btnNextQuestion.className = "btn " + (state.selectedIndex !== null ? "btn-primary" : "btn-secondary");

            if (afterAnswer && !questionFeedback.hidden && questionFeedback.scrollIntoView) {
                questionFeedback.scrollIntoView({ block: "nearest", behavior: "smooth" });
            }
        }

        function showResult() {
            var correct = 0;

            questions.forEach(function (question, index) {
                if (answers[index].selectedIndex === question.correctIndex) {
                    correct++;
                }
            });

            var pct = total === 0 ? 0 : Math.round((correct / total) * 100);

            resultMeta.textContent = category.title + " · " + setLabel(setInfo.number);
            resultScore.textContent = correct + " / " + total;
            resultPercentage.textContent = pct + "%";

            var unanswered = total - answeredCount();
            resCorrect.textContent = String(correct);
            resIncorrect.textContent = String(total - correct - unanswered);
            resUnanswered.textContent = String(unanswered);

            resultMessage.textContent = pct >= 90 ? "Excellent work!"
                : (pct >= 70 ? "Good job. Keep practising to get even stronger."
                : (pct >= 50 ? "Getting there. Review your mistakes to improve."
                : "Keep practising. Review the answers to learn from your mistakes."));

            // "Next Set" is the main step when there is one; otherwise Review Answers is.
            var nextSet = PracticeDemoData.getActiveSets(categoryId).filter(function (x) { return x.number > setInfo.number; })[0];

            if (nextSet) {
                lnkNextSet.href = "PracticeAttempt.aspx?category=" + encodeURIComponent(categoryId) + "&set=" + encodeURIComponent(nextSet.number);
                lnkNextSet.textContent = "Next Set";
                lnkNextSet.hidden = false;
            } else {
                lnkNextSet.hidden = true;
            }

            btnReviewAnswers.className = "btn " + (nextSet ? "btn-secondary" : "btn-primary");

            setView("result");
        }

        function showSubmitConfirmation(unanswered) {
            submitConfirmText.textContent = "You still have " + unanswered + (unanswered === 1 ? " unanswered question." : " unanswered questions.");
            submitConfirmOverlay.hidden = false;
        }

        // Same review layout as the Mock Exam: outcome-coloured card, "Question N" with a ✓ Correct / ✕ Incorrect /
        // Unanswered label, then "Your Answer:", and (when not correct) "Correct Answer:" and "Explanation:".
        function addReviewLine(parent, label, value, valueClass) {
            var labelEl = document.createElement("p");
            labelEl.className = "practice-review__label";
            labelEl.textContent = label + ":";
            parent.appendChild(labelEl);

            var valueEl = document.createElement("p");
            valueEl.className = "practice-review__answer" + (valueClass ? " " + valueClass : "");
            valueEl.textContent = value;
            parent.appendChild(valueEl);
        }

        function renderReview() {
            reviewList.innerHTML = "";
            var onlyMistakes = chkMistakesOnly.checked;

            var correctCount = questions.filter(function (q, i) { return answers[i].selectedIndex === q.correctIndex; }).length;
            document.getElementById("reviewSub").textContent = category.title + " · " + setLabel(setInfo.number) + " · " + correctCount + " / " + total + " correct";

            questions.forEach(function (question, index) {
                var selected = answers[index].selectedIndex;
                var outcome = selected === null ? "unanswered" : (selected === question.correctIndex ? "correct" : "incorrect");

                if (onlyMistakes && outcome === "correct") { return; }

                var item = document.createElement("article");
                item.className = "card practice-review__item mx-review-item mx-review-item--" + outcome;

                var head = document.createElement("div");
                head.className = "mx-review-item__head";

                var heading = document.createElement("h3");
                heading.textContent = "Question " + (index + 1);
                head.appendChild(heading);

                var status = document.createElement("span");
                status.className = "mx-outcome mx-outcome--" + outcome;
                status.textContent = outcome === "correct" ? "✓ Correct" : (outcome === "incorrect" ? "✕ Incorrect" : "Unanswered");
                head.appendChild(status);

                item.appendChild(head);

                if (question.image) {
                    var img = document.createElement("img");
                    img.src = appRoot + question.image;
                    img.alt = question.imageAlt || "";
                    img.className = "lesson-image";
                    item.appendChild(img);
                }

                var questionPara = document.createElement("p");
                questionPara.className = "practice-review__question-text";
                questionPara.textContent = question.text;
                item.appendChild(questionPara);

                addReviewLine(item, "Your Answer",
                    selected === null ? "Not answered" : letterFor(selected) + ". " + question.options[selected],
                    outcome === "correct" ? "practice-review__answer--correct" : "");

                if (outcome !== "correct") {
                    addReviewLine(item, "Correct Answer", letterFor(question.correctIndex) + ". " + question.options[question.correctIndex], "practice-review__answer--correct");
                    addReviewLine(item, "Explanation", question.explanation, "practice-review__explanation");
                }

                reviewList.appendChild(item);
            });

            if (!reviewList.children.length) {
                var none = document.createElement("p");
                none.className = "form-hint";
                none.textContent = "No mistakes to show. Every answer was correct.";
                reviewList.appendChild(none);
            }
        }

        function resetAttempt() {
            answers = questions.map(function () { return { selectedIndex: null }; });
            currentIndex = 0;
            renderQuestion();
            setView("attempt");
        }

        btnStartPractice.addEventListener("click", function (event) {
            event.preventDefault();
            setView("attempt");
            renderQuestion();
        });

        btnPrevQuestion.addEventListener("click", function (event) {
            event.preventDefault();
            if (currentIndex > 0) {
                currentIndex--;
                renderQuestion();
            }
        });

        btnNextQuestion.addEventListener("click", function (event) {
            event.preventDefault();
            if (currentIndex < total - 1) {
                currentIndex++;
                renderQuestion();
            }
        });

        btnSubmitPractice.addEventListener("click", function (event) {
            event.preventDefault();
            var unanswered = total - answeredCount();

            if (unanswered === 0) {
                showResult();
                return;
            }

            showSubmitConfirmation(unanswered);
        });

        btnContinuePractice.addEventListener("click", function (event) {
            event.preventDefault();
            submitConfirmOverlay.hidden = true;

            for (var i = 0; i < answers.length; i++) {
                if (answers[i].selectedIndex === null) {
                    currentIndex = i;
                    renderQuestion();
                    break;
                }
            }
        });

        btnSubmitAnyway.addEventListener("click", function (event) {
            event.preventDefault();
            submitConfirmOverlay.hidden = true;
            showResult();
        });

        btnReviewAnswers.addEventListener("click", function (event) {
            event.preventDefault();
            chkMistakesOnly.checked = false;
            renderReview();
            setView("review");
        });

        btnBackToResult.addEventListener("click", function (event) {
            event.preventDefault();
            setView("result");
        });

        document.getElementById("btnBackToResultTop").addEventListener("click", function (event) {
            event.preventDefault();
            setView("result");
        });

        btnTryAgain.addEventListener("click", function (event) {
            event.preventDefault();
            resetAttempt();
        });
        chkMistakesOnly.addEventListener("change", renderReview);

        // Leaving mid-practice (sidebar, back button, closing the tab) loses the answers; ask first.
        window.addEventListener("beforeunload", function (event) {
            if (currentViewName === "attempt" && answeredCount() > 0) {
                event.preventDefault();
                event.returnValue = "";
            }
        });

        renderQuestion();
        setView("instructions");
    }

    document.addEventListener("DOMContentLoaded", function () {
        initCategoryPage();
        initSetsPage();
        initAttemptPage();
    });
})(window);
