// ✨ Deck page - refactored as ES module
import { apiGet, apiPost, apiDelete } from "./api.js";

checkLogin(); // from utils.js

const id = getParam("id");

async function load() {
    const deck = await apiGet("/api/decks/" + id);

    document.getElementById("title").innerText = deck.name;
    document.getElementById("desc").innerText = deck.descrizione || "";
    document.getElementById("count").innerText = deck.cards.length;

    let html = "";
    deck.cards.forEach(c => {
        html += `<div class="card-row"><span><b>${escapeHtml(c.front)}</b> → ${escapeHtml(c.back)}</span>`;
        html += `<button class="btn btn-red" onclick="delCard(${c.id})">X</button></div>`;
    });
    document.getElementById("cards").innerHTML = html;
}

async function addCard() {
    const front = document.getElementById("front").value;
    const back = document.getElementById("back").value;
    await apiPost("/api/decks/" + id + "/cards", { front, back });
    document.getElementById("front").value = "";
    document.getElementById("back").value = "";
    load();
}

async function addQuestion() {
    const fields = ["op1", "op2", "op3", "op4"];
    // the select is 1-based (1-4), the server wants a 0-based index among the options it receives
    const chosen = parseInt(document.getElementById("corretta").value) - 1;

    // empty options are skipped, so the correct index must be recalculated among the kept ones
    const opzioni = [];
    let rispostaCorretta = -1;
    fields.forEach((f, i) => {
        const text = document.getElementById(f).value.trim();
        if (text == "") return;
        if (i == chosen) rispostaCorretta = opzioni.length;
        opzioni.push(text);
    });

    if (rispostaCorretta == -1) {
        alert("L'opzione indicata come corretta è vuota.");
        return;
    }

    const body = {
        domanda: document.getElementById("domanda").value.trim(),
        opzioni,
        rispostaCorretta
    };
    const result = await apiPost("/api/quiz/" + id + "/questions", body);
    if (result == null) {
        alert("Domanda non aggiunta: serve il testo della domanda e almeno 2 opzioni (senza il carattere |).");
        return;
    }

    ["domanda", ...fields].forEach(f => document.getElementById(f).value = "");
    alert("Domanda aggiunta!");
}

async function delCard(cardId) {
    await apiDelete("/api/cards/" + cardId);
    load();
}

async function deleteDeck() {
    if (confirm("Sicuro di voler eliminare il mazzo?")) {
        await apiDelete("/api/decks/" + id);
        window.location.href = "index.html";
    }
}

// needed because the module scope is not global
window.delCard = delCard;

document.getElementById("btnAddCard").addEventListener("click", addCard);
document.getElementById("btnAddQuestion").addEventListener("click", addQuestion);
document.getElementById("btnDelete").addEventListener("click", deleteDeck);

load();
