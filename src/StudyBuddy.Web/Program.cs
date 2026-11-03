using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Web.Data;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services;
using StudyBuddy.Web.Endpoints;
using StudyBuddy.Web.Utils;

var builder = WebApplication.CreateBuilder(args);

// database
Directory.CreateDirectory("database");
builder.Services.AddDbContext<StudyDbContext>(o => o.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Register MVC controllers and application services
builder.Services.AddControllers();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<DeckService>();

// Authentication: a cookie issued at login identifies the user on every request.
// APIs answer 401/403 instead of redirecting to a login page.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = "studybuddy.auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax; // the cookie is not sent on cross-site POST/PUT/DELETE
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = 401; return Task.CompletedTask; };
        o.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = 403; return Task.CompletedTask; };
    });

// Every endpoint requires a logged-in user unless it is marked AllowAnonymous.
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

var app = builder.Build();

// create the DB if it does not exist
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StudyDbContext>();
    db.Database.EnsureCreated();
    // an existing DB created before decks had an owner needs the UserId column
    new DbHelper(app.Configuration.GetConnectionString("Default")!).EnsureDeckOwnerColumn();
    Seed.Init(db);
}

// Command line mode: "dotnet run -- add-quiz <file.json>" adds a shared quiz straight to the DB and exits
// without starting the web server (no admin key needed: whoever runs it already has access to the DB file).
if (args.Length == 2 && args[0] == "add-quiz")
{
    using var cliScope = app.Services.CreateScope();
    var cliDb = cliScope.ServiceProvider.GetRequiredService<StudyDbContext>();
    try
    {
        var json = File.ReadAllText(args[1]);
        var quiz = System.Text.Json.JsonSerializer.Deserialize<SharedQuizRequest>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var (status, message, deck) = quiz == null
            ? (SharedQuizStatus.Invalid, "empty or invalid JSON", null)
            : SharedQuizCreator.Create(cliDb, quiz);
        Console.WriteLine(status == SharedQuizStatus.Created
            ? $"OK id={deck!.Id} name=\"{deck.Name}\" domande={quiz!.Domande.Count} carte={quiz.Carte?.Count ?? 0}"
            : $"{status.ToString().ToUpper()}: {message}");
        Environment.ExitCode = status == SharedQuizStatus.Created ? 0 : status == SharedQuizStatus.Duplicate ? 2 : 1;
    }
    catch (Exception e) when (e is IOException or System.Text.Json.JsonException)
    {
        Console.WriteLine($"INVALID: {e.Message}");
        Environment.ExitCode = 1;
    }
    return;
}

app.UseDefaultFiles();
// no-cache: the browser must revalidate JS/HTML on every load (cheap 304 answers),
// otherwise it can keep running an old script after the code has been fixed
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "no-cache"
});
app.UseAuthentication();
app.UseAuthorization();

// ===================== LOGIN =====================

// login: the password is checked against the stored hash, then the auth cookie is issued
app.MapPost("/api/login", async (HttpContext ctx, StudyDbContext db, LoginRequest req) =>
{
    if (string.IsNullOrEmpty(req.Username) || string.IsNullOrEmpty(req.Password))
        return Results.Unauthorized();

    // find the user
    var u = db.Users.FirstOrDefault(x => x.Username == req.Username);
    if (u == null || !PasswordHasher.Verify(req.Password, u.Password, out var needsRehash))
        return Results.Unauthorized();

    // old plain-text password: store it as a hash on the first successful login
    if (needsRehash)
    {
        u.Password = PasswordHasher.Hash(req.Password);
        db.SaveChanges();
    }

    var identity = new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()),
        new Claim(ClaimTypes.Name, u.Username)
    }, CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

    return Results.Ok(new { u.Id, u.Username });
}).AllowAnonymous();

app.MapPost("/api/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
}).AllowAnonymous();

app.MapPost("/api/register", (StudyDbContext db, LoginRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrEmpty(req.Password))
        return Results.BadRequest("username e password obbligatori");

    if (db.Users.Any(x => x.Username == req.Username))
        return Results.BadRequest("utente gia esistente");

    // create the new user (the password is stored as a hash)
    var u = new User { Username = req.Username, Password = PasswordHasher.Hash(req.Password) };
    db.Users.Add(u);
    db.SaveChanges();
    return Results.Ok(new { u.Id, u.Username });
}).AllowAnonymous();

// ===================== DECKS =====================
// Every deck belongs to the user who created it: users only see and change their own decks.
// Someone else's deck behaves as if it did not exist (404).

