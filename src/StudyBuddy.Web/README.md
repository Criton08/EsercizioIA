# StudyBuddy 📚

App per studiare con le flashcard e i quiz!

## Funzionalità

- ✅ Mazzi di carte personali, divisi per materia (crea, modifica, elimina)
- ✅ Mazzi condivisi visibili a tutti gli utenti (creati dall'admin)
- ✅ Flashcard: la risposta salva la scatola Leitner della carta (non ancora usata per scegliere le carte)
- ✅ Quiz a risposta multipla, con punteggio calcolato dal server
- ✅ Ricerca nelle carte
- ✅ Statistiche dei quiz
- ✅ Login e registrazione utenti

Non ci sono: timer nel quiz, import da CSV, export in PDF.

## Come avviarlo

```
cd src/StudyBuddy.Web
dotnet run
```

Poi apri http://localhost:5200 (la porta è in `Properties/launchSettings.json`).

Utenti di prova: `mario` / `password123`, `giulia` / `giulia2008`

## Architettura

Vedi [docs/architettura.md](../../docs/architettura.md)
