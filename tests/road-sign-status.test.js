const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");

const repoRoot = path.resolve(__dirname, "..");
const markup = fs.readFileSync(
    path.join(repoRoot, "MyKPPLearn/Admin/RoadSignAssets.aspx"),
    "utf8"
);
const codeBehind = fs.readFileSync(
    path.join(repoRoot, "MyKPPLearn/Admin/RoadSignAssets.aspx.cs"),
    "utf8"
);
const designer = fs.readFileSync(
    path.join(repoRoot, "MyKPPLearn/Admin/RoadSignAssets.aspx.designer.cs"),
    "utf8"
);

assert.doesNotMatch(
    markup,
    /ID="rblStatus"/,
    "The Add/Edit Road Sign form should not render a Status option"
);
assert.doesNotMatch(
    codeBehind,
    /rblStatus/,
    "Road Sign saves should not accept Status from the form"
);
assert.doesNotMatch(
    designer,
    /rblStatus/,
    "The removed Status control should not remain in the generated designer"
);
assert.match(
    codeBehind,
    /HiddenRoadSignAssetRepository\.Create\(name, newImagePath, true\);/,
    "New Road Signs should always be created as Active"
);
assert.match(
    codeBehind,
    /HiddenRoadSignAssetRepository\.Update\(editingId, name, newImagePath, \(bool\)existing\["IsActive"\]\);/,
    "Editing a Road Sign should preserve its existing database status"
);
assert.match(
    markup,
    /ID="btnStatus"[^>]+CommandName="toggle"/,
    "The Road Sign list should retain its status action"
);

console.log("Road Sign status is controlled from the list; new signs start Active.");
