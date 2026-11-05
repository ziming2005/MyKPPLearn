const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");

const repoRoot = path.resolve(__dirname, "..");
const markup = fs.readFileSync(
    path.join(repoRoot, "MyKPPLearn/Controls/ProfileEditor.ascx"),
    "utf8"
);
const css = fs.readFileSync(
    path.join(repoRoot, "MyKPPLearn/Content/css/app-shell.css"),
    "utf8"
);

assert.doesNotMatch(
    markup,
    /Click or tap the picture to change it\./,
    "The profile page should not render the removed photo helper text"
);
assert.doesNotMatch(
    css,
    /\.profile-photo-hint\s*\{/,
    "The removed helper text should not leave unused CSS"
);
assert.match(
    css,
    /\.profile-avatar__circle::after\s*\{[^}]*background:\s*rgba\([^)]+\);[^}]*opacity:\s*0;[^}]*transition:\s*opacity[^}]*\}/s,
    "The avatar should define a hidden dark overlay"
);
assert.match(
    css,
    /\.profile-avatar:hover\s+\.profile-avatar__circle::after,\s*\.profile-avatar:focus-visible\s+\.profile-avatar__circle::after\s*\{[^}]*opacity:\s*1;[^}]*\}/s,
    "Hover and keyboard focus should reveal the avatar overlay"
);
assert.doesNotMatch(
    css,
    /\.profile-avatar:hover\s+\.profile-avatar__badge/,
    "Hover should leave the camera badge unchanged"
);
assert.doesNotMatch(
    css,
    /\.profile-avatar__badge\s*\{[^}]*transition\s*:/s,
    "The static camera badge should not retain an unused transition"
);
assert.match(
    markup,
    /class="profile-avatar"[^\r\n]*aria-label="Change profile picture"/,
    "The avatar should retain its accessible action label"
);

console.log("Profile photo helper text is removed and only the avatar image receives hover feedback.");
