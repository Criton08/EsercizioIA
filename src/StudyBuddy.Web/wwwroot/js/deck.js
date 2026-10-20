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
        html += `<div class="card-row"><span><b>${c.front}</b> → ${c.back}</span>`;
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
    const body = {
        domanda: document.getElementById("domanda").value,
        opzioni: [
            document.getElementById("op1").value,
            document.getElementById("op2").value,
            document.getElementById("op3").value,
            document.getElementById("op4").value
        ],
        rispostaCorretta: parseInt(document.getElementById("corretta").value)
    };
    await apiPost("/api/quiz/" + id + "/questions", body);
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
