using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace Ssera.Api.Data;

[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength",
    Justification = "Not possible in Sqlite")]
public sealed class WorkerHistory
{
    public int Id { get; set; }
    public required DateTime CreatedUtc { get; set; }
    public required int TotalEvents { get; set; }
    public required TimeSpan TimeTaken { get; set; }
    public string? Message { get; set; }

    private WorkerHistory()
    {
    }

    public static WorkerHistory CreateNew(int totalEvents, TimeSpan timeTaken, string? message)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalEvents);
        return new WorkerHistory
        {
            CreatedUtc = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
            TotalEvents = totalEvents,
            TimeTaken = timeTaken,
            Message = message
        };
    }

    private sealed class Configuration : IEntityTypeConfiguration<WorkerHistory>
    {
        public void Configure(EntityTypeBuilder<WorkerHistory> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            _ = builder.HasIndex(m => m.CreatedUtc);
        }
    }
}
