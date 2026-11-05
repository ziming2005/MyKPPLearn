const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");

const repoRoot = path.resolve(__dirname, "..");
const markup = fs.readFileSync(
    path.join(repoRoot, "MyKPPLearn/Learner/MockExamAttempt.aspx"),
    "utf8"
);
const script = fs.readFileSync(
    path.join(repoRoot, "MyKPPLearn/Scripts/mock-exam.js"),
    "utf8"
);
const styles = fs.readFileSync(
    path.join(repoRoot, "MyKPPLearn/Content/css/learner.css"),
    "utf8"
);

assert.doesNotMatch(
    markup,
    /mxResultWeak/,
    "The Mock Exam result should not render the most-points-lost callout"
);
assert.doesNotMatch(
    script,
    /Most points lost|mxResultWeak/,
    "The Mock Exam result script should not calculate or populate the removed callout"
);
assert.doesNotMatch(
    styles,
    /\.mx-result__weak\s*\{/,
    "The removed callout should not leave unused styling"
);
assert.match(
    markup,
    /<h3>Theory Breakdown<\/h3>/,
    "The Theory Breakdown should remain on the result page"
);

console.log("Mock Exam results omit the most-points-lost callout.");
