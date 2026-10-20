using Microsoft.EntityFrameworkCore;
using StudyBuddy.Web.Data;
using StudyBuddy.Web.Models;

namespace StudyBuddy.Web.Services;

// ✅ REFACTORING: deck logic moved out of Program.cs into a dedicated service 🚀
// 👉 Program.cs should now call this service for everything deck-related
public class DeckService
{
    private readonly StudyDbContext db;

    public DeckService(StudyDbContext db)
    {
        this.db = db;
    }

    // 📦 Get all decks with card and question counts
    public List<object> GetAllDecks()
    {
        var decks = db.Decks.ToList();
        var result = new List<object>();

        foreach (var d in decks)
        {
            var cardCount = db.Cards.Count(c => c.DeckId == d.Id);
            var questionCount = db.Questions.Count(q => q.DeckId == d.Id);
            result.Add(new { d.Id, d.Name, d.Materia, d.Descrizione, numCarte = cardCount, numDomande = questionCount });
        }

        return result;
    }

    // ottiene i mazzi (versione semplice)
    public List<object> OttieniMazzi()
    {
        var lista = new List<object>();
        foreach (var mazzo in db.Decks.ToList())
        {
            // conta le carte del mazzo
            var numero = db.Cards.Where(c => c.DeckId == mazzo.Id).Count();
            lista.Add(new { mazzo.Id, mazzo.Name, mazzo.Materia, numCarte = numero });
        }
        return lista;
    }

    // 🔍 Get a single deck with its cards
    public Deck? GetDeck(int id)
    {
        return db.Decks.Include(d => d.Cards).FirstOrDefault(d => d.Id == id);
    }

    // 🗑️ Delete a deck and everything inside it (cards + questions)
    public bool DeleteDeck(int id)
    {
        var deck = db.Decks.Find(id);
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
