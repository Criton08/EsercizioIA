namespace StudyBuddy.Web.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
