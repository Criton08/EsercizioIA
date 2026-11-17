// 📊 Stats page
checkLogin();

async function loadStats() {
    // relative URL: a fixed "http://localhost:5200" broke the page on any other port or host.
    // No userId in the query: the server takes the user from the auth cookie.
    const res = await fetch("/api/stats");
    if (!res.ok) {
        document.getElementById("empty").innerText = "Impossibile caricare le statistiche. Prova ad accedere di nuovo.";
        document.getElementById("empty").style.display = "block";
        return;
    }
    const stats = await res.json();

    document.getElementById("totale").innerText = stats.totale;
    // the average is a decimal number (e.g. 33.2727...): show it rounded
    document.getElementById("media").innerText = Math.round(stats.media) + "%";

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

// grade label for a score
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
