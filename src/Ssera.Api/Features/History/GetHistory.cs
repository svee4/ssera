using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.EntityFrameworkCore;
using Ssera.Api.Data;

namespace Ssera.Api.Features.History;

[Handler]
[MapGet("/api/history")]
public sealed partial class GetHistory
{
    public sealed record Query;

    private static async ValueTask<List<HistoryModel>> HandleAsync(
        Query _,
        ApiDbContext dbContext,
        CancellationToken token
    )
    {
        return await dbContext.WorkerHistory
            .OrderByDescending(m => m.Timestamp)
            .Take(50)
            .Select(m => new HistoryModel(new DateTimeOffset(m.Timestamp), m.WorkerName, m.Message))
            .ToListAsync(token);
    }

    public sealed record HistoryModel(DateTimeOffset Timestamp, string WorkerName, string Message);
}
