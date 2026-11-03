using StudyBuddy.Web.Data;
using StudyBuddy.Web.Models;

namespace StudyBuddy.Web.Services;

public record SharedQuestion(string Testo, bool Vera, string? Spiegazione);

// A flashcard of the shared deck: Fronte is the question or term, Retro the answer
public record SharedCard(string Fronte, string Retro);

// Argomento is optional: it narrows the quiz to a topic inside the subject.
// Carte is optional: the flashcards shown in study mode.
public record SharedQuizRequest(string Materia, string? Argomento, List<SharedQuestion> Domande, List<SharedCard>? Carte = null);

public enum SharedQuizStatus { Created, Invalid, Duplicate }

/// <summary>
/// Creates a shared true/false quiz: a deck with no owner (UserId null) visible to every user.
/// Used by the admin endpoint and by the "add-quiz" command line mode.
/// </summary>
public static class SharedQuizCreator
{
    public static (SharedQuizStatus Status, string Message, Deck? Deck) Create(StudyDbContext db, SharedQuizRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Materia))
            return (SharedQuizStatus.Invalid, "materia is required", null);
        if (req.Domande == null || req.Domande.Count == 0 || req.Domande.Count > 50
            || req.Domande.Any(d => string.IsNullOrWhiteSpace(d.Testo)))
            return (SharedQuizStatus.Invalid, "domande must contain 1-50 statements with text", null);
        if (req.Carte != null && (req.Carte.Count > 100
            || req.Carte.Any(c => string.IsNullOrWhiteSpace(c.Fronte) || string.IsNullOrWhiteSpace(c.Retro))))
            return (SharedQuizStatus.Invalid, "carte must contain at most 100 cards with fronte and retro", null);

        var materia = req.Materia.Trim();
        var argomento = string.IsNullOrWhiteSpace(req.Argomento) ? null : req.Argomento.Trim();

        // the deck name shows the topic too, so two topics of the same subject do not clash
        var name = argomento == null ? materia : $"{materia} - {argomento}";
        if (db.Decks.Any(d => d.UserId == null && d.Name == name))
            return (SharedQuizStatus.Duplicate, "a shared quiz with this name already exists", null);

        var deck = new Deck
        {
            UserId = null,
            Name = name,
            Materia = materia,
            Descrizione = argomento == null ? "Quiz Vero/Falso" : $"Quiz Vero/Falso: {argomento}"
        };
        db.Decks.Add(deck);
        db.SaveChanges();

        // true/false: options are fixed, the correct index is 0 (Vero) or 1 (Falso)
        db.Questions.AddRange(req.Domande.Select(d => new QuizQuestion
        {
            DeckId = deck.Id,
            Domanda = d.Testo.Trim(),
            Opzioni = "Vero|Falso",
            RispostaCorretta = d.Vera ? 0 : 1,
            Spiegazione = d.Spiegazione
        }));

        // flashcards for study mode
        if (req.Carte != null)
            db.Cards.AddRange(req.Carte.Select(c => new Card { DeckId = deck.Id, Front = c.Fronte.Trim(), Back = c.Retro.Trim() }));
        db.SaveChanges();

        return (SharedQuizStatus.Created, "ok", deck);
    }
}
