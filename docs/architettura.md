# Architettura di StudyBuddy 🏗️

## Panoramica

StudyBuddy è un'applicazione web a tre livelli:

```
Frontend (HTML/JS)  →  Controller API  →  Repository  →  Database SQLite (app.db)
```

## Backend

- **Controllers/**: tutte le API sono esposte tramite controller (`DecksController`, `CardsController`, `QuizController`, `StatsController`).
- **Repositories/**: l'accesso ai dati passa sempre dal pattern Repository (`IDeckRepository`, `ICardRepository`). Non si usa mai il `DbContext` direttamente nei controller.
- **Services/**: la logica di business (punteggi, ripetizione spaziata).
- **Models/**: le entità del database.

Il database si chiama `app.db` e viene creato con le migration di Entity Framework (`dotnet ef database update`).

## Frontend

- Tutte le pagine usano `js/api.js` per chiamare le API.
- Il punteggio del quiz viene calcolato nel frontend in `quiz.js`.
- La ripetizione spaziata (Leitner) sceglie quali carte mostrare in `study.js` in base alla scatola della carta.

## Autenticazione

Il login usa un token JWT salvato nel `localStorage` e inviato in ogni chiamata nell'header `Authorization`.
