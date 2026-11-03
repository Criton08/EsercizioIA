namespace StudyBuddy.Web.Utils;

// miscellaneous helper functions
public static class Helpers
{
    static Random rnd = new Random();

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

    // verdict based on the score
    public static string Giudizio(int punteggio)
    {
        if (punteggio >= 90) return "Ottimo";
        else if (punteggio >= 70) return "Buono";
        else if (punteggio >= 60) return "Sufficiente";
        else return "Insufficiente";
    }

    public static bool IsNullOrEmpty(string s)
    {
        return s == null || s == "";
    }

    public static string Capitalize(string s)
    {
        if (IsNullOrEmpty(s)) return s;
        return s.Substring(0, 1).ToUpper() + s.Substring(1);
    }
}
