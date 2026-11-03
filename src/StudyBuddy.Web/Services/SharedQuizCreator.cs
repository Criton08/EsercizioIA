using StudyBuddy.Web.Data;
using StudyBuddy.Web.Models;

namespace StudyBuddy.Web.Services;

public record SharedQuestion(string Testo, bool Vera, string? Spiegazione);

// Argomento is optional: it narrows the quiz to a topic inside the subject
public record SharedQuizRequest(string Materia, string? Argomento, List<SharedQuestion> Domande);

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
        db.SaveChanges();

        return (SharedQuizStatus.Created, "ok", deck);
    }
}
