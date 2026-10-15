using System.ClientModel;
using GlossaryAgent.Agent;
using GlossaryAgent.Contracts;
using GlossaryAgent.Data;
using GlossaryAgent.Services;
using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

// настройки и база
builder.Services.Configure<LlmOptions>(builder.Configuration.GetSection("Llm"));

builder.Services.AddDbContextFactory<GlossaryDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Glossary")));

// инструменты и агент
builder.Services.AddSingleton<GlossaryTools>();

builder.Services.AddSingleton<AIAgent>(sp =>
{
    var llm = sp.GetRequiredService<IOptions<LlmOptions>>().Value;
    if (string.IsNullOrWhiteSpace(llm.Model))
        throw new InvalidOperationException(
            "Не задана модель. Укажите Llm:Model в appsettings.json (или переменную окружения Llm__Model).");

    var tools = sp.GetRequiredService<GlossaryTools>();

    // клиент для LM Studio
    var openAi = new OpenAIClient(
        new ApiKeyCredential(llm.ApiKey),
        new OpenAIClientOptions
        {
            Endpoint = new Uri(llm.BaseUrl),
            NetworkTimeout = TimeSpan.FromMinutes(5)
        });

    // температура 0, чтобы ответы были стабильнее
    IChatClient chatClient = openAi.GetChatClient(llm.Model)
        .AsIChatClient()
        .AsBuilder()
        .ConfigureOptions(o => o.Temperature = 0f)
        .Build();

    return chatClient.AsAIAgent(
        instructions: AgentInstructions.Text,
        name: "GlossaryExtractionAgent",
        tools:
        [
            AIFunctionFactory.Create(tools.FindTerm, name: "FindTerm"),
            AIFunctionFactory.Create(tools.SaveTerm, name: "SaveTerm")
        ]);
});

builder.Services.AddSingleton<ExtractionService>();

var app = builder.Build();

// создаём базу, если её нет
await using (var db = await app.Services.GetRequiredService<IDbContextFactory<GlossaryDbContext>>().CreateDbContextAsync())
{
    await db.Database.EnsureCreatedAsync();
}

// API

// основной метод из ТЗ
app.MapPost("/api/extract", async (ExtractRequest request, ExtractionService service, CancellationToken ct) =>
{
    try
    {
        await service.ExtractAsync(request.Text, ct);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Ошибка при обработке /api/extract");
        return Results.Text(ex.Message, "text/plain; charset=utf-8", statusCode: StatusCodes.Status500InternalServerError);
    }
});

// посмотреть, что сейчас в глоссарии
app.MapGet("/api/glossary", async (IDbContextFactory<GlossaryDbContext> dbFactory) =>
{
    await using var db = await dbFactory.CreateDbContextAsync();
    var terms = await db.Terms.AsNoTracking()
        .OrderBy(t => t.Name)
        .Select(t => new { t.Name, t.Definition })
        .ToListAsync();
    return Results.Ok(terms);
});

app.Run();
