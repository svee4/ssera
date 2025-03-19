using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ssera.Api.Data;

public sealed class WorkerHistory
{
    public int Id { get; set; }

    public string WorkerName { get; private set; } = null!;
    public DateTime Timestamp { get; private set; }
    public string Message { get; private set; }

    private WorkerHistory() { }

    public static WorkerHistory CreateNew(
        string workerName,
        DateTime timeStamp,
        string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(workerName);

        return new WorkerHistory
        {
            WorkerName = workerName,
            Timestamp = timeStamp,
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
