using System.Text;

namespace GlossaryAgent.Agent;

public static class TextChunker
{
    public static IReadOnlyList<string> Split(string text, int maxChars)
    {
        if (maxChars < 200) maxChars = 200;

        var chunks = new List<string>();
        var current = new StringBuilder();

        void Flush()
        {
            if (current.Length == 0) return;
            chunks.Add(current.ToString());
            current.Clear();
        }

        var paragraphs = text.Replace("\r\n", "\n")
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var p in paragraphs)
        {
            var paragraph = p;

            // слишком длинный абзац режем по лимиту
            while (paragraph.Length > maxChars)
            {
                Flush();
                chunks.Add(paragraph[..maxChars]);
                paragraph = paragraph[maxChars..];
            }

            if (current.Length > 0 && current.Length + paragraph.Length + 2 > maxChars)
                Flush();

            if (current.Length > 0) current.Append("\n\n");
            current.Append(paragraph);
        }

        Flush();
        return chunks;
    }
}
