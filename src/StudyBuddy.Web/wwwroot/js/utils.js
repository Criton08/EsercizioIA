// shared helper functions

// The "user" in localStorage only says who logged in on this browser; the real login is the auth cookie.
// The cookie can disappear while localStorage stays: it expires after 8 hours, and "Esci" on another
// localhost port deletes it too (cookies are shared by host, not by port). Then every API call answers 401
// and the page looked logged in but did nothing. Any 401 from the API now clears the stale login
// and goes back to the login page.
(function () {
    var originalFetch = window.fetch.bind(window);
    window.fetch = function (input, init) {
        return originalFetch(input, init).then(function (res) {
            var url = typeof input == "string" ? input : input.url;
            var isAuthCall = url.indexOf("/api/login") >= 0 || url.indexOf("/api/logout") >= 0 || url.indexOf("/api/register") >= 0;
            if (res.status == 401 && url.indexOf("/api/") >= 0 && !isAuthCall) {
                localStorage.removeItem("user");
                window.location.href = "login.html?scaduta=1";
            }
            return res;
        });
    };
})();

function getUser() {
    var u = localStorage.getItem("user");
    if (u == null) return null;
    return JSON.parse(u);
}

function checkLogin() {
    if (getUser() == null) {
        window.location.href = "login.html";
    }
}

function logout() {
    localStorage.removeItem("user");
    // also end the server session (auth cookie), then go back to the login page
    fetch("/api/logout", { method: "POST" }).finally(function () {
        window.location.href = "login.html";
    });
}

// shuffles a copy of the array (Fisher-Yates).
// sort() with a random comparator is biased: some orders come out far more often than others.
function shuffle(arr) {
    var copy = arr.slice();
    for (var i = copy.length - 1; i > 0; i--) {
        var j = Math.floor(Math.random() * (i + 1));
        var tmp = copy[i];
        copy[i] = copy[j];
        copy[j] = tmp;
    }
    return copy;
}

// shows a message inside the page instead of a blocking alert().
// type: "ok" (disappears after a few seconds) or "error" (stays until the next message)
function setMessage(elementId, text, type) {
    var el = document.getElementById(elementId);
    clearTimeout(el._timer);
    el.className = "msg " + (type || "ok");
    el.innerText = text;
    if (type != "error") {
        el._timer = setTimeout(function () { el.innerText = ""; }, 4000);
    }
}

// asks for confirmation inside the page instead of a blocking confirm().
// The box is added at the end of "container"; onYes runs only if the user clicks the confirm button.
function askConfirm(container, text, yesLabel, onYes) {
    if (container.querySelector(".confirm")) return; // already asking
    var box = document.createElement("div");
    box.className = "confirm";
    box.setAttribute("role", "alertdialog");
    var p = document.createElement("p");
    p.innerText = text;
    var yes = document.createElement("button");
    yes.className = "btn btn-red";
    yes.innerText = yesLabel;
    var no = document.createElement("button");
    no.className = "btn btn-outline";
    no.innerText = "Annulla";
    yes.onclick = function () { box.remove(); onYes(); };
    no.onclick = function () { box.remove(); };
    box.append(p, yes, no);
    container.appendChild(box);
    no.focus(); // the safe choice gets the keyboard focus
}

function escapeHtml(text) {
    var div = document.createElement("div");
    div.innerText = text;
    return div.innerHTML;
}

function getParam(name) {
    var params = new URLSearchParams(window.location.search);
    return params.get(name);
}
