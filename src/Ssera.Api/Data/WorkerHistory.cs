using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ssera.Api.Data;

public sealed class WorkerHistory
{
    public int Id { get; set; }

    public string WorkerName { get; private set; } = null!;

    /// <summary>
    /// UTC
    /// </summary>
    public DateTime Timestamp
    {
        get => DateTime.SpecifyKind(_timestamp, DateTimeKind.Utc);
        set => _timestamp = value;
    }

    private DateTime _timestamp;

    public string Message { get; private set; } = null!;

    private WorkerHistory() { }

    public static WorkerHistory CreateNew(
        string workerName,
        DateTime timeStamp,
        string message)
    {
        if (timeStamp.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("DateTime Kind must be UTC");
        }

        ArgumentException.ThrowIfNullOrEmpty(workerName);

        return new WorkerHistory
        {
            WorkerName = workerName,
            _timestamp = timeStamp,
            Message = message
        };
    }

    private sealed class Configuration : IEntityTypeConfiguration<WorkerHistory>
    {
        public void Configure(EntityTypeBuilder<WorkerHistory> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            _ = builder.HasIndex(m => m.Timestamp);
        }
    }
}
