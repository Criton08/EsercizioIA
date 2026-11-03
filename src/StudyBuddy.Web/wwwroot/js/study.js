checkLogin();

// variabili
var listaCarte = []; // le carte
var indice = 0; // indice della carta corrente
var showingBack = false;
var known = 0; // carte sapute
var idMazzo = getParam("deck");

fetch("/api/decks/" + idMazzo)
    .then(function (r) {
        if (!r.ok) throw new Error("deck not available (" + r.status + ")");
        return r.json();
    })
    .then(function (deck) {
        document.getElementById("title").innerText = deck.name;
        if (deck.cards.length == 0) {
            showMessage("Questo mazzo non ha carte da studiare.");
            return;
        }
        listaCarte = shuffle(deck.cards);
        show();
    })
    .catch(function () {
        showMessage("Impossibile caricare il mazzo. Prova ad accedere di nuovo.");
    });

// shows a message instead of the card (empty deck, load error)
function showMessage(text) {
    document.getElementById("card").style.display = "none";
    document.getElementById("counter").innerText = text;
    document.getElementById("hint").style.display = "none";
}

function show() {
    showingBack = false;
    var el = document.getElementById("card");
    el.className = "flashcard";
    el.innerText = listaCarte[indice].front;
    document.getElementById("counter").innerText = "Carta " + (indice + 1) + " di " + listaCarte.length;
    document.getElementById("buttons").style.display = "none";
}

function flip() {
    var el = document.getElementById("card");
    if (showingBack) {
        el.className = "flashcard";
        el.innerText = listaCarte[indice].front;
    } else {
        el.className = "flashcard back";
        el.innerText = listaCarte[indice].back;
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
    if (indice >= listaCarte.length) { // the last card must be shown too
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
