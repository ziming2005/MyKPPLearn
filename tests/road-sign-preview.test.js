const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");

const css = fs.readFileSync(
    path.resolve(__dirname, "../MyKPPLearn/Content/css/admin.css"),
    "utf8"
);

function declarations(selector) {
    const escaped = selector.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    const match = css.match(new RegExp(escaped + "\\s*\\{([^}]*)\\}"));

    assert.ok(match, "Missing CSS rule for " + selector);
    return match[1].replace(/\s+/g, " ").trim();
}

const roadSignWrapper = declarations(".image-preview-box");

assert.doesNotMatch(
    roadSignWrapper,
    /(?:padding|border|background-color)\s*:/,
    "The Road Sign preview wrapper should not draw a full-width frame"
);
assert.equal(
    declarations(".image-preview-box img"),
    declarations(".qb-image-preview img"),
    "The Road Sign image should use the Question Bank preview sizing and frame"
);

console.log("Road Sign images use the Question Bank preview frame and responsive sizing.");
