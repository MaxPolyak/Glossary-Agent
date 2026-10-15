namespace GlossaryAgent.Data;

public static class TermNormalizer
{
    private static readonly char[] QuoteChars = ['«', '»', '"', '\'', '“', '”', '„'];

    public static string Clean(string? name)
    {
        var s = (name ?? "").Trim().Trim(QuoteChars).Trim();
        return string.Join(' ', s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    public static string Normalize(string? name) =>
        Clean(name).Replace('ё', 'е').Replace('Ё', 'Е').ToUpperInvariant();
}
