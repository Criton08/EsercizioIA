using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Web.Data;
using StudyBuddy.Web.Dtos;
using StudyBuddy.Web.Services;
using StudyBuddy.Web.Utils;

namespace StudyBuddy.Web.Controllers;

/// <summary>
/// RESTful controller exposing the quiz functionality.
/// Requires a logged-in user (fallback authorization policy) and only works on the user's own decks.
/// </summary>
[ApiController]
[Route("api/quiz")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;
    private readonly IConfiguration _config;

    public QuizController(IQuizService quizService, IConfiguration config)
    {
        _quizService = quizService;
        _config = config;
    }

    /// <summary>
    /// GET api/quiz/{deckId} - returns the questions of a deck.
    /// </summary>
    [HttpGet("{deckId}")]
    public async Task<IActionResult> GetQuestions(int deckId)
    {
        if (!await _quizService.DeckIsVisibleToUserAsync(deckId, User.GetUserId()))
            return NotFound();

        var questions =await _quizService.GetQuestionsAsync(deckId);
        return Ok(questions);
    }

    /// <summary>
    /// POST api/quiz/submit - evaluates the answers and stores the result.
    /// </summary>
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] QuizSubmission submission)
    {
        // The user comes from the auth cookie: submission.UserId is ignored.
        var userId = User.GetUserId();
        if (!await _quizService.DeckIsVisibleToUserAsync(submission.DeckId, userId))
            return NotFound();

        var outcome = await _quizService.EvaluateAsync(submission);

        // Persist the result for the statistics page.
        // Use the app's connection string: the DbHelper default points to a different DB file.
        var helper = new DbHelper(_config.GetConnectionString("Default")!);
        helper.SaveResult(userId, submission.DeckId, outcome.Percentage, outcome.Correct, outcome.Total);

        return Ok(outcome);
    }

    /// <summary>
    /// POST api/quiz/{deckId}/questions - adds a new question.
    /// </summary>
    [HttpPost("{deckId}/questions")]
    public async Task<IActionResult> AddQuestion(int deckId, [FromBody] NewQuestionRequest request)
    {
        if (!await _quizService.DeckBelongsToUserAsync(deckId, User.GetUserId()))
            return NotFound();

        // Options are stored joined by '|', so they must be non-empty and cannot contain it.
        if (string.IsNullOrWhiteSpace(request.Domanda)
            || request.Opzioni == null
            || request.Opzioni.Count < 2
            || request.Opzioni.Any(o => string.IsNullOrWhiteSpace(o) || o.Contains('|'))
            || request.RispostaCorretta < 0
            || request.RispostaCorretta >= request.Opzioni.Count)
        {
            return BadRequest("a question needs text, at least 2 non-empty options without '|', and a valid correct index");
        }

        var question = await _quizService.AddQuestionAsync(deckId, request);
        return Ok(question);
    }
}
