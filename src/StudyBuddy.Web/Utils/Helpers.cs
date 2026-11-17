namespace StudyBuddy.Web.Utils;

// miscellaneous helper functions
public static class Helpers
{
    // Random.Shared is thread-safe; a shared "new Random()" is not
    static Random rnd = Random.Shared;

    // shuffles a list (Fisher-Yates)
    public static List<T> Shuffle<T>(List<T> list)
    {
        var l = new List<T>(list);
        for (int i = l.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1); // j must be allowed to equal i, otherwise the shuffle is biased
            (l[i], l[j]) = (l[j], l[i]);
        }
        return l;
    }

    public static string FormatDate(DateTime d)
    {
        return d.ToString("dd/MM/yyyy HH:mm");
    }

    public static int CalcolaPercentuale(int giuste, int totali)
    {
        if (totali == 0) return 0;
        return (int)Math.Round(giuste * 100.0 / totali);
    }

}
