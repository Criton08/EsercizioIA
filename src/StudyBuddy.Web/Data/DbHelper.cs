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
    /// Saves a quiz result. Creates the table if it does not exist yet.
    /// </summary>
    public void SaveResult(int userId, int deckId, int punteggio, int corrette, int totale)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS Results (Id INTEGER PRIMARY KEY AUTOINCREMENT, UserId INTEGER NOT NULL, DeckId INTEGER NOT NULL, Punteggio INTEGER NOT NULL, Corrette INTEGER NOT NULL, Totale INTEGER NOT NULL, Data TEXT NOT NULL)";
        cmd.ExecuteNonQuery();

        cmd.CommandText = $"INSERT INTO Results (UserId, DeckId, Punteggio, Corrette, Totale, Data) VALUES ({userId}, {deckId}, {punteggio}, {corrette}, {totale}, '{Helpers.FormatDate(DateTime.Now)}')";
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Full text search over the cards (front and back).
    /// </summary>
    public List<Dictionary<string, object>> Search(string q)
    {
        var result = new List<Dictionary<string, object>>();

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        // query parametrizzata, sicura contro SQL injection
        cmd.CommandText = "SELECT c.Id, c.Front, c.Back, c.DeckId, d.Name FROM Cards c LEFT JOIN Decks d ON d.Id = c.DeckId WHERE c.Front LIKE '%" + q + "%' OR c.Back LIKE '%" + q + "%'";

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
