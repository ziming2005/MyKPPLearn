/*
 * Hidden Road Sign Game (learner). Vanilla JavaScript.
 *
 * Rules implemented here:
 *  - The road signs are VISIBLE: each configured Road Sign image is drawn over the Scene at its
 *    stored percentage position and size. Nothing else marks them: no outline, label, name,
 *    order number, hover effect or special cursor. The image bounds are the click target and
 *    clicks are hit-tested against those percentage rectangles.
 *  - A click outside every unfound road sign does nothing at all.
 *  - Clicking an unfound road sign counts it as FOUND immediately (independent of the answer) and
 *    opens the attempt modal. The modal cannot be dismissed before the question is answered.
 *  - Choosing A/B/C is checked at once by the server (read-only call, nothing is stored) and then locks.
 *  - Next closes the modal and shows a "found" marker on the scene. After the LAST road sign,
 *    Next sends the whole finished game to the server (one transaction) and opens the result page.
 *  - No timer, no wrong-click tracking, no resume. State only lives in this page; nothing is
 *    stored in localStorage or the database until the game is complete.
 *
 * Text from the database is only ever written with textContent (never innerHTML).
 */
(function () {
    "use strict";

    var root = document.querySelector(".hrs-game");
    var scene = document.getElementById("hrsScene");

    if (!root || !scene) { return; }

    var game;
    try { game = JSON.parse(root.getAttribute("data-game")); } catch (e) { game = null; }
    if (!game || !game.items || !game.items.length) { return; }

    var img = scene.querySelector("img");
    var modal = document.getElementById("hrsModal");
    var signImg = document.getElementById("hrsSignImg");
    var questionEl = document.getElementById("hrsQuestion");
    var optionsEl = document.getElementById("hrsOptions");
    var feedbackEl = document.getElementById("hrsFeedback");
    var explanationBlock = document.getElementById("hrsExplanationBlock");
    var explanationEl = document.getElementById("hrsExplanation");
    var errorEl = document.getElementById("hrsError");
    var nextBtn = document.getElementById("hrsNextBtn");
    var foundCountEl = document.getElementById("hrsFoundCount");
    var announceEl = document.getElementById("hrsAnnounce");

    var items = game.items;
    var total = items.length;
    var found = [];             // { itemId, choiceId, foundOrder } in the order discovered
    var foundIds = {};          // itemId -> entry
    var active = null;          // entry whose modal is open
    var busy = false;           // a server call is in flight
    var answered = false;       // the open question has been answered
    var submitting = false;
    var lastFocus = null;
    var allowLeave = false;     // true once the finished game is saved and we are moving to the result

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------
    function announce(text) { if (announceEl) { announceEl.textContent = text; } }

    function itemById(id) {
        for (var i = 0; i < items.length; i++) { if (items[i].id === id) { return items[i]; } }
        return null;
    }

    function postJson(url, payload) {
        return fetch(url, {
            method: "POST",
            credentials: "same-origin",
            headers: { "Content-Type": "application/json; charset=utf-8" },
            body: JSON.stringify(payload)
        }).then(function (res) {
            if (!res.ok) { throw new Error("HTTP " + res.status); }
            return res.json();
        }).then(function (json) {
            return json && json.d !== undefined ? json.d : json;
        });
    }

    function showError(text) {
        errorEl.textContent = text;
        errorEl.hidden = !text;
    }

    // The last question's button finishes the game instead of opening another sign.
    function nextLabel() { return found.length === total ? "Finish" : "Next"; }

    function updateProgress() {
        foundCountEl.textContent = String(found.length);
    }

    // ------------------------------------------------------------------
    // The road signs placed in the scene
    // ------------------------------------------------------------------
    // The layer ignores the pointer, so clicks always reach the scene and are hit-tested by position.
    var signLayer = document.createElement("div");
    signLayer.className = "hrs-signs";
    signLayer.setAttribute("aria-hidden", "true");

    // The stored rectangle (item.x/y/w/h, percent of the scene) can be a different shape from the sign
    // image: older items were drawn freehand. The image is shown "contained" in that rectangle, so the
    // part the learner actually SEES is the fitted rectangle computed here. That fitted rectangle (item.fit)
    // is what is drawn, what is hit-tested and what the found marker surrounds, so the click target is
    // always exactly the visible sign. Nothing stored is changed. For items placed by the Scene Editor the
    // stored rectangle already has the image's shape, so the fitted rectangle equals it.
    function sceneRatio() {
        if (img.naturalWidth && img.naturalHeight) { return img.naturalWidth / img.naturalHeight; }
        var r = img.getBoundingClientRect();
        return r.width && r.height ? r.width / r.height : 0;
    }

    function fitRect(item, signRatio) {
        // The formula is shared with the Admin preview (Scripts/road-sign-fit.js).
        return window.RoadSignFit ? window.RoadSignFit.fit(item, signRatio, sceneRatio()) : null;
    }

    function applyFit(item, pic) {
        var fit = fitRect(item, pic.naturalWidth && pic.naturalHeight ? pic.naturalWidth / pic.naturalHeight : 0);
        if (!fit) { return; }

        item.fit = fit;
        pic.style.left = fit.x + "%";
        pic.style.top = fit.y + "%";
        pic.style.width = fit.w + "%";
        pic.style.height = fit.h + "%";
    }

    function targetOf(item) { return item.fit || { x: item.x, y: item.y, w: item.w, h: item.h }; }

    var signPics = [];

    items.forEach(function (item) {
        if (!item.signUrl) { return; }

        var pic = document.createElement("img");
        pic.className = "hrs-sign";
        pic.alt = "";
        pic.draggable = false;
        // Until the sign image is measured, show it in its stored rectangle.
        pic.style.left = item.x + "%";
        pic.style.top = item.y + "%";
        pic.style.width = item.w + "%";
        pic.style.height = item.h + "%";
        signLayer.appendChild(pic);
        signPics.push({ item: item, pic: pic });

        pic.addEventListener("load", function () { applyFit(item, pic); });
        pic.src = item.signUrl;
        if (pic.complete) { applyFit(item, pic); }
    });

    // The scene's own shape may only be known once it has loaded.
    if (!img.complete) {
        img.addEventListener("load", function () {
            signPics.forEach(function (entry) { applyFit(entry.item, entry.pic); });
        });
    }

    scene.appendChild(signLayer);

    // ------------------------------------------------------------------
    // Searching the scene
    // ------------------------------------------------------------------
    // padX / padY (percent of the scene) widen every sign slightly for a finger tap, which is far less
    // precise than a mouse click. They are 0 for a mouse, so desktop behaviour is unchanged.
    function hitTest(px, py, padX, padY) {
        var best = null;
        var bestArea = Infinity;
        padX = padX || 0;
        padY = padY || 0;

        items.forEach(function (item) {
            if (foundIds[item.id]) { return; }

            var t = targetOf(item);      // the visible sign image, not the stored rectangle
            if (px >= t.x - padX && px <= t.x + t.w + padX && py >= t.y - padY && py <= t.y + t.h + padY) {
                var area = t.w * t.h;
                if (area < bestArea) { best = item; bestArea = area; }   // smallest wins when signs overlap
            }
        });

        return best;
    }

    scene.addEventListener("click", function (e) {
        if (active || submitting) { return; }

        var rect = img.getBoundingClientRect();
        if (!rect.width || !rect.height) { return; }

        var px = (e.clientX - rect.left) / rect.width * 100;
        var py = (e.clientY - rect.top) / rect.height * 100;

        if (px < 0 || px > 100 || py < 0 || py > 100) { return; }

        var touch = e.pointerType === "touch" || (e.pointerType === undefined && window.matchMedia && window.matchMedia("(pointer: coarse)").matches);
        var item = touch
            ? hitTest(px, py, 14 / rect.width * 100, 14 / rect.height * 100)   // about 14px of forgiveness round each sign
            : hitTest(px, py);

        // Not a road sign (or already found): no message, penalty or count, just a faint ripple so the tap feels received.
        if (!item) {
            showMiss(e.clientX - rect.left + (rect.left - scene.getBoundingClientRect().left), e.clientY - rect.top + (rect.top - scene.getBoundingClientRect().top));
            return;
        }

        discover(item);
    });

    function showMiss(x, y) {
        var ripple = document.createElement("span");
        ripple.className = "hrs-miss";
        ripple.setAttribute("aria-hidden", "true");
        ripple.style.left = x + "px";
        ripple.style.top = y + "px";
        scene.appendChild(ripple);
        window.setTimeout(function () { if (ripple.parentNode) { ripple.parentNode.removeChild(ripple); } }, 600);
    }

    function discover(item) {
        var entry = { itemId: item.id, choiceId: null, foundOrder: found.length + 1 };

        found.push(entry);
        foundIds[item.id] = entry;
        updateProgress();           // Found is counted now, regardless of the answer
        openModal(item, entry);
    }

    function addFoundMarker(item) {
        var marker = document.createElement("div");
        marker.className = "hrs-found";
        var t = targetOf(item);
        marker.style.left = t.x + "%";
        marker.style.top = t.y + "%";
        marker.style.width = t.w + "%";
        marker.style.height = t.h + "%";
        marker.setAttribute("role", "img");
        marker.setAttribute("aria-label", "Road sign found");

        var tag = document.createElement("span");
        tag.className = "hrs-found__tag";
        tag.textContent = "✓";
        marker.appendChild(tag);

        scene.appendChild(marker);
    }

    // ------------------------------------------------------------------
    // Attempt modal
    // ------------------------------------------------------------------
    function openModal(item, entry) {
        active = entry;
        answered = false;
        busy = false;
        lastFocus = document.activeElement;

        questionEl.textContent = item.question;
        showError("");
        feedbackEl.hidden = true;
        explanationBlock.hidden = true;
        explanationEl.textContent = "";
        nextBtn.disabled = true;
        nextBtn.textContent = nextLabel();

        if (item.signUrl) {
            signImg.src = item.signUrl;
            signImg.hidden = false;
        } else {
            signImg.hidden = true;
        }

        optionsEl.textContent = "";
        item.choices.forEach(function (choice) {
            var btn = document.createElement("button");
            btn.type = "button";
            btn.className = "option-row";
            btn.setAttribute("role", "radio");
            btn.setAttribute("aria-checked", "false");
            btn.setAttribute("data-choice-id", String(choice.id));

            var letter = document.createElement("span");
            letter.className = "option-row__letter";
            letter.setAttribute("aria-hidden", "true");
            letter.textContent = choice.letter;
            btn.appendChild(letter);

            var text = document.createElement("span");
            text.className = "option-row__text";
            text.textContent = choice.text;
            btn.appendChild(text);

            var mark = document.createElement("span");
            mark.className = "hrs-option__mark";
            mark.setAttribute("aria-hidden", "true");
            btn.appendChild(mark);

            btn.addEventListener("click", function () { onChoose(item, entry, choice, btn); });
            optionsEl.appendChild(btn);
        });

        modal.hidden = false;
        document.body.classList.add("hrs-modal-open");

        var panel = modal.querySelector(".hrs-modal");
        if (panel) {
            try {
                panel.focus({ preventScroll: true });
            } catch (e) {
                panel.focus();
            }
        }
        announce("Road sign found. " + item.question);
    }

    function closeModal() {
        modal.hidden = true;
        document.body.classList.remove("hrs-modal-open");
        active = null;

        if (lastFocus && lastFocus.focus && document.contains(lastFocus)) {
            lastFocus.focus();
        }
    }

    function setOptionsDisabled(disabled) {
        Array.prototype.forEach.call(optionsEl.querySelectorAll("button"), function (btn) { btn.disabled = disabled; });
    }

    function markOption(btn, kind) {
        btn.classList.add(kind === "correct" ? "option-row--correct" : "option-row--incorrect");
        btn.querySelector(".hrs-option__mark").textContent = kind === "correct" ? "✓" : "✗";
    }

    function onChoose(item, entry, choice, btn) {
        if (answered || busy || submitting) { return; }

        busy = true;
        showError("");
        setOptionsDisabled(true);

        postJson(game.checkUrl, { setId: game.setId, itemId: item.id, choiceId: choice.id })
            .then(function (res) {
                busy = false;

                if (!res || !res.ok) {
                    // Not locked: the learner can try choosing again.
                    setOptionsDisabled(false);
                    showError((res && res.error) || "Your answer could not be checked. Please try again.");
                    return;
                }

                answered = true;
                entry.choiceId = choice.id;
                btn.setAttribute("aria-checked", "true");

                Array.prototype.forEach.call(optionsEl.querySelectorAll("button"), function (other) {
                    other.disabled = true;
                    if (other !== btn) { other.classList.add("option-row--muted"); }
                });

                if (res.correct) {
                    markOption(btn, "correct");
                    announce("Correct.");
                } else {
                    markOption(btn, "incorrect");

                    var correctBtn = optionsEl.querySelector('button[data-choice-id="' + res.correctChoiceId + '"]');
                    if (correctBtn) {
                        correctBtn.classList.remove("option-row--muted");
                        markOption(correctBtn, "correct");
                    }

                    if (res.explanation) {
                        explanationEl.textContent = res.explanation;
                        feedbackEl.hidden = false;
                        explanationBlock.hidden = false;
                    }
                    announce("Incorrect." + (res.explanation ? " Explanation: " + res.explanation : ""));
                }

                nextBtn.disabled = false;
                nextBtn.focus();
            })
            .catch(function () {
                busy = false;
                setOptionsDisabled(false);
                showError("Your answer could not be checked. Please check your connection and try again.");
            });
    }

    nextBtn.addEventListener("click", function () {
        if (!answered || busy || submitting || !active) { return; }

        var item = itemById(active.itemId);

        // Every road sign found and answered: save the finished game and open the result page.
        if (found.length === total) {
            submitGame();
            return;
        }

        closeModal();
        if (item) { addFoundMarker(item); }
    });

    function submitGame() {
        submitting = true;
        nextBtn.disabled = true;
        nextBtn.textContent = "Saving…";
        showError("");

        var payload = {
            setId: game.setId,
            token: game.token,
            answers: found.map(function (entry) {
                return { itemId: entry.itemId, choiceId: entry.choiceId, foundOrder: entry.foundOrder };
            })
        };

        postJson(game.completeUrl, payload)
            .then(function (res) {
                if (res && res.ok && res.redirectUrl) {
                    allowLeave = true;
                    window.location.href = res.redirectUrl;
                    return;
                }

                submitting = false;
                nextBtn.disabled = false;
                nextBtn.textContent = nextLabel();
                showError((res && res.error) || "Your result could not be saved. Please try again.");
            })
            .catch(function () {
                submitting = false;
                nextBtn.disabled = false;
                nextBtn.textContent = nextLabel();
                showError("Your result could not be saved. Please check your connection and try again.");
            });
    }

    // The modal can't be dismissed: Escape does nothing, and Tab stays inside it.
    document.addEventListener("keydown", function (e) {
        if (modal.hidden) { return; }

        if (e.key === "Escape") {
            e.preventDefault();
            return;
        }

        if (e.key === "Tab") {
            var focusable = Array.prototype.filter.call(
                modal.querySelectorAll("button"),
                function (el) { return !el.disabled && el.offsetParent !== null; }
            );

            if (!focusable.length) { e.preventDefault(); return; }

            var first = focusable[0];
            var last = focusable[focusable.length - 1];

            if (e.shiftKey && document.activeElement === first) {
                e.preventDefault();
                last.focus();
            } else if (!e.shiftKey && document.activeElement === last) {
                e.preventDefault();
                first.focus();
            } else if (focusable.indexOf(document.activeElement) === -1) {
                e.preventDefault();
                first.focus();
            }
        }
    });

    // An unfinished game is never resumed: if the browser restores this page from its back/forward
    // cache, reload so the learner starts again from 0.
    // Leaving mid-game loses the signs found so far (only a finished game is saved); ask first.
    window.addEventListener("beforeunload", function (e) {
        if (found.length > 0 && !allowLeave) {
            e.preventDefault();
            e.returnValue = "";
        }
    });

    window.addEventListener("pageshow", function (e) {
        if (e.persisted) { window.location.reload(); }
    });

    updateProgress();
})();
