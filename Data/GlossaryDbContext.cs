using Microsoft.EntityFrameworkCore;

namespace GlossaryAgent.Data;

public class GlossaryDbContext(DbContextOptions<GlossaryDbContext> options) : DbContext(options)
{
    public DbSet<GlossaryTerm> Terms => Set<GlossaryTerm>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GlossaryTerm>(e =>
        {
            e.ToTable("Glossary");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.NormalizedName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Definition).HasMaxLength(2000).IsRequired();

            // имя термина должно быть уникальным
            e.HasIndex(x => x.NormalizedName).IsUnique();
        });
    }
}
