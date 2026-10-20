// 📊 Stats page
const API_URL = "http://localhost:5200";

checkLogin();

async function loadStats() {
    const user = getUser();
    const res = await fetch(API_URL + "/api/stats?userId=" + user.id);
    const stats = await res.json();

    document.getElementById("totale").innerText = stats.totale;
    document.getElementById("media").innerText = stats.media + "%";

    if (stats.risultati.length == 0) {
        document.getElementById("empty").style.display = "block";
        return;
    }

    const table = document.getElementById("tabella");
    stats.risultati.forEach(r => {
        const row = table.insertRow();
        row.insertCell().innerText = r.data;
        row.insertCell().innerText = r.mazzo;
        row.insertCell().innerText = r.corrette + "/" + r.totale;
        row.insertCell().innerText = r.punteggio + "%";
        row.insertCell().innerText = calcolaGiudizio(r.punteggio);
    });
}

// calcola il giudizio
function calcolaGiudizio(punteggio) {
    if (punteggio < 60) {
        return "Insufficiente 😕";
    } else if (punteggio < 70) {
        return "Sufficiente 🙂";
    } else if (punteggio < 90) {
        return "Buono 👍";
    } else {
        return "Ottimo 🏆";
    }
}

loadStats();
