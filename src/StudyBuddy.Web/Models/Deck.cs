namespace StudyBuddy.Web.Models;

public class Deck
{
    public int Id { get; set; }

    // owner of the deck; null means a shared deck: every user can see it and take its quiz,
    // but nobody can edit or delete it (shared decks are created through the admin API)
    public int? UserId { get; set; }

    public string Name { get; set; } = "";
    public string? Descrizione { get; set; }
    public string Materia { get; set; } = "Altro";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<Card> Cards { get; set; } = new();
}
