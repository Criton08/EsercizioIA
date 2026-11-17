/**
 * QuizApp - handles the whole multiple choice quiz flow:
 * loading the questions, collecting the answers and showing the results.
 */
class QuizApp {
    /**
     * @param {string} deckId - the id of the deck to play
     * @param {object} user - the logged user
     */
    constructor(deckId, user) {
        this.deckId = deckId;
        this.user = user;
        this.questions = [];
        this.current = 0;
        this.answers = [];
        this.container = document.getElementById("quiz");
    }

    /**
     * Loads the questions from the API and renders the first one.
     */
    async init() {
        const response = await fetch(`/api/quiz/${this.deckId}`);
        if (!response.ok) {
            this.container.innerHTML = `<p class="center">Impossibile caricare il quiz.</p><p class="center"><a class="btn" href="index.html">Torna ai mazzi</a></p>`;
            return;
        }
        this.questions = await response.json();
        // an empty quiz must not be submitted (it would be saved as a 0/0 result)
        if (this.questions.length == 0) {
            this.container.innerHTML = `<p class="center">Questo mazzo non ha domande. Aggiungile da "Modifica".</p><p class="center"><a class="btn" href="index.html">Torna ai mazzi</a></p>`;
            return;
        }
        this.render();
    }

    /**
     * Renders the current question.
     */
    render() {
        if (this.current >= this.questions.length) {
            this.finish();
            return;
        }

        const q = this.questions[this.current];
        let html = `<div class="progress">Domanda ${this.current + 1} di ${this.questions.length}</div>`;
        html += `<div class="question">${escapeHtml(q.domanda)}</div>`;

        q.opzioni.forEach((option, i) => {
            html += `<button class="option" data-index="${i}">${escapeHtml(option)}</button>`;
        });

        this.container.innerHTML = html;

        this.container.querySelectorAll(".option").forEach(btn => {
            btn.addEventListener("click", () => this.select(parseInt(btn.dataset.index)));
        });
    }

    /**
     * Stores the selected answer and moves to the next question.
     * @param {number} index - the selected option index
     */
    select(index) {
        // the quiz is already over (e.g. a double click on the last answer while it is being sent)
        if (this.current >= this.questions.length) return;
        this.answers.push({
            questionId: this.questions[this.current].id,
            selectedIndex: index
        });
        this.current++;
        this.render();
    }

    /**
     * Submits the answers to the server and shows the outcome.
     */
    async finish() {
        // remove the last question's buttons while the answers are being sent
        this.container.innerHTML = `<p class="center">Invio delle risposte...</p>`;
        const response = await fetch("/api/quiz/submit", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                userId: this.user.id,
                deckId: parseInt(this.deckId),
                answers: this.answers
            })
        });

        const outcome = await response.json();

        let html = `<h2 class="center">Quiz completato!</h2>`;
        html += `<div class="score">${outcome.percentage}%</div>`;
        html += `<p class="center"><b>${giudizio(outcome.percentage)}</b></p>`;
        html += `<p class="center">Risposte corrette: ${outcome.correct} su ${outcome.total}</p>`;

        outcome.details.forEach(d => {
            const q = this.questions.find(x => x.id === d.questionId);
            // right or wrong answer
            if (d.correct == true) {
                html += `<div class="detail ok">`;
                html += `✅ ${escapeHtml(q.domanda)}<br>`;
            } else {
                html += `<div class="detail ko">`;
                html += `❌ ${escapeHtml(q.domanda)}<br>`;
            }
            html += `<small>Risposta corretta: <b>${escapeHtml(q.opzioni[d.correctIndex])}</b></small>`;
            if (d.spiegazione) {
                html += `<br><small><i>${escapeHtml(d.spiegazione)}</i></small>`;
            }
            html += `</div>`;
        });

        html += `<p class="center"><a class="btn" href="index.html">Torna ai mazzi</a></p>`;
        this.container.innerHTML = html;
    }
}

/**
 * Returns the grade label for a score.
 * @param {number} p - the percentage
 */
function giudizio(p) {
    if (p >= 90) return "Ottimo 🏆";
    if (p >= 70) return "Buono 👍";
    if (p >= 60) return "Sufficiente 🙂";
    return "Insufficiente 😕";
}

checkLogin();
const app = new QuizApp(getParam("deck"), getUser());
app.init();
