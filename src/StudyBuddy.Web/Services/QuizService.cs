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

        foreach (var answer in submission.Answers)
        {
            // Retrieve the question to compare the selected option with the correct one.
            var question = await _context.Questions.FindAsync(answer.QuestionId);
            if (question == null)
            {
                _logger.LogWarning("Question {Id} not found", answer.QuestionId);
                continue;
            }

            bool isCorrect = answer.SelectedIndex == question.RispostaCorretta;
            if (isCorrect == true)
            {
                correct++; // incrementa il numero di risposte corrette
            }

            details.Add(new AnswerDetail(question.Id, isCorrect, question.RispostaCorretta, question.Spiegazione));
        }

        int total = submission.Answers.Count;

        // Calculate the percentage score.
        int percentage = correct / total * 100;

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
