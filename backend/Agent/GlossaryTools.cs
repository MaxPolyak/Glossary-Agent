using System.ComponentModel;
using GlossaryAgent.Data;
using Microsoft.EntityFrameworkCore;

namespace GlossaryAgent.Agent;

public class GlossaryTools(IDbContextFactory<GlossaryDbContext> dbFactory, ILogger<GlossaryTools> logger)
{
    private const int MaxNameLength = 200;
    private const int MaxDefinitionLength = 2000;

    [Description("Проверяет, есть ли термин в глоссарии, по его имени (регистр не важен). " +
                 "Ничего не изменяет. Возвращает 'FOUND: <имя> - <определение>', если термин есть, " +
                 "или 'NOT_FOUND', если его нет.")]
    public async Task<string> FindTerm(
        [Description("Имя термина или сокращение, например 'СЭД' или 'Контрагент'.")] string name)
    {
        var key = TermNormalizer.Normalize(name);
        if (key.Length == 0)
            return Log("FindTerm", name, "ERROR: name is empty.");

        await using var db = await dbFactory.CreateDbContextAsync();
        var term = await db.Terms.AsNoTracking().FirstOrDefaultAsync(t => t.NormalizedName == key);

        return Log("FindTerm", name, term is null
            ? "NOT_FOUND"
            : $"FOUND: {term.Name} - {term.Definition}");
    }

    [Description("Сохраняет термин в глоссарий. Если термина с таким именем ещё нет - создаёт запись " +
                 "и возвращает 'CREATED'. Если такой термин уже есть - ничего не меняет и возвращает " +
                 "'SKIPPED'. При неверных аргументах возвращает 'ERROR: <причина>' - исправь аргументы " +
                 "и попробуй ещё раз.")]
    public async Task<string> SaveTerm(
        [Description("Имя термина или сокращение: короткое, как в тексте, без кавычек. " +
                     "Для сокращения - само сокращение ('СЭД'), а не расшифровка.")] string name,
        [Description("Определение или расшифровка, взятые из текста документа. " +
                     "Для сокращения - его расшифровка ('система электронного документооборота').")] string definition)
    {
        var display = TermNormalizer.Clean(name);
        var key = TermNormalizer.Normalize(name);
        var def = (definition ?? "").Trim();

        if (key.Length == 0)
            return Log("SaveTerm", name, "ERROR: name is empty.");
        if (def.Length == 0)
            return Log("SaveTerm", name, "ERROR: definition is empty.");
        if (display.Length > MaxNameLength)
            return Log("SaveTerm", name, $"ERROR: name is too long (max {MaxNameLength}). A term name must be short.");
        if (def.Length > MaxDefinitionLength)
            return Log("SaveTerm", name, $"ERROR: definition is too long (max {MaxDefinitionLength}). Shorten it.");

        await using var db = await dbFactory.CreateDbContextAsync();

        if (await db.Terms.AnyAsync(t => t.NormalizedName == key))
            return Log("SaveTerm", name, "SKIPPED");

        db.Terms.Add(new GlossaryTerm { Name = display, NormalizedName = key, Definition = def });

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // термин могли добавить параллельно
            db.ChangeTracker.Clear();
            if (await db.Terms.AnyAsync(t => t.NormalizedName == key))
                return Log("SaveTerm", name, "SKIPPED");
            throw;
        }

        return Log("SaveTerm", name, "CREATED");
    }

    private string Log(string tool, string? arg, string result)
    {
        logger.LogInformation("[tool] {Tool}({Arg}) -> {Result}", tool, arg, result);
        return result;
    }
}
