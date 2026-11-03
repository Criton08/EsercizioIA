using System.Security.Cryptography;
using System.Text;
using StudyBuddy.Web.Data;
using StudyBuddy.Web.Services;

namespace StudyBuddy.Web.Endpoints;

public static class AdminEndpoints
{
    // The admin key is read from configuration (env var AdminKey or user-secrets), never from the repo.
    // It is sent in the X-Admin-Key header. If no key is configured, admin endpoints are disabled.
    private static bool IsAdmin(HttpRequest request, IConfiguration cfg)
    {
        var expected = cfg["AdminKey"];
        if (string.IsNullOrEmpty(expected)) return false;

        var provided = request.Headers["X-Admin-Key"].ToString();
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(provided),
            Encoding.UTF8.GetBytes(expected));
    }

    public static void MapAdmin(this WebApplication app)
    {
        // protected: admins only, does not expose passwords
        app.MapGet("/api/admin/users", (HttpRequest request, StudyDbContext db, IConfiguration cfg) =>
        {
            if (!IsAdmin(request, cfg)) return Results.Unauthorized();

            return Results.Ok(db.Users.Select(u => new { u.Id, u.Username, u.CreatedAt }).ToList());
        }).AllowAnonymous(); // protected by the admin key, not by the user cookie

        // creates a shared true/false quiz: a deck with no owner (UserId null) visible to every user
        app.MapPost("/api/admin/quiz", (HttpRequest request, StudyDbContext db, IConfiguration cfg, SharedQuizRequest req) =>
        {
            if (!IsAdmin(request, cfg)) return Results.Unauthorized();

            var (status, message, deck) = SharedQuizCreator.Create(db, req);
            return status switch
            {
                SharedQuizStatus.Created => Results.Ok(new { deck!.Id, deck.Name, numDomande = req.Domande.Count }),
                SharedQuizStatus.Duplicate => Results.Conflict(message),
                _ => Results.BadRequest(message)
            };
        }).AllowAnonymous(); // protected by the admin key, not by the user cookie

        app.MapPost("/api/admin/reset", (HttpRequest request, StudyDbContext db, IConfiguration cfg) =>
        {
            if (!IsAdmin(request, cfg)) return Results.Unauthorized();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            Seed.Init(db);
            return Results.Ok("reset fatto");
        }).AllowAnonymous(); // protected by the admin key, not by the user cookie
    }
}
