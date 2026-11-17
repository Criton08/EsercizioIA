// ✨ Deck page - refactored as ES module
import { apiGet, apiPost, apiDelete } from "./api.js";

checkLogin(); // from utils.js

const id = getParam("id");

async function load() {
    const deck = await apiGet("/api/decks/" + id);
    // missing deck, or a shared one (not editable): back to the list
    if (deck == null || deck.userId == null) {
        // no user left: utils.js is already going to the login page, do not send the user elsewhere
        if (getUser() == null) return;
        window.location.href = "index.html";
        return;
    }

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
    // before, a refused card (empty or too long) failed without telling the user
    if (front.trim() == "" || back.trim() == "") {
        setMessage("msgCard", "Scrivi sia il fronte sia il retro della carta.", "error");
        return;
    }
    const card = await apiPost("/api/decks/" + id + "/cards", { front, back });
    if (card == null) {
        setMessage("msgCard", "Carta non aggiunta: fronte e retro possono avere al massimo 500 caratteri.", "error");
        return;
    }
    document.getElementById("front").value = "";
    document.getElementById("back").value = "";
    setMessage("msgCard", "Carta aggiunta.", "ok");
    document.getElementById("front").focus(); // ready for the next card
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
        setMessage("msgQuestion", "L'opzione indicata come corretta è vuota: scrivila o scegli un'altra risposta corretta.", "error");
        return;
    }

    const body = {
        domanda: document.getElementById("domanda").value.trim(),
        opzioni,
        rispostaCorretta
    };
    const result = await apiPost("/api/quiz/" + id + "/questions", body);
    if (result == null) {
        setMessage("msgQuestion", "Domanda non aggiunta: serve il testo della domanda e almeno 2 opzioni (senza il carattere |).", "error");
        return;
    }

    ["domanda", ...fields].forEach(f => document.getElementById(f).value = "");
    setMessage("msgQuestion", "Domanda aggiunta al quiz.", "ok");
}

async function delCard(cardId) {
    await apiDelete("/api/cards/" + cardId);
    load();
}

// asks for confirmation inside the page, then deletes the deck and goes back to the list
function deleteDeck() {
    askConfirm(document.getElementById("deleteArea"),
        "Eliminare questo mazzo con tutte le sue carte e domande? Non si può annullare.", "Sì, elimina",
        async () => {
            if (!await apiDelete("/api/decks/" + id)) {
                setMessage("msgDelete", "Mazzo non eliminato: ricarica la pagina e riprova.", "error");
                return;
            }
            window.location.href = "index.html";
        });
}

// needed because the module scope is not global
window.delCard = delCard;

document.getElementById("btnAddCard").addEventListener("click", addCard);
document.getElementById("btnAddQuestion").addEventListener("click", addQuestion);
document.getElementById("btnDelete").addEventListener("click", deleteDeck);

load();
