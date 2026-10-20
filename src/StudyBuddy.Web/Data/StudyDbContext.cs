using Microsoft.EntityFrameworkCore;
using StudyBuddy.Web.Models;

namespace StudyBuddy.Web.Data;

public class StudyDbContext : DbContext
{
    public StudyDbContext(DbContextOptions<StudyDbContext> options) : base(options)
    {
    }

    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<User> Users => Set<User>();
    public DbSet<QuizQuestion> Questions => Set<QuizQuestion>();
    public DbSet<QuizResult> Results => Set<QuizResult>();
}
