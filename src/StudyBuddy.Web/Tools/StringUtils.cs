namespace StudyBuddy.Web.Tools;

// 🧰 String helpers
public static class StringUtils
{
    // ✂️ Truncate long text for previews
    public static string Truncate(string s, int max)
    {
        if (s.Length > max)
            return s.Substring(0, max) + "...";
        return s;
    }

    // 🔗 Make a URL-friendly slug (e.g. "La Cellula" -> "la-cellula")
    public static string Slugify(string s)
    {
        return s.ToLower().Replace(" ", "-");
    }
}
