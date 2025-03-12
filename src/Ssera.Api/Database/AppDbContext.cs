using Microsoft.EntityFrameworkCore;

namespace Ssera.Api.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events { get; set; }
    public DbSet<WorkerHistory> WorkerHistory { get; set; }
}
