namespace StudyBuddy.Web.Utils;

// Maximum lengths accepted by the API, so a client cannot store huge texts
// (or make the server hash a huge password on every login attempt).
public static class InputLimits
{
    public const int UsernameMin = 3;
    public const int UsernameMax = 50;
    public const int PasswordMin = 4;
    public const int PasswordMax = 100;

    public const int DeckName = 100;
    public const int DeckDescription = 500;
    public const int DeckSubject = 50;

    public const int CardSide = 500;

    public const int QuestionText = 500;
    public const int QuestionOption = 200;
    public const int QuestionOptionsMax = 10;
    public const int QuestionExplanation = 1000;
}
