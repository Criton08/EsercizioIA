namespace StudyBuddy.Web.Models;

public class Deck
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Descrizione { get; set; }
    public string Materia { get; set; } = "Altro";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<Card> Cards { get; set; } = new();
}