// the decks of the logged-in user
app.MapGet("/api/decks", (ClaimsPrincipal user, DeckService service) => service.GetAllDecks(user.GetUserId()));

// returns the deck, or 404 if it does not exist or belongs to another user
app.MapGet("/api/decks/{id}", (int id, ClaimsPrincipal user, DeckService service) =>
{
    var d = service.GetDeck(id, user.GetUserId());
    return d == null ? Results.NotFound() : Results.Ok(d);
});

// binds a DTO, not the entity: clients cannot set Id, UserId, CreatedAt or Cards
app.MapPost("/api/decks", (ClaimsPrincipal user, StudyDbContext db, NewDeckRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Name))
        return Results.BadRequest("name is required");

    var deck = new Deck { UserId = user.GetUserId(), Name = req.Name, Descrizione = req.Descrizione };
    if (!string.IsNullOrEmpty(req.Materia)) deck.Materia = req.Materia;
    db.Decks.Add(deck);
    db.SaveChanges();
    return Results.Ok(deck);
});

// deletes the deck with its cards and questions
app.MapDelete("/api/decks/{id}", (int id, ClaimsPrincipal user, DeckService service) =>
{
    return service.DeleteDeck(id, user.GetUserId()) ? Results.Ok() : Results.NotFound();
});

// ===================== CARDS =====================

// binds a DTO, not the entity: clients cannot set Id, Box or LastReview
app.MapPost("/api/decks/{id}/cards", (int id, NewCardRequest req, ClaimsPrincipal user, StudyDbContext db) =>
{
    var userId = user.GetUserId();
    if (!db.Decks.Any(d => d.Id == id && d.UserId == userId))
        return Results.NotFound();
    if (string.IsNullOrWhiteSpace(req.Front) || string.IsNullOrWhiteSpace(req.Back))
        return Results.BadRequest("front and back are required");

    var card = new Card { DeckId = id, Front = req.Front, Back = req.Back };
    db.Cards.Add(card);
    db.SaveChanges();
    return Results.Ok(card);
});

// deletes the card, only if it belongs to a deck of the logged-in user
app.MapDelete("/api/cards/{id}", (int id, ClaimsPrincipal user, StudyDbContext db) =>
{
    var userId = user.GetUserId();
    var c = db.Cards.FirstOrDefault(x => x.Id == id && x.Deck!.UserId == userId);
    if (c == null) return Results.NotFound();
    db.Cards.Remove(c);
    db.SaveChanges();
    return Results.Ok();
});

// known -> moves up one box
app.MapPut("/api/cards/{id}/known", (int id, ClaimsPrincipal user, StudyDbContext db) =>
{
    var userId = user.GetUserId();
    var c = db.Cards.FirstOrDefault(x => x.Id == id && (x.Deck!.UserId == userId || x.Deck.UserId == null));
    if (c == null) return Results.NotFound();
    // shared cards belong to nobody: the box is not saved, or it would be shared by all users
    if (db.Decks.Any(d => d.Id == c.DeckId && d.UserId == null)) return Results.Ok(c);
    c.Box = Math.Min(c.Box + 1, 5); // max 5 boxes
    c.LastReview = DateTime.Now;
    db.SaveChanges();
    return Results.Ok(c);
});

// unknown -> back to box 1
app.MapPut("/api/cards/{id}/unknown", (int id, ClaimsPrincipal user, StudyDbContext db) =>
{
    var userId = user.GetUserId();
    var c = db.Cards.FirstOrDefault(x => x.Id == id && (x.Deck!.UserId == userId || x.Deck.UserId == null));
    if (c == null) return Results.NotFound();
    // shared cards belong to nobody: the box is not saved, or it would be shared by all users
    if (db.Decks.Any(d => d.Id == c.DeckId && d.UserId == null)) return Results.Ok(c);
    c.Box = 1; // a missed card always restarts from the first box
    c.LastReview = DateTime.Now;
    db.SaveChanges();
    return Results.Ok(c);
});

// ===================== SEARCH =====================

app.MapGet("/api/search", (string q, ClaimsPrincipal user, IConfiguration config) =>
{
    var helper = new DbHelper(config.GetConnectionString("Default")!);
    return helper.Search(q, user.GetUserId());
});

app.MapControllers();
app.MapStatsEndpoints();
app.MapAdmin();

app.Run();

record LoginRequest(string Username, string Password);
record NewDeckRequest(string? Name, string? Descrizione, string? Materia);
record NewCardRequest(string? Front, string? Back);
