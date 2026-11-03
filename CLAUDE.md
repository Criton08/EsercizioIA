# CLAUDE.md

## Stack

- .NET 10 (`net10.0`), ASP.NET Core minimal APIs plus one MVC controller
- SQLite through EF Core (`EnsureCreated`, no migrations) and a raw-SQL helper (`DbHelper`)
- Static frontend in `wwwroot/` (plain HTML and JS, no build step, no bundler)

## Setup and run

```
cd src/StudyBuddy.Web
dotnet run
```

- Open `http://localhost:5200` (port comes from `Properties/launchSettings.json`).
- The DB is created and seeded on startup at `database/studybuddy.db` (relative to the working directory, gitignored).
- Demo users: `mario` / `password123`, `giulia` / `giulia2008`.
- Admin endpoints (`/api/admin/*`) are disabled unless `AdminKey` is configured (env var `AdminKey` or `dotnet user-secrets set AdminKey <value>`). Send it in the `X-Admin-Key` header.
- The dev container (`.devcontainer/devcontainer.json`) runs `dotnet restore` only; it does not start the app.

## Code layout (src/StudyBuddy.Web)

- `Program.cs`: most API routes (login, register, decks, cards, search); wires up the controller and endpoint groups.
- `Controllers/QuizController.cs`, `Services/QuizService.cs`, `Services/IQuizService.cs`: quiz API and scoring.
- `Endpoints/StatsEndpoints.cs`, `Endpoints/AdminEndpoints.cs`: extension methods mapped from `Program.cs`.
- `Data/`: `StudyDbContext`, `Seed` (demo data), `DbHelper` (raw SQL: search and saving quiz results).
- `Models/`, `Dtos/`: entities and request/response records.
- `wwwroot/`: pages (`index`, `login`, `study`, `quiz`, `deck`, `stats`) and `js/` (one script per page, plus `utils.js` and `api.js`).

Read in this order: `Program.cs`, `QuizService.cs`, `QuizController.cs`, `DbHelper.cs`; then `Seed.cs`, `StudyDbContext.cs`, `Models/*`, `Endpoints/*`; then the `wwwroot` pages and scripts; then config.

## Rules for agents

- Make the narrowest change the task asks for. No unrelated refactors or cleanups.
- Reproduce or confirm an issue in the real code before changing behavior.
- Match the style of the file you edit; use and normalize the style on english.
- Never add hardcoded secrets. Use configuration and parameterized queries.
- Do not commit `database/` or any `studybuddy.db` file.
- Explain every change. If the reason cannot be explained, the change is not done.
