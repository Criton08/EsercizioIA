using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Web.Data;
using StudyBuddy.Web.Dtos;
using StudyBuddy.Web.Services;

namespace StudyBuddy.Web.Controllers;

/// <summary>
/// RESTful controller exposing the quiz functionality.
/// </summary>
[ApiController]
[Route("api/quiz")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    /// <summary>
    /// GET api/quiz/{deckId} - returns the questions of a deck.
    /// </summary>
    [HttpGet("{deckId}")]
    public async Task<IActionResult> GetQuestions(int deckId)
    {
        var questions = await _quizService.GetQuestionsAsync(deckId);
        return Ok(questions);
    }

    /// <summary>
    /// GET api/quiz/{deckId}/all - returns the raw questions including the answers.
    /// Useful for debugging.
    /// </summary>
    [HttpGet("{deckId}/all")]
    public IActionResult GetAll(int deckId)
    {
        var db = HttpContext.RequestServices.GetRequiredService<StudyDbContext>();
        return Ok(db.Questions.Where(q => q.DeckId == deckId).ToList());
    }

    /// <summary>
    /// POST api/quiz/submit - evaluates the answers and stores the result.
    /// </summary>
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] QuizSubmission submission)
    {
        var outcome = await _quizService.EvaluateAsync(submission);

        // Persist the result for the statistics page.
        var helper = new DbHelper();
        helper.SaveResult(submission.UserId, submission.DeckId, outcome.Percentage, outcome.Correct, outcome.Total);

        return Ok(outcome);
    }

    /// <summary>
    /// POST api/quiz/{deckId}/questions - adds a new question.
    /// </summary>
    [HttpPost("{deckId}/questions")]
    public async Task<IActionResult> AddQuestion(int deckId, [FromBody] NewQuestionRequest request)
    {
        var question = await _quizService.AddQuestionAsync(deckId, request);
        return Ok(question);
    }
}
