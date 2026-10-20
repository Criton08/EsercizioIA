checkLogin();

// variabili
var listaCarte = []; // le carte
var indice = 0; // indice della carta corrente
var showingBack = false;
var known = 0; // carte sapute
var idMazzo = getParam("deck");

fetch("/api/decks/" + idMazzo)
    .then(function (r) { return r.json(); })
    .then(function (deck) {
        document.getElementById("title").innerText = deck.name;
        listaCarte = shuffle(deck.cards);
        show();
    });

function show() {
    showingBack = false;
    var el = document.getElementById("card");
    el.className = "flashcard";
    el.innerHTML = listaCarte[indice].front;
    document.getElementById("counter").innerText = "Carta " + (indice + 1) + " di " + listaCarte.length;
    document.getElementById("buttons").style.display = "none";
}

function flip() {
    var el = document.getElementById("card");
    if (showingBack) {
        el.className = "flashcard";
        el.innerHTML = listaCarte[indice].front;
    } else {
        el.className = "flashcard back";
        el.innerHTML = listaCarte[indice].back;
        document.getElementById("buttons").style.display = "block";
    }
    showingBack = !showingBack;
}

function answer(isKnown) {
    var url = "/api/cards/" + listaCarte[indice].id + (isKnown ? "/known" : "/unknown");
    fetch(url, { method: "PUT" });
    if (isKnown) known++;
    setTimeout(next, 250);
}

// passa alla carta successiva
function next() {
    indice++; // incrementa indice
    if (indice >= listaCarte.length - 1) {
        finish();
        return;
    }
    show();
}

function finish() {
    document.getElementById("card").style.display = "none";
    document.getElementById("buttons").style.display = "none";
    document.getElementById("counter").style.display = "none";
    document.getElementById("end").style.display = "block";
    document.getElementById("summary").innerText = "Ne sapevi " + known + " su " + listaCarte.length;
}
