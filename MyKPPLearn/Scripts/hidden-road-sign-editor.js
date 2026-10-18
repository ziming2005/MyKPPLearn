/*
 * Hidden Road Sign Game: Admin Add/Edit Game Set - Road Sign Scene Editor (vanilla JavaScript, no libraries).
 *
 * The whole Game Set is edited in ONE page-state session and saved by ONE final button (Save Game Set / Save Changes):
 *  - Nothing is written to the database while editing. Choosing the Scene Image, dragging a Road Sign from the
 *    library, moving/resizing it, writing its question, or removing it only change the state held here.
 *  - On the final Save the whole state is serialised into one hidden field and posted with the form; the server
 *    re-validates everything and saves it in a single transaction. If the server rejects it, the page comes back
 *    with this state restored (the browser cannot restore a chosen file, so an Add page asks for the Scene Image again).
 *  - A Road Sign is Configured only when it has a question, options A/B/C and exactly one correct answer. The status is
 *    calculated here from the current state. ANY incomplete road sign blocks the final Save.
 *
 * The placed sign IMAGE is both what the learner sees and the clickable target, so one placed sign is one item.
 * Geometry is held as PERCENTAGES of the Scene Image (x, y, width, height), never pixels. A sign keeps its aspect
 * ratio when it is placed and when it is resized.
 *
 * Administrator-entered text is only ever written with .value / .textContent (never innerHTML).
 */
