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
    ) => await dbContext.WorkerHistory
            .OrderByDescending(m => m.Timestamp)
            .Take(50)
            .Select(m => new HistoryModel(m.Timestamp, m.WorkerName, m.Message))
            .ToListAsync(token);

    // TODO figure out home timespans work in javascript
    public sealed record HistoryModel(DateTime Timestamp, string WorkerName, string Message);
}
