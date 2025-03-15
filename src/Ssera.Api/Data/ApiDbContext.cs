using Microsoft.EntityFrameworkCore;

namespace Ssera.Api.Data;

public sealed class ApiDbContext(DbContextOptions<ApiDbContext> options) : DbContext(options)
{
    public DbSet<EventArchiveEntry> EventArchive { get; private set; } = null!;
    public DbSet<ImageArchiveEntry> ImageArchive { get; private set; } = null!;
    public DbSet<WorkerHistory> WorkerHistory { get; private set; } = null!;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods",
        Justification = "Its not public")]
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApiDbContext).Assembly);
    }
}
