using GlossaryAgent.Agent;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;

namespace GlossaryAgent.Services;

public class ExtractionService(AIAgent agent, IOptions<LlmOptions> options, ILogger<ExtractionService> logger)
{
    public async Task ExtractAsync(string? text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Поле 'text' пустое: нужен текст документа.");

        var chunks = TextChunker.Split(text, options.Value.MaxChunkChars);
        logger.LogInformation("Документ: {Chars} символов, кусков: {Count}", text.Length, chunks.Count);

        for (var i = 0; i < chunks.Count; i++)
        {
            // оборачиваем текст в теги, чтобы модель не путала его с инструкциями
            var message = $"<document>\n{chunks[i]}\n</document>";

            var response = await agent.RunAsync(message, cancellationToken: ct);

            logger.LogInformation("Кусок {I}/{N}, итог агента: {Answer}", i + 1, chunks.Count, response.Text);
        }
    }
}
