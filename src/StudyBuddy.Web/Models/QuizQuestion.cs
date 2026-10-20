namespace StudyBuddy.Web.Models;

/// <summary>
/// Represents a multiple choice question belonging to a deck.
/// </summary>
public class QuizQuestion
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>The deck this question belongs to.</summary>
    public int DeckId { get; set; }

    /// <summary>The question text.</summary>
    public string Domanda { get; set; } = string.Empty;

    /// <summary>The answer options, separated by a pipe character (e.g. "A|B|C|D").</summary>
    public string Opzioni { get; set; } = string.Empty;

    /// <summary>Index of the correct option.</summary>
    public int RispostaCorretta { get; set; }

    /// <summary>Optional explanation shown after answering.</summary>
    public string? Spiegazione { get; set; }
}
