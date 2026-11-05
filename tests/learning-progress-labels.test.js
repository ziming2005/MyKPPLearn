const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");

const repoRoot = path.resolve(__dirname, "..");
const markup = fs.readFileSync(
    path.join(repoRoot, "MyKPPLearn/Learner/Learn.aspx"),
    "utf8"
);
const codeBehind = fs.readFileSync(
    path.join(repoRoot, "MyKPPLearn/Learner/Learn.aspx.cs"),
    "utf8"
);
const designer = fs.readFileSync(
    path.join(repoRoot, "MyKPPLearn/Learner/Learn.aspx.designer.cs"),
    "utf8"
);

assert.doesNotMatch(
    markup,
    /litSummaryPercent/,
    "The Learning summary should not display a percentage"
);
assert.doesNotMatch(
    codeBehind,
    /litSummaryPercent/,
    "The removed summary percentage control should not be populated"
);
assert.doesNotMatch(
    designer,
    /litSummaryPercent/,
    "The removed summary percentage control should not remain in the designer"
);
assert.doesNotMatch(
    codeBehind,
    /return done \+ " of " \+ all[\s\S]*LearningDisplay\.Percent\(done, all\) \+ "%";/,
    "Chapter lesson-count labels should not include a percentage"
);

console.log("Learning progress labels show lesson counts without percentages.");
