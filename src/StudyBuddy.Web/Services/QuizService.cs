using Microsoft.EntityFrameworkCore;
using StudyBuddy.Web.Data;
using StudyBuddy.Web.Dtos;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Utils;

namespace StudyBuddy.Web.Services;

/// <summary>
/// Default implementation of <see cref="IQuizService"/>.
/// Encapsulates all the quiz-related business rules in a single, cohesive unit.
/// </summary>
public class QuizService : IQuizService
{
    private readonly StudyDbContext _context;
    private readonly ILogger<QuizService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuizService"/> class.
    /// </summary>
    public QuizService(StudyDbContext context, ILogger<QuizService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<bool> DeckBelongsToUserAsync(int deckId, int userId)
    {
        return _context.Decks.AnyAsync(d => d.Id == deckId && d.UserId == userId);
    }

    /// <inheritdoc />
    public Task<bool> DeckIsVisibleToUserAsync(int deckId, int userId)
    {
        return _context.Decks.AnyAsync(d => d.Id == deckId && (d.UserId == userId || d.UserId == null));
    }

    /// <inheritdoc />
    public async Task<List<QuizQuestionDto>> GetQuestionsAsync(int deckId)
    {
        _logger.LogInformation("Loading questions for deck {DeckId}", deckId);

        var questions = await _context.Questions
            .Where(q => q.DeckId == deckId)
            .ToListAsync();

        // Randomize the order so that every quiz session feels different.
        return Helpers.Shuffle(questions)
            .Select(q => new QuizQuestionDto(q.Id, q.Domanda, q.Opzioni.Split('|').ToList()))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<QuizOutcome> EvaluateAsync(QuizSubmission submission)
    {
        int correct = 0;
        var details = new List<AnswerDetail>();

        // Keep one answer per question, so repeating a QuestionId cannot inflate the score.
        var answers = submission.Answers.GroupBy(a => a.QuestionId).Select(g => g.First()).ToList();

        // Load all the needed questions with a single query instead of one per answer.
        var ids = answers.Select(a => a.QuestionId).ToList();
        var questions = await _context.Questions
            .Where(q => q.DeckId == submission.DeckId && ids.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);

        foreach (var answer in answers)
        {
            // Compare the selected option with the correct one.
            if (!questions.TryGetValue(answer.QuestionId, out var question))
            {
                _logger.LogWarning("Question {Id} not found", answer.QuestionId);
                continue;
            }

            bool isCorrect = answer.SelectedIndex == question.RispostaCorretta;
            if (isCorrect)
            {
                correct++; // count the correct answers
            }

            details.Add(new AnswerDetail(question.Id, isCorrect, question.RispostaCorretta, question.Spiegazione));
        }

        // Unknown questions are skipped, so they are not counted in the total either.
        int total = details.Count;

        // Calculate the percentage score (handles total == 0).
        int percentage = Helpers.CalcolaPercentuale(correct, total);

        return new QuizOutcome(correct, total, percentage, details);
    }

    /// <inheritdoc />
    public async Task<QuizQuestion> AddQuestionAsync(int deckId, NewQuestionRequest request)
    {
        var question = new QuizQuestion
        {
            DeckId = deckId,
            Domanda = request.Domanda,
            Opzioni = string.Join("|", request.Opzioni),
            RispostaCorretta = request.RispostaCorretta,
            Spiegazione = request.Spiegazione
        };

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        return question;
    }
}
