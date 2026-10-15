namespace GlossaryAgent.Data;

public class GlossaryTerm
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string NormalizedName { get; set; } = "";

    public string Definition { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
