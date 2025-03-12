using Microsoft.EntityFrameworkCore;

namespace Ssera.Api.Data;

public class ApiDbContext(DbContextOptions<ApiDbContext> options) : DbContext(options)
{
    public DbSet<EventSheetEvent> EventSheetEvents { get; set; }
    public DbSet<WorkerHistory> WorkerHistory { get; set; }
}