(function () {
    "use strict";

    var root = document.querySelector(".hrs-editor");
    var stage = document.getElementById("hrsStage");

    if (!root || !stage) { return; }

    var img = stage.querySelector("img");
    var MIN_W = 2;          // smallest sign width, in percent of the Scene
    var KEY_STEP = 0.5;     // keyboard nudge, in percent
    var DEFAULT_W = 9;      // default dropped width, in percent of the Scene
    var MAX_DEFAULT_H = 16; // tall signs are limited to this height, in percent of the Scene
    var MAX_SCENE_BYTES = 4 * 1024 * 1024;

    function readJson(name) {
        try { return JSON.parse(root.getAttribute(name) || "[]"); } catch (e) { return []; }
    }

    var assets = readJson("data-assets");
    var stateField = root.querySelector('input[id$="hfState"]');
    var fileInput = document.querySelector('input[type="file"][id$="fuScene"]');
    var setNumberInput = document.querySelector('input[id$="txtSetNumber"]');
    var statusInputs = document.querySelectorAll('input[name$="rblStatus"]');
    var isEditMode = root.getAttribute("data-mode") === "edit";

    // ------------------------------------------------------------------
    // Editor state
    //   item: { key, id (database id of an existing road sign, 0 for a new one), assetId, assetName, img,
    //           x, y, w, h, question, a, b, c, correct (0-3), explanation, hasHistory }
    //   removed: database ids of existing road signs the administrator removed (deleted only by the final Save)
    // ------------------------------------------------------------------
    var items = [];
    var removed = [];
    var selKey = "";
    var keyCounter = 0;

    function fromServer(entry) {
        return {
            key: "e" + entry.id, id: entry.id, assetId: entry.assetId, assetName: entry.assetName, img: entry.img,
            x: entry.x, y: entry.y, w: entry.w, h: entry.h,
            question: entry.question || "",
            a: (entry.choices && entry.choices[0]) || "",
            b: (entry.choices && entry.choices[1]) || "",
            c: (entry.choices && entry.choices[2]) || "",
            correct: entry.correct || 0,
            explanation: entry.explanation || "",
            hasHistory: !!entry.hasHistory
        };
    }

    var restored = null;
    if (stateField && stateField.value) {
        try { restored = JSON.parse(stateField.value); } catch (e) { restored = null; }
    }

    if (restored && restored.items) {
        items = restored.items;
        removed = restored.removed || [];
        selKey = restored.sel || "";
        items.forEach(function (item) {
            var m = /^n(\d+)$/.exec(item.key || "");
            if (m) { keyCounter = Math.max(keyCounter, parseInt(m[1], 10)); }
        });
    } else {
        items = readJson("data-items").map(fromServer);
    }

    var notice = document.getElementById("hrsNotice");
    var listEl = document.getElementById("hrsList");
    var listEmptyEl = document.getElementById("hrsSceneListEmpty");
    var libraryEl = document.getElementById("hrsLibrary");
    var searchEl = document.getElementById("hrsSearch");
    var noMatchEl = document.getElementById("hrsLibraryEmpty");
    var selectedImg = document.getElementById("hrsSelectedImg");
    var selectedName = document.getElementById("hrsSelectedName");
    var selectedStatus = document.getElementById("hrsSelectedStatus");
    var removeBtn = document.getElementById("hrsRemoveBtn");
    var bodyEl = document.getElementById("hrsEditorBody");
    var noSceneEl = document.getElementById("hrsNoScene");

    function field(name) { return root.querySelector('[data-field="' + name + '"]'); }

    var f = {
        question: field("question"),
        a: field("a"), b: field("b"), c: field("c"),
        explanation: field("explanation")
    };
    var correctInputs = root.querySelectorAll('[data-field="correct"] input[type="radio"]');
    var textFields = [f.question, f.a, f.b, f.c, f.explanation];

    var drag = null;          // active pointer interaction on the stage (move / resize)
    var dirty = root.getAttribute("data-dirty") === "1";   // unsaved changes anywhere in the editor
    var allowLeave = false;   // set while the final Save / a confirmed Back is submitting
    var signEls = [];
    var ratios = {};          // image url -> natural width / height

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------
    function clamp(value, min, max) { return Math.min(Math.max(value, min), max); }
    function round2(value) { return Math.round(value * 100) / 100; }

    function findItem(key) {
        for (var i = 0; i < items.length; i++) {
            if (items[i].key === key) { return items[i]; }
        }
        return null;
    }

    function selectedItem() { return selKey ? findItem(selKey) : null; }

    function markDirty() { dirty = true; }

    // Same limits as the Question Bank and Mock Exam questions (and the server: HiddenRoadSignItemRepository).
    var MAX_QUESTION = 500, MAX_OPTION = 200, MAX_EXPLANATION = 500;

    /** The first pair of identical options (A/B/C, ignoring case and spaces), as a message; "" when all three differ. */
    function duplicateOptionMessage(item) {
        var texts = [item.a, item.b, item.c].map(function (t) { return t.trim().toLowerCase(); });

        for (var i = 0; i < texts.length; i++) {
            for (var j = i + 1; j < texts.length; j++) {
                if (texts[i] && texts[i] === texts[j]) {
                    return "Option " + "ABC".charAt(j) + " repeats Option " + "ABC".charAt(i) + ". Each option must be different.";
                }
            }
        }

        return "";
    }

    /** Complete = question, options A/B/C (all different), exactly one correct answer and an explanation. */
    function isComplete(item) {
        return item.question.trim().length > 0 && item.question.trim().length <= MAX_QUESTION
            && item.a.trim().length > 0 && item.b.trim().length > 0 && item.c.trim().length > 0
            && item.a.trim().length <= MAX_OPTION && item.b.trim().length <= MAX_OPTION && item.c.trim().length <= MAX_OPTION
            && duplicateOptionMessage(item) === ""
            && item.correct >= 1 && item.correct <= 3
            && item.explanation.trim().length > 0 && item.explanation.trim().length <= MAX_EXPLANATION;
    }

    function incompleteItems() { return items.filter(function (item) { return !isComplete(item); }); }

    function pointerPercentFrom(clientX, clientY) {
        var r = img.getBoundingClientRect();
        return {
            x: clamp((clientX - r.left) / r.width * 100, 0, 100),
            y: clamp((clientY - r.top) / r.height * 100, 0, 100)
        };
    }

    function pointerPercent(e) { return pointerPercentFrom(e.clientX, e.clientY); }

    // Keeps receiving move/up events if the pointer leaves the stage; harmless if unsupported.
    function capture(e) {
        try { stage.setPointerCapture(e.pointerId); } catch (err) { /* not an active pointer */ }
    }

    function showNotice(text, isError) {
        if (!notice) { return; }
        notice.textContent = text || "";
        notice.className = "alert " + (isError ? "alert-error" : "alert-success");
        notice.hidden = !text;
    }

    function sceneReady() {
        return !!img.getAttribute("src") && img.complete && img.naturalWidth > 0;
    }

    // ------------------------------------------------------------------
    // Aspect ratio
    // ------------------------------------------------------------------
    /** Natural width/height of the Scene Image (fallback: how it is currently displayed). */
    function sceneRatio() {
        if (img.naturalWidth && img.naturalHeight) { return img.naturalWidth / img.naturalHeight; }
        var r = img.getBoundingClientRect();
        return r.width && r.height ? r.width / r.height : 1;
    }

    /** Calls back with the natural width/height of a sign image (loading it once if needed). */
    function withRatio(url, done) {
        if (ratios[url]) { done(ratios[url]); return; }

        var probe = new Image();
        probe.onload = function () {
            ratios[url] = probe.naturalWidth && probe.naturalHeight ? probe.naturalWidth / probe.naturalHeight : 1;
            done(ratios[url]);
        };
        probe.onerror = function () { ratios[url] = 1; done(1); };
        probe.src = url;
    }

    /** Height-per-width in Scene percent units that keeps the sign undistorted. */
    function heightPerWidth(item) {
        var r = ratios[item.img];
        if (r) { return sceneRatio() / r; }
        return item.w > 0 ? item.h / item.w : 1;
    }

    // ------------------------------------------------------------------
    // Edit Question form <-> selected item (live: there is no Save Question)
    // ------------------------------------------------------------------
    var questionCount = document.getElementById("hrsQuestionCount");
    var explanationCount = document.getElementById("hrsExplanationCount");
    var formProblem = document.getElementById("hrsFormProblem");

    // Character counters, and the reason an option set is not accepted (identical options).
    function updateFormHints() {
        if (questionCount) { questionCount.textContent = String(f.question.value.length); }
        if (explanationCount) { explanationCount.textContent = String(f.explanation.value.length); }

        if (formProblem) {
            var item = selectedItem();
            var message = item ? duplicateOptionMessage({ a: f.a.value, b: f.b.value, c: f.c.value }) : "";
            formProblem.textContent = message;
            formProblem.hidden = !message;
        }
    }

    function fillFormFromItem(item) {
        f.question.value = item.question;
        f.a.value = item.a;
        f.b.value = item.b;
        f.c.value = item.c;
        f.explanation.value = item.explanation;
        Array.prototype.forEach.call(correctInputs, function (el) { el.checked = String(item.correct) === el.value; });
        updateFormHints();
    }

    function clearForm() {
        f.question.value = f.a.value = f.b.value = f.c.value = f.explanation.value = "";
        Array.prototype.forEach.call(correctInputs, function (el) { el.checked = false; });
        updateFormHints();
    }

    function readFormIntoItem(item) {
        item.question = f.question.value;
        item.a = f.a.value;
        item.b = f.b.value;
        item.c = f.c.value;
        item.explanation = f.explanation.value;

        item.correct = 0;
        Array.prototype.forEach.call(correctInputs, function (el) { if (el.checked) { item.correct = parseInt(el.value, 10); } });
    }

    function setFormEnabled(enabled) {
        textFields.forEach(function (el) { if (el) { el.disabled = !enabled; } });
        Array.prototype.forEach.call(correctInputs, function (el) { el.disabled = !enabled; });
    }

    function statusText(item) { return isComplete(item) ? "Configured" : "Incomplete"; }
    function statusClass(item) { return "hrs-status " + (isComplete(item) ? "hrs-status--ok" : "hrs-status--pending"); }

    function updateChrome() {
        var item = selectedItem();
        var ready = sceneReady();

        setFormEnabled(!!item);

        selectedName.textContent = item ? item.assetName : "None selected";

        if (item) {
            selectedImg.src = item.img;
            selectedImg.alt = item.assetName;
            selectedImg.hidden = false;
            selectedStatus.textContent = statusText(item);
            selectedStatus.className = statusClass(item);
            selectedStatus.hidden = false;
        } else {
            selectedImg.removeAttribute("src");
            selectedImg.hidden = true;
            selectedStatus.hidden = true;
        }

        if (removeBtn) { removeBtn.hidden = !item; }
        if (listEmptyEl) { listEmptyEl.hidden = items.length > 0; }

        // The Scene Editor is usable as soon as a Scene Image is available (no database record is needed).
        if (bodyEl) { bodyEl.hidden = !ready; }
        if (noSceneEl) {
            noSceneEl.hidden = ready;
            noSceneEl.textContent = items.length > 0 && !isEditMode
                ? "Choose the Scene Image again to continue. Your road signs and questions are still here."
                : "Upload a Scene Image to start placing road signs.";
        }
    }

    // ------------------------------------------------------------------
    // Rendering (placed signs + Scene Road Signs list)
    // ------------------------------------------------------------------
    function makeSign(item, selected) {
        var el = document.createElement("div");
        el.className = "hrs-sign" + (selected ? " is-selected" : "");
        el.setAttribute("data-key", item.key);
        el.style.left = item.x + "%";
        el.style.top = item.y + "%";
        el.style.width = item.w + "%";
        el.style.height = item.h + "%";

        var pic = document.createElement("img");
        pic.className = "hrs-sign__img";
        pic.src = item.img;
        pic.alt = "";
        pic.draggable = false;
        el.appendChild(pic);

        el.tabIndex = 0;
        el.setAttribute("role", "button");

        if (selected) {
            el.setAttribute("aria-label", "Selected road sign " + item.assetName + ". Use the arrow keys to move it, Shift plus arrow keys to resize it.");
            ["nw", "ne", "sw", "se"].forEach(function (dir) {
                var handle = document.createElement("span");
                handle.className = "hrs-handle hrs-handle--" + dir;
                handle.setAttribute("data-dir", dir);
                el.appendChild(handle);
            });
        } else {
            el.setAttribute("aria-label", "Road sign " + item.assetName + (isComplete(item) ? "" : " (incomplete)") + ". Press Enter to edit it.");
        }

        return el;
    }

    function render() {
        signEls.forEach(function (el) { if (el.parentNode) { el.parentNode.removeChild(el); } });
        signEls = [];

        items.forEach(function (item) {
            var el = makeSign(item, item.key === selKey);
            stage.appendChild(el);
            signEls.push(el);
        });

        renderList();
        updateChrome();
    }

    var TRASH_SVG = '<svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true" focusable="false">'
        + '<path fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" '
        + 'd="M3 6h18M8 6V4h8v2m-9 0 1 14h8l1-14M10 10v6m4-6v6"/></svg>';

    function renderList() {
        if (!listEl) { return; }
        listEl.textContent = "";

        items.forEach(function (item) {
            var li = document.createElement("li");
            li.className = "hrs-scenelist__row" + (item.key === selKey ? " is-selected" : "") + (isComplete(item) ? "" : " is-incomplete");

            var pick = document.createElement("button");
            pick.type = "button";
            pick.className = "hrs-scenelist__pick";
            pick.addEventListener("click", function () { selectItem(item.key); });

            var thumb = document.createElement("span");
            thumb.className = "hrs-scenelist__thumb";
            var pic = document.createElement("img");
            pic.src = item.img;
            pic.alt = "";
            pic.loading = "lazy";
            thumb.appendChild(pic);
            pick.appendChild(thumb);

            var text = document.createElement("span");
            text.className = "hrs-scenelist__text";
            var name = document.createElement("span");
            name.className = "hrs-scenelist__name";
            name.textContent = item.assetName;
            var status = document.createElement("span");
            status.className = statusClass(item);
            status.textContent = statusText(item);
            text.appendChild(name);
            text.appendChild(status);
            pick.appendChild(text);

            li.appendChild(pick);

            var trash = document.createElement("button");
            trash.type = "button";
            trash.className = "hrs-scenelist__trash";
            trash.title = "Remove from Scene";
            trash.setAttribute("aria-label", "Remove " + item.assetName + " from the scene");
            trash.innerHTML = TRASH_SVG;          // fixed markup, no user data
            trash.addEventListener("click", function () { removeItem(item); });
            li.appendChild(trash);

            listEl.appendChild(li);
        });
    }

    /** Light refresh while typing: only the list rows and the status chrome, never the stage or the form. */
    function refreshStatus() {
        renderList();
        updateChrome();
    }

    // ------------------------------------------------------------------
    // Selection
    // ------------------------------------------------------------------
    function selectItem(key) {
        if (key === selKey) { return; }

        var item = findItem(key);
        if (!item) { return; }

        selKey = key;
        fillFormFromItem(item);
        render();
    }

    function deselect() {
        if (!selKey) { return; }

        selKey = "";
        clearForm();
        render();
    }

    // ------------------------------------------------------------------
    // Placing a new sign from the library (editor state only: no database row yet)
    // ------------------------------------------------------------------
    function placeNew(asset, centerPercent) {
        if (!sceneReady()) { return; }

        withRatio(asset.img, function (r) {
            var k = sceneRatio() / r;          // height% per width%
            var w = DEFAULT_W;
            var h = k * w;

            if (h > MAX_DEFAULT_H) { h = MAX_DEFAULT_H; w = h / k; }
            if (w < MIN_W) { w = MIN_W; h = k * w; }
            if (w > 100) { w = 100; h = k * w; }
            if (h > 100) { h = 100; w = h / k; }

            var g = {
                x: round2(clamp(centerPercent.x - w / 2, 0, 100 - w)),
                y: round2(clamp(centerPercent.y - h / 2, 0, 100 - h)),
                w: round2(w),
                h: round2(h)
            };

            // Rounding must never push the sign past the edge.
            g.w = Math.min(g.w, round2(100 - g.x));
            g.h = Math.min(g.h, round2(100 - g.y));

            keyCounter++;

            var item = {
                key: "n" + keyCounter, id: 0, assetId: asset.id, assetName: asset.name, img: asset.img,
                x: g.x, y: g.y, w: g.w, h: g.h,
                question: "", a: "", b: "", c: "", correct: 0, explanation: "", hasHistory: false
            };

            ratios[item.img] = r;
            items.push(item);
            markDirty();
            showNotice("", false);

            selKey = item.key;
            fillFormFromItem(item);
            render();

            if (f.question) { f.question.focus({ preventScroll: true }); }
        });
    }

    // ------------------------------------------------------------------
    // Removing a sign from the scene (editor state; the final Save deletes it). Never touches the Road Sign Asset.
    // ------------------------------------------------------------------
    function removeItem(item) {
        if (item.id > 0 && item.hasHistory) {
            showNotice("This Hidden Road Sign Game has learner attempt history, so this road sign cannot be removed. Edit it instead, or deactivate the whole set.", true);
            return;
        }

        if (item.id > 0 && removed.indexOf(item.id) < 0) { removed.push(item.id); }

        var index = items.indexOf(item);
        if (index >= 0) { items.splice(index, 1); }

        if (item.key === selKey) {
            selKey = "";
            clearForm();
        }

        markDirty();
        showNotice("", false);
        render();
    }

    // ------------------------------------------------------------------
    // Pointer interactions on the stage (move / resize)
    // ------------------------------------------------------------------
    stage.addEventListener("pointerdown", function (e) {
        if (e.button !== undefined && e.button !== 0) { return; }

        var handle = e.target.closest ? e.target.closest(".hrs-handle") : null;
        var sign = e.target.closest ? e.target.closest(".hrs-sign") : null;
        var p = pointerPercent(e);
        var item = selectedItem();

        if (handle && sign && sign.classList.contains("is-selected") && item) {
            e.preventDefault();
            drag = { type: "resize", item: item, dir: handle.getAttribute("data-dir"), geom: { x: item.x, y: item.y, w: item.w, h: item.h }, k: heightPerWidth(item) };
            capture(e);
            return;
        }

        if (sign) {
            var key = sign.getAttribute("data-key");

            e.preventDefault();

            if (item && key === selKey) {
                drag = { type: "move", item: item, start: p, geom: { x: item.x, y: item.y, w: item.w, h: item.h } };
                capture(e);
            } else {
                selectItem(key);
            }
            return;
        }

        // Clicking empty space in the scene unselects the current road sign.
        if (item) { deselect(); }
    });

    function resizeGeom(geom, dir, p, k) {
        var left = dir.indexOf("w") >= 0, up = dir.indexOf("n") >= 0;
        var ax = left ? geom.x + geom.w : geom.x;        // the opposite corner stays fixed
        var ay = up ? geom.y + geom.h : geom.y;
        var maxW = Math.min(left ? ax : 100 - ax, (up ? ay : 100 - ay) / k);
        var w = Math.max(Math.abs(p.x - ax), Math.abs(p.y - ay) / k);

        w = clamp(w, Math.min(MIN_W, maxW), maxW);

        var h = w * k;
        return { x: left ? ax - w : ax, y: up ? ay - h : ay, w: w, h: h };
    }

    function applyGeom(item, g) {
        item.x = round2(g.x); item.y = round2(g.y); item.w = round2(g.w); item.h = round2(g.h);
        // Rounding must never push the sign past the Scene edge.
        item.w = Math.min(item.w, round2(100 - item.x));
        item.h = Math.min(item.h, round2(100 - item.y));
        positionSign(item);
        markDirty();
    }

    function positionSign(item) {
        var el = stage.querySelector('.hrs-sign[data-key="' + item.key + '"]');
        if (!el) { return; }
        el.style.left = item.x + "%";
        el.style.top = item.y + "%";
        el.style.width = item.w + "%";
        el.style.height = item.h + "%";
    }

    stage.addEventListener("pointermove", function (e) {
        if (!drag) { return; }

        var p = pointerPercent(e);
        var g;

        if (drag.type === "move") {
            g = {
                x: clamp(drag.geom.x + (p.x - drag.start.x), 0, 100 - drag.geom.w),
                y: clamp(drag.geom.y + (p.y - drag.start.y), 0, 100 - drag.geom.h),
                w: drag.geom.w,
                h: drag.geom.h
            };
        } else {
            g = resizeGeom(drag.geom, drag.dir, p, drag.k);
        }

        applyGeom(drag.item, g);
    });

    function endDrag(e) {
        if (!drag) { return; }

        drag = null;
        try { stage.releasePointerCapture(e.pointerId); } catch (err) { /* already released */ }
        render();
    }

    stage.addEventListener("pointerup", endDrag);
    stage.addEventListener("pointercancel", endDrag);

    // Keyboard: Enter opens a sign; arrows nudge the selected one (Shift resizes, keeping its shape).
    stage.addEventListener("keydown", function (e) {
        var sign = e.target.closest ? e.target.closest(".hrs-sign") : null;
        if (!sign) { return; }

        var key = sign.getAttribute("data-key");

        if ((e.key === "Enter" || e.key === " ") && key !== selKey) {
            e.preventDefault();
            selectItem(key);
            return;
        }

        var item = selectedItem();
        if (!item || key !== selKey) { return; }

        var dx = 0, dy = 0;
        if (e.key === "ArrowLeft") { dx = -KEY_STEP; }
        else if (e.key === "ArrowRight") { dx = KEY_STEP; }
        else if (e.key === "ArrowUp") { dy = -KEY_STEP; }
        else if (e.key === "ArrowDown") { dy = KEY_STEP; }
        else { return; }

        e.preventDefault();
        var g = { x: item.x, y: item.y, w: item.w, h: item.h };

        if (e.shiftKey) {
            var k = heightPerWidth(item);
            var w = clamp(g.w + dx + dy, Math.min(MIN_W, 100 - g.x), Math.min(100 - g.x, (100 - g.y) / k));
            g.w = w;
            g.h = w * k;
        } else {
            g.x = clamp(g.x + dx, 0, 100 - g.w);
            g.y = clamp(g.y + dy, 0, 100 - g.h);
        }

        applyGeom(item, g);
    });

    // ------------------------------------------------------------------
    // Road Sign Library (search + drag and drop). The library only ever offers signs to place:
    // there is deliberately no delete action here.
    // ------------------------------------------------------------------
    var dragState = null;     // library drag in progress
    var ghost = null;

    function renderLibrary() {
        if (!libraryEl) { return; }

        var query = searchEl ? searchEl.value.trim().toLowerCase() : "";
        var shown = 0;
        libraryEl.textContent = "";

        assets.forEach(function (asset) {
            if (query && asset.name.toLowerCase().indexOf(query) < 0) { return; }
            shown++;

            var card = document.createElement("div");
            card.className = "hrs-card";
            card.setAttribute("role", "listitem");
            card.tabIndex = 0;
            card.title = asset.name;
            card.setAttribute("aria-label", asset.name + ". Drag onto the scene, or press Enter to place it in the middle of the scene.");

            var frame = document.createElement("span");
            frame.className = "hrs-card__img";
            var pic = document.createElement("img");
            pic.src = asset.img;
            pic.alt = "";
            pic.loading = "lazy";
            pic.decoding = "async";
            pic.draggable = false;
            frame.appendChild(pic);
            card.appendChild(frame);

            var name = document.createElement("span");
            name.className = "hrs-card__name";
            name.textContent = asset.name;
            card.appendChild(name);

            card.addEventListener("pointerdown", function (e) { beginLibraryDrag(e, asset); });
            card.addEventListener("keydown", function (e) {
                if (e.key === "Enter" || e.key === " ") {
                    e.preventDefault();
                    placeNew(asset, { x: 50, y: 50 });
                }
            });

            libraryEl.appendChild(card);
        });

        if (noMatchEl) { noMatchEl.hidden = shown !== 0; }
    }

    function isOverScene(clientX, clientY) {
        var r = img.getBoundingClientRect();
        return clientX >= r.left && clientX <= r.right && clientY >= r.top && clientY <= r.bottom;
    }

    function moveGhost(clientX, clientY) {
        if (!ghost) { return; }
        ghost.style.left = clientX + "px";
        ghost.style.top = clientY + "px";
        stage.classList.toggle("is-drop-target", isOverScene(clientX, clientY));
    }

    function startGhost(asset, clientX, clientY) {
        ghost = document.createElement("img");
        ghost.className = "hrs-ghost";
        ghost.src = asset.img;
        ghost.alt = "";
        document.body.appendChild(ghost);
        document.body.classList.add("hrs-dragging");
        moveGhost(clientX, clientY);
    }

    function endLibraryDrag(drop, clientX, clientY) {
        var state = dragState;
        dragState = null;

        document.removeEventListener("pointermove", onLibraryMove);
        document.removeEventListener("pointerup", onLibraryUp);
        document.removeEventListener("pointercancel", onLibraryCancel);
        document.removeEventListener("touchmove", onLibraryTouchMove, { passive: false });

        if (state && state.timer) { window.clearTimeout(state.timer); }
        if (ghost && ghost.parentNode) { ghost.parentNode.removeChild(ghost); }
        ghost = null;
        document.body.classList.remove("hrs-dragging");
        stage.classList.remove("is-drop-target");

        if (state && state.active && drop && isOverScene(clientX, clientY)) {
            placeNew(state.asset, pointerPercentFrom(clientX, clientY));
        }
    }

    function activate(clientX, clientY) {
        if (!dragState || dragState.active) { return; }
        dragState.active = true;
        startGhost(dragState.asset, clientX, clientY);
    }

    function onLibraryMove(e) {
        if (!dragState || e.pointerId !== dragState.pointerId) { return; }

        var dist = Math.abs(e.clientX - dragState.startX) + Math.abs(e.clientY - dragState.startY);

        if (!dragState.active) {
            if (dragState.touch) {
                // Touch scrolls the library unless the sign was pressed and held first.
                if (dist > 10) { endLibraryDrag(false); }
                return;
            }
            if (dist < 5) { return; }
            activate(e.clientX, e.clientY);
        }

        moveGhost(e.clientX, e.clientY);
    }

    function onLibraryUp(e) {
        if (!dragState || e.pointerId !== dragState.pointerId) { return; }
        endLibraryDrag(true, e.clientX, e.clientY);
    }

    function onLibraryCancel(e) {
        if (!dragState || e.pointerId !== dragState.pointerId) { return; }
        endLibraryDrag(false);
    }

    function onLibraryTouchMove(e) {
        if (dragState && dragState.active && e.cancelable) { e.preventDefault(); }   // no page scroll while carrying a sign
    }

    function beginLibraryDrag(e, asset) {
        if (dragState || (e.button !== undefined && e.button !== 0)) { return; }

        var touch = e.pointerType === "touch";

        dragState = { asset: asset, pointerId: e.pointerId, startX: e.clientX, startY: e.clientY, active: false, touch: touch, timer: null };

        if (!touch) { e.preventDefault(); }                // no text selection / native image drag with a mouse
        else {
            var x = e.clientX, y = e.clientY;
            dragState.timer = window.setTimeout(function () { activate(x, y); }, 300);
        }

        document.addEventListener("pointermove", onLibraryMove);
        document.addEventListener("pointerup", onLibraryUp);
        document.addEventListener("pointercancel", onLibraryCancel);
        document.addEventListener("touchmove", onLibraryTouchMove, { passive: false });
    }

    // ------------------------------------------------------------------
    // Scene Image (chosen file is previewed locally; it is uploaded only by the final Save)
    // ------------------------------------------------------------------
    function onSceneChosen() {
        var file = fileInput && fileInput.files && fileInput.files[0];
        if (!file) { return; }

        var name = (file.name || "").toLowerCase();
        var okType = /\.(jpe?g|png|webp)$/.test(name);

        if (!okType) {
            showNotice("Scene Image must be a .jpg, .jpeg, .png or .webp file.", true);
            fileInput.value = "";
            return;
        }

        if (file.size <= 0 || file.size > MAX_SCENE_BYTES) {
            showNotice("Scene Image must be smaller than 4 MB.", true);
            fileInput.value = "";
            return;
        }

        showNotice("", false);
        img.src = URL.createObjectURL(file);
        markDirty();
    }

    img.addEventListener("load", function () { render(); });

    if (fileInput) { fileInput.addEventListener("change", onSceneChosen); }

    // ------------------------------------------------------------------
    // Wiring
    // ------------------------------------------------------------------
    if (searchEl) { searchEl.addEventListener("input", renderLibrary); }

    if (removeBtn) {
        removeBtn.addEventListener("click", function () {
            var item = selectedItem();
            if (item) { removeItem(item); }
        });
    }

    // Typing in Edit Question updates the selected road sign's state straight away.
    textFields.forEach(function (el) {
        if (!el) { return; }
        el.addEventListener("input", function () {
            var item = selectedItem();
            if (!item) { return; }
            readFormIntoItem(item);
            markDirty();
            refreshStatus();
            updateFormHints();
        });
    });

    Array.prototype.forEach.call(correctInputs, function (el) {
        el.addEventListener("change", function () {
            var item = selectedItem();
            if (!item) { return; }
            readFormIntoItem(item);
            markDirty();
            refreshStatus();
        });
    });

    if (setNumberInput) { setNumberInput.addEventListener("input", markDirty); }
    Array.prototype.forEach.call(statusInputs, function (el) { el.addEventListener("change", markDirty); });

    // Reloading, closing the tab or navigating away with unsaved work asks first.
    window.addEventListener("beforeunload", function (e) {
        if (dirty && !allowLeave) {
            e.preventDefault();
            e.returnValue = "";
            return "";
        }
    });

    function serialise() {
        return JSON.stringify({
            sel: selKey,
            removed: removed,
            items: items.map(function (item) {
                return {
                    key: item.key, id: item.id, assetId: item.assetId, assetName: item.assetName, img: item.img,
                    x: item.x, y: item.y, w: item.w, h: item.h,
                    question: item.question, a: item.a, b: item.b, c: item.c, correct: item.correct,
                    explanation: item.explanation, hasHistory: item.hasHistory
                };
            })
        });
    }

    // ------------------------------------------------------------------
    // "Incomplete road signs" dialog (replaces an inline message when the final Save is blocked)
    // ------------------------------------------------------------------
    var modal = document.getElementById("hrsIncompleteModal");
    var modalText = document.getElementById("hrsIncompleteText");
    var modalOk = document.getElementById("hrsIncompleteOk");
    var modalFirstKey = "";
    var modalReturnFocus = null;

    function showIncompleteModal(incomplete) {
        if (!modal) { return; }

        modalFirstKey = incomplete[0].key;
        modalReturnFocus = document.activeElement;

        modalText.textContent = "Complete all Road Sign questions before saving the Game Set.";

        modal.hidden = false;
        document.body.classList.add("hrs-modal-open");
        modalOk.focus();
    }

    function closeIncompleteModal(goToFirst) {
        if (!modal || modal.hidden) { return; }

        modal.hidden = true;
        document.body.classList.remove("hrs-modal-open");

        // Take the administrator straight to the first road sign that needs a question.
        if (goToFirst && modalFirstKey) {
            if (!selectedItem() || selKey !== modalFirstKey) { selectItem(modalFirstKey); }

            var target = document.getElementById("hrsFormTitle");
            if (target && target.scrollIntoView) { target.scrollIntoView({ behavior: "smooth", block: "center" }); }
            if (f.question) { f.question.focus({ preventScroll: true }); }
        } else if (modalReturnFocus && modalReturnFocus.focus) {
            modalReturnFocus.focus();
        }
    }

    if (modalOk) { modalOk.addEventListener("click", function () { closeIncompleteModal(true); }); }

    if (modal) {
        modal.addEventListener("click", function (e) { if (e.target === modal) { closeIncompleteModal(false); } });
        document.addEventListener("keydown", function (e) {
            if (!modal.hidden && e.key === "Escape") { e.preventDefault(); closeIncompleteModal(false); }
        });
    }

    // Used by the buttons' OnClientClick.
    window.hrsEditor = {
        /** Back to Game Sets: asks first when there is unsaved work. */
        confirmLeave: function () {
            if (!dirty) { allowLeave = true; return true; }

            if (window.confirm("You have unsaved changes. Leave without saving?")) {
                allowLeave = true;
                return true;
            }

            return false;
        },

        /** Final Save: blocks while any road sign is incomplete, otherwise puts the whole editor state in the form. */
        beforeSave: function () {
            var incomplete = incompleteItems();

            if (incomplete.length) {
                showIncompleteModal(incomplete);
                return false;
            }

            if (stateField) { stateField.value = serialise(); }
            allowLeave = true;
            return true;
        }
    };

    // ------------------------------------------------------------------
    // Initial state (also restores the editor after a server-side validation error)
    // ------------------------------------------------------------------
    var current = selectedItem();
    if (current) { fillFormFromItem(current); } else { selKey = ""; clearForm(); }

    // Load every sign's real shape so that nothing is ever resized with the wrong proportions.
    items.forEach(function (item) { withRatio(item.img, function () { }); });

    renderLibrary();
    render();
})();
