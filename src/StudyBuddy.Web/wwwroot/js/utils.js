// funzioni comuni

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

// mischia array
function shuffle(arr) {
    return arr.sort(function () { return Math.random() - 0.5; });
}

// mischia le carte (versione nuova)
function mischia(lista) {
    var copia = lista.slice();
    for (var i = copia.length - 1; i > 0; i--) {
        var j = Math.floor(Math.random() * (i + 1));
        var tmp = copia[i];
        copia[i] = copia[j];
        copia[j] = tmp;
    }
    return copia;
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
