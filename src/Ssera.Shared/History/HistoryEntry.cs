namespace Ssera.Shared.History;

public sealed record HistoryEntry(DateTimeOffset Timestamp, string WorkerName, string Message);
