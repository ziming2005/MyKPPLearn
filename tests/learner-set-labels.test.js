const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");

const repoRoot = path.resolve(__dirname, "..");

class TestElement {
    constructor(attributes) {
        this.attributes = attributes || {};
        this.children = [];
        this.textContent = "";
        this.className = "";
        this.href = "";
    }

    getAttribute(name) {
        return this.attributes[name] || null;
    }

    appendChild(child) {
        this.children.push(child);
        return child;
    }

    set innerHTML(value) {
        if (value === "") {
            this.children = [];
        }
    }
}

function renderSetCards(scriptRelativePath, containerId, attributes) {
    const container = new TestElement(attributes);
    let ready;
    const document = {
        readyState: "loading",
        createElement: () => new TestElement(),
        getElementById: (id) => id === containerId ? container : null,
        addEventListener: (name, callback) => {
            if (name === "DOMContentLoaded") {
                ready = callback;
            }
        }
    };
    const context = {
        document,
        encodeURIComponent,
        console,
        setTimeout,
        clearTimeout
    };
    context.window = context;

    const scriptPath = path.join(repoRoot, scriptRelativePath);
    vm.runInNewContext(fs.readFileSync(scriptPath, "utf8"), context, { filename: scriptPath });
    ready();

    return container.children.map((card) => card.children[0].textContent);
}

assert.deepEqual(
    renderSetCards(
        "MyKPPLearn/Scripts/practice-demo.js",
        "practiceSetsContainer",
        { "data-category": "road-signs" }
    ),
    ["Set 1", "Set 2"],
    "Learner Practice Set cards should use short Set N labels"
);

assert.deepEqual(
    renderSetCards(
        "MyKPPLearn/Scripts/mock-exam-demo.js",
        "mockExamSetsContainer",
        { "data-type": "combined", "data-app-root": "/" }
    ),
    ["Set 1", "Set 2"],
    "Learner Mock Exam Set cards should use short Set N labels"
);

const gameMarkup = fs.readFileSync(path.join(repoRoot, "MyKPPLearn/Learner/Game.aspx"), "utf8");
assert.match(
    gameMarkup,
    /<h3><%#\s*"Set " \+ Eval\("SetNumber"\)\s*%><\/h3>/,
    "Learner Hidden Road Sign Game cards should use short Set N labels"
);

const practiceSetsMarkup = fs.readFileSync(path.join(repoRoot, "MyKPPLearn/Learner/PracticeSets.aspx"), "utf8");
assert.match(
    practiceSetsMarkup,
    /practice-demo\.js"\) \+ "\?v=20261017-set-labels"/,
    "Learner Practice Sets should invalidate cached scripts for the new labels"
);

console.log("Learner set labels render as Set 1, Set 2, ...");
