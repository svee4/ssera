using Microsoft.EntityFrameworkCore;

namespace LsfArchiveHelper.Api.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events { get; set; }
    public DbSet<WorkerHistory> WorkerHistory { get; set; }
}
