namespace StudyBuddy.Web.Models;

/// <summary>
/// Stores the outcome of a completed quiz.
/// </summary>
public class QuizResult
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int DeckId { get; set; }

    /// <summary>Score as a percentage (0-100).</summary>
    public int Punteggio { get; set; }

    public int Corrette { get; set; }
    public int Totale { get; set; }

    /// <summary>Date of the quiz, formatted for display.</summary>
    public string Data { get; set; } = string.Empty;
}
