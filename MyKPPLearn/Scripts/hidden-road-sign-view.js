/*
 * Admin "Learner Scene Preview" (Game Set details). Read-only. The server draws each CONFIGURED road sign in its stored
 * rectangle (data-x/y/w/h, percent of the Scene Image); once the images are measured, each sign is moved to its
 * fitted visible bounds with the same formula the Learner game uses (Scripts/road-sign-fit.js), so the preview
 * matches what a learner sees. Nothing stored is changed and nothing is made interactive.
 */
(function () {
    "use strict";

    var scenes = document.querySelectorAll(".hrs-view-scene");

    Array.prototype.forEach.call(scenes, function (scene) {
        var sceneImg = scene.querySelector(":scope > img");
        var signs = scene.querySelectorAll(".hrs-view-sign");

        if (!sceneImg || !signs.length || !window.RoadSignFit) { return; }

        function apply(sign) {
            var box = {
                x: parseFloat(sign.getAttribute("data-x")),
                y: parseFloat(sign.getAttribute("data-y")),
                w: parseFloat(sign.getAttribute("data-w")),
                h: parseFloat(sign.getAttribute("data-h"))
            };

            if (!sceneImg.naturalWidth || !sceneImg.naturalHeight || !sign.naturalWidth || !sign.naturalHeight) { return; }

            var fit = window.RoadSignFit.fit(box,
                sign.naturalWidth / sign.naturalHeight,
                sceneImg.naturalWidth / sceneImg.naturalHeight);

            if (!fit) { return; }

            sign.style.left = fit.x + "%";
            sign.style.top = fit.y + "%";
            sign.style.width = fit.w + "%";
            sign.style.height = fit.h + "%";
        }

        function applyAll() {
            Array.prototype.forEach.call(signs, apply);
        }

        Array.prototype.forEach.call(signs, function (sign) {
            if (!sign.complete) { sign.addEventListener("load", function () { apply(sign); }); }
        });

        if (!sceneImg.complete) { sceneImg.addEventListener("load", applyAll); }

        applyAll();
    });
})();
