using System.Text.Json.Serialization;

namespace StudyBuddy.Web.Models;

public class Card
{
    public int Id { get; set; }
    public int? DeckId { get; set; }

    [JsonIgnore]
    public Deck? Deck { get; set; }

    public string Front { get; set; } = "";
    public string Back { get; set; } = "";

    // scatola leitner (1-5)
    public int Box { get; set; } = 1;
    public DateTime? LastReview { get; set; }
}
