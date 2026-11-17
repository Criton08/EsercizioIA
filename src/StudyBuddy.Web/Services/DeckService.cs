using Microsoft.EntityFrameworkCore;
using StudyBuddy.Web.Data;
using StudyBuddy.Web.Models;

namespace StudyBuddy.Web.Services;

// Deck queries used by the endpoints in Program.cs (list, read and delete)
public class DeckService
{
    private readonly StudyDbContext db;

    public DeckService(StudyDbContext db)
    {
        this.db = db;
    }

    // Get the decks of a user (plus the shared ones) with card and question counts.
    // A single SQL query: the counts are subqueries, not two extra queries per deck.
    public List<object> GetAllDecks(int userId)
    {
        return db.Decks
            .Where(d => d.UserId == userId || d.UserId == null)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.Materia,
                d.Descrizione,
                shared = d.UserId == null,
                numCarte = d.Cards.Count,
                numDomande = db.Questions.Count(q => q.DeckId == d.Id)
            })
            .AsEnumerable()
            .Cast<object>()
            .ToList();
    }

    // Get a single deck of a user (or a shared one) with its cards
    public Deck? GetDeck(int id, int userId)
    {
        return db.Decks.Include(d => d.Cards).FirstOrDefault(d => d.Id == id && (d.UserId == userId || d.UserId == null));
    }

    // Delete a deck of a user and everything inside it (cards + questions)
    public bool DeleteDeck(int id, int userId)
    {
        var deck = db.Decks.FirstOrDefault(d => d.Id == id && d.UserId == userId);
        if (deck == null) return false;

        var cards = db.Cards.Where(c => c.DeckId == id).ToList();
        db.Cards.RemoveRange(cards);

        var questions = db.Questions.Where(q => q.DeckId == id).ToList();
        db.Questions.RemoveRange(questions);

        db.Decks.Remove(deck);
        db.SaveChanges();
        return true;
    }
}
