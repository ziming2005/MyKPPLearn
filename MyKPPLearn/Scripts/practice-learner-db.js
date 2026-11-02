(function () {
    "use strict";
    var grid = document.getElementById("practiceCategoryGrid");
    if (!grid) { return; }
    fetch("Practice.aspx/LoadActive", { method: "POST", credentials: "same-origin", headers: { "Content-Type": "application/json; charset=utf-8" }, body: "{}" })
        .then(function (response) { return response.json(); })
        .then(function (payload) {
            var data = payload.d || { categories: [], sets: [] };
            grid.innerHTML = "";
            data.categories.forEach(function (category) {
                var sets = data.sets.filter(function (set) { return set.categoryCode === category.code; });
                var card = document.createElement("div"); card.className = "card practice-card";
                var title = document.createElement("h3"); title.textContent = category.title; card.appendChild(title);
                var description = document.createElement("p"); description.textContent = category.description || "Practice questions for this topic."; card.appendChild(description);
                var count = document.createElement("p"); count.className = "practice-card__count";
                var questionCount = sets.reduce(function (sum, set) { return sum + set.questionCount; }, 0);
                count.textContent = sets.length ? sets.length + (sets.length === 1 ? " set · " : " sets · ") + questionCount + (questionCount === 1 ? " question" : " questions") : "Coming soon"; card.appendChild(count);
                var row = document.createElement("div"); row.className = "practice-card__sets";
                if (!sets.length) { var empty = document.createElement("span"); empty.className = "btn btn-secondary practice-card__soon"; empty.textContent = "No sets yet"; row.appendChild(empty); }
                sets.slice(0, 4).forEach(function (set) { var link = document.createElement("a"); link.className = "btn btn-primary"; link.href = "PracticeAttempt.aspx?category=" + encodeURIComponent(category.code) + "&set=" + encodeURIComponent(set.number); link.textContent = "Set " + set.number; row.appendChild(link); });
                card.appendChild(row); grid.appendChild(card);
            });
        })
        .catch(function () { /* Existing empty state stays available if the request cannot be loaded. */ });
})();
