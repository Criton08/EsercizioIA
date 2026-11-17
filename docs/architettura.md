# Architettura di StudyBuddy 🏗️

## Panoramica

```
Frontend (HTML/JS statico in wwwroot/)  →  API (minimal API + un controller MVC)  →  EF Core / SQL diretto  →  SQLite (database/studybuddy.db)
```

## Backend (`src/StudyBuddy.Web`)

- **Program.cs**: configurazione e quasi tutte le API come minimal API (login, registrazione, logout, mazzi, carte, ricerca).
- **Controllers/QuizController.cs**: l'unico controller MVC, per le API del quiz (`/api/quiz/...`).
- **Endpoints/**: gruppi di endpoint registrati da `Program.cs` con metodi di estensione (`StatsEndpoints`, `AdminEndpoints`).
- **Services/**: `QuizService` (domande e calcolo del punteggio), `DeckService` (lettura ed eliminazione dei mazzi), `SharedQuizCreator` (quiz condivisi Vero/Falso).
- **Data/**: `StudyDbContext` (EF Core), `Seed` (dati di prova), `DbHelper` (SQL diretto parametrizzato: ricerca e salvataggio dei risultati).
- **Models/** e **Dtos/**: entità del database e oggetti di richiesta/risposta.
- **Utils/**: hash delle password, lettura dell'utente dal cookie, limiti di lunghezza degli input, funzioni varie.

Non c'è un pattern Repository: gli endpoint usano direttamente `StudyDbContext` o i servizi.

## Database

- SQLite, file `database/studybuddy.db` (relativo alla cartella da cui si avvia l'app, escluso da git).
- Viene creato all'avvio con `EnsureCreated()`: **non ci sono migration**. Una modifica allo schema non si applica a un database già esistente (per la colonna `Decks.UserId` c'è una correzione manuale in `DbHelper.EnsureDeckOwnerColumn`).

## Autenticazione e permessi

- Login con username e password; la password è salvata come hash (PBKDF2, `Utils/PasswordHasher.cs`).
- Dopo il login il server imposta un **cookie** `studybuddy.auth` (HttpOnly, SameSite=Lax, durata 8 ore). Niente JWT.
- Tutte le API richiedono il login, tranne login, registrazione e logout.
- Login e registrazione hanno un limite di 10 tentativi al minuto per indirizzo IP (risposta 429).
- Ogni mazzo appartiene a un utente: si vedono solo i propri mazzi più quelli **condivisi** (`UserId` nullo). I mazzi condivisi si possono studiare e usare nel quiz, ma non modificare né eliminare.
- Le API `/api/admin/*` funzionano solo se è configurata la chiave `AdminKey` (variabile d'ambiente o user-secrets), inviata nell'header `X-Admin-Key`. Il reset del database richiede anche `?confirm=RESET`.

## Frontend (`wwwroot/`)

- Pagine HTML statiche: `index`, `login`, `study`, `quiz`, `deck`, `stats`. Nessuna build.
- Un file JS per pagina in `js/`, più `utils.js` (funzioni comuni). Solo `deck.js` usa `api.js`; le altre pagine chiamano `fetch` direttamente.
- L'utente loggato è copiato nel `localStorage` solo per mostrarne il nome: l'autorizzazione vera è il cookie.

## Quiz

- Le domande arrivano al browser **senza** la risposta corretta.
- Il punteggio è calcolato **dal server** (`QuizService.EvaluateAsync`), non dal frontend, e salvato nella tabella `Results`.

## Studio con le flashcard

- `study.js` mostra **tutte** le carte del mazzo in ordine casuale.
- "La sapevo" / "Non la sapevo" aggiornano la scatola Leitner della carta (1-5), ma per ora la scatola **non** viene usata per scegliere quali carte mostrare.
- Nei mazzi condivisi la scatola non viene salvata, perché sarebbe la stessa per tutti gli utenti.
