namespace StudyBuddy.Web.Legacy;

// VECCHIA VERSIONE DEL QUIZ - NON CANCELLARE!!! potrebbe servire
// ora si usa QuizService
public static class OldQuizEngine
{
    public static int Punteggio(int[] risposte, int[] corrette)
    {
        int ok = 0;
        for (int i = 0; i < risposte.Length; i++)
        {
            if (risposte[i] == corrette[i]) ok++;
        }
        return ok * 100 / risposte.Length;
    }

    public static string Giudizio(int punteggio)
    {
        if (punteggio >= 90) return "Ottimo";
        if (punteggio >= 70) return "Buono";
        if (punteggio >= 60) return "Sufficiente";
        return "Insufficiente";
    }

    //public static List<int> Mischia(List<int> l)
    //{
    //    var r = new Random();
    //    return l.OrderBy(x => r.Next()).ToList();
    //}
}
