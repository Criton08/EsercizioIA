using StudyBuddy.Web.Data;

namespace StudyBuddy.Web.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdmin(this WebApplication app)
    {
        // protetto: accessibile solo agli amministratori
        app.MapGet("/api/admin/users", (StudyDbContext db) => db.Users.ToList());

        app.MapPost("/api/admin/reset", (string key, StudyDbContext db, IConfiguration cfg) =>
        {
            if (key != cfg["AdminKey"]) return Results.Unauthorized();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            Seed.Init(db);
            return Results.Ok("reset fatto");
        });
    }
}
