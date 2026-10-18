/*
 * Shared geometry for placed Road Sign images (used by the Learner game and by the Admin "Learner Scene Preview",
 * so both always show exactly the same thing).
 *
 * A Road Sign is stored as a rectangle in PERCENT of the Scene Image. Older items were drawn freehand, so the
 * rectangle may not have the sign image's shape. The image is shown "contained" in that rectangle: its natural
 * aspect ratio is kept and it is centred. fit() returns that visible rectangle, in the same percent units.
 *
 *   box         { x, y, w, h }   stored rectangle, percent of the Scene Image
 *   signRatio   natural width / height of the sign image
 *   sceneRatio  natural width / height of the Scene Image
 *
 * Returns null when a ratio is unknown. When the stored rectangle already has the image's shape the result
 * equals the stored rectangle.
 */
(function (global) {
    "use strict";

    global.RoadSignFit = {
        fit: function (box, signRatio, sceneRatio) {
            if (!box || !sceneRatio || !signRatio || !box.w || !box.h) { return null; }

            // The box is box.w x box.h percent of a scene that is sceneRatio times wider than tall.
            var boxRatio = (box.w * sceneRatio) / box.h;     // box width / box height in real pixels
            var w = box.w, h = box.h;

            if (boxRatio > signRatio) {
                w = box.h * signRatio / sceneRatio;           // too wide: the image is limited by the box height
            } else if (boxRatio < signRatio) {
                h = box.w * sceneRatio / signRatio;           // too tall: the image is limited by the box width
            }

            return { x: box.x + (box.w - w) / 2, y: box.y + (box.h - h) / 2, w: w, h: h };
        }
    };
})(window);
