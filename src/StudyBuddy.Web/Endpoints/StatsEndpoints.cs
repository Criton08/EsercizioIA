using StudyBuddy.Web.Data;

namespace StudyBuddy.Web.Endpoints;

// 📊 Statistics endpoints - grouped in an extension method to keep Program.cs clean ✨
public static class StatsEndpoints
{
    public static void MapStatsEndpoints(this WebApplication app)
    {
        // 📈 All the quiz results of a user, newest first
        app.MapGet("/api/stats", (int userId, StudyDbContext db) =>
        {
            var results = db.Results
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Data)
                .ToList();

            var decks = db.Decks.ToList();

            var list = results.Select(r => new
            {
                r.Id,
                mazzo = decks.FirstOrDefault(d => d.Id == r.DeckId)?.Name ?? "?",
                r.Punteggio,
                r.Corrette,
                r.Totale,
                r.Data
            }).ToList();

            var media = results.Count == 0 ? 0 : results.Average(r => r.Punteggio);

            return new { totale = results.Count, media, risultati = list };
        });
    }
}
