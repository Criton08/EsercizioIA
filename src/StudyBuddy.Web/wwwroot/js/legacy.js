// vecchie funzioni, non piu usate (forse)

function caricaMazzi() {
    var xhr = new XMLHttpRequest();
    xhr.open("GET", "/api/mazzi");
    xhr.onload = function () {
        var mazzi = JSON.parse(xhr.responseText);
        var html = "";
        for (var i = 0; i < mazzi.length; i++) {
            html += "<li>" + mazzi[i].nome + "</li>";
        }
        document.getElementById("lista").innerHTML = html;
    };
    xhr.send();
}

function calcolaVoto(giuste, totali) {
    return Math.round(giuste / totali * 10);
}

function timer(secondi, callback) {
    var t = setInterval(function () {
        secondi--;
        document.getElementById("timer").innerText = secondi;
        if (secondi == 0) {
            clearInterval(t);
            callback();
        }
    }, 1000);
}
