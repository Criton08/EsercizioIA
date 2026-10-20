using StudyBuddy.Web.Dtos;
using StudyBuddy.Web.Models;

namespace StudyBuddy.Web.Services;

/// <summary>
/// Abstraction over the quiz business logic, to allow for testability and separation of concerns.
/// </summary>
public interface IQuizService
{
    /// <summary>Returns the questions of a deck in random order.</summary>
    Task<List<QuizQuestionDto>> GetQuestionsAsync(int deckId);

    /// <summary>Evaluates a submission and returns the outcome.</summary>
    Task<QuizOutcome> EvaluateAsync(QuizSubmission submission);

    /// <summary>Adds a new question to a deck.</summary>
    Task<QuizQuestion> AddQuestionAsync(int deckId, NewQuestionRequest request);
}
