namespace Movies.Infrastructure.Persistence;

/// <summary>Builds SQL LIKE patterns from user input.</summary>
internal static class LikePattern
{
    public const string Escape = "\\";

    /// <summary>
    /// "%text%", with LIKE's wildcards in <paramref name="text"/> escaped. Without this, searching for
    /// "100%" would match every title instead of finding "100% Wolf".
    /// </summary>
    public static string Contains(string text) =>
        "%" + text.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
}
