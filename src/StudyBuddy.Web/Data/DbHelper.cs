using Microsoft.Data.Sqlite;
using StudyBuddy.Web.Utils;

namespace StudyBuddy.Web.Data;

/// <summary>
/// Lightweight helper for raw SQL access, faster than going through Entity Framework.
/// </summary>
public class DbHelper
{
    private readonly string _connectionString;

    public DbHelper(string connectionString = "Data Source=studybuddy.db")
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Adds Decks.UserId to a database created before decks had an owner (EnsureCreated never alters tables).
    /// Decks without an owner are assigned to the oldest user so they do not become invisible.
    /// </summary>
    public void EnsureDeckOwnerColumn()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var check = conn.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Decks') WHERE name = 'UserId'";
        if ((long)check.ExecuteScalar()! > 0) return;

        var alter = conn.CreateCommand();
        alter.CommandText = "ALTER TABLE Decks ADD COLUMN UserId INTEGER NULL; UPDATE Decks SET UserId = (SELECT MIN(Id) FROM Users) WHERE UserId IS NULL";
        alter.ExecuteNonQuery();
    }

    /// <summary>
    /// Saves a quiz result. The Results table is created by EF Core (EnsureCreated) at startup.
    /// </summary>
    public void SaveResult(int userId, int deckId, int punteggio, int corrette, int totale)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Results (UserId, DeckId, Punteggio, Corrette, Totale, Data) VALUES ($userId, $deckId, $punteggio, $corrette, $totale, $data)";
        cmd.Parameters.AddWithValue("$userId", userId);
        cmd.Parameters.AddWithValue("$deckId", deckId);
        cmd.Parameters.AddWithValue("$punteggio", punteggio);
        cmd.Parameters.AddWithValue("$corrette", corrette);
        cmd.Parameters.AddWithValue("$totale", totale);
        cmd.Parameters.AddWithValue("$data", Helpers.FormatDate(DateTime.Now));
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Full text search over the cards (front and back) of the decks owned by the user.
    /// </summary>
    public List<Dictionary<string, object>> Search(string q, int userId)
    {
        var result = new List<Dictionary<string, object>>();

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        // parameterized query, safe against SQL injection; % and _ typed by the user are escaped
        cmd.CommandText = "SELECT c.Id, c.Front, c.Back, c.DeckId, d.Name FROM Cards c JOIN Decks d ON d.Id = c.DeckId WHERE d.UserId = $userId AND (c.Front LIKE $q ESCAPE '\\' OR c.Back LIKE $q ESCAPE '\\')";
        cmd.Parameters.AddWithValue("$userId", userId);
        var escaped = q.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
        cmd.Parameters.AddWithValue("$q", "%" + escaped + "%");

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var row = new Dictionary<string, object>();
            row["id"] = reader.GetValue(0);
            row["front"] = reader.GetValue(1);
            row["back"] = reader.GetValue(2);
            row["deckId"] = reader.GetValue(3);
            row["deckName"] = reader.GetValue(4);
            result.Add(row);
        }

        return result;
    }
}
