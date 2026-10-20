namespace StudyBuddy.Web.Dtos;

/// <summary>A question as sent to the client (without the correct answer).</summary>
public record QuizQuestionDto(int Id, string Domanda, List<string> Opzioni);

/// <summary>A single answer given by the user.</summary>
public record AnswerDto(int QuestionId, int SelectedIndex);

/// <summary>The full set of answers submitted at the end of a quiz.</summary>
public record QuizSubmission(int UserId, int DeckId, List<AnswerDto> Answers);

/// <summary>Per-question feedback returned after evaluation.</summary>
public record AnswerDetail(int QuestionId, bool Correct, int CorrectIndex, string? Spiegazione);

/// <summary>The evaluated outcome of a quiz.</summary>
public record QuizOutcome(int Correct, int Total, int Percentage, List<AnswerDetail> Details);

/// <summary>Payload used to create a new question.</summary>
public record NewQuestionRequest(string Domanda, List<string> Opzioni, int RispostaCorretta, string? Spiegazione);

/// <summary>Generic DTO for a deck (currently unused, kept for future API versioning).</summary>
public record DeckDto(int Id, string Name, string Materia, int CardCount);
