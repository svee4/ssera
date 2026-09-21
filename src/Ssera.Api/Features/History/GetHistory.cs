using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.EntityFrameworkCore;
using Ssera.Api.Data;
using Ssera.Shared.History;

namespace Ssera.Api.Features.History;

[Handler]
[MapGet("/api/history")]
public static partial class GetHistory
{
    public sealed record Query;

    private static async ValueTask<List<HistoryEntry>> HandleAsync(
        Query _,
        ApiDbContext dbContext,
        CancellationToken token
    )
    {
        return await dbContext.WorkerHistory
            .OrderByDescending(m => m.Timestamp)
            .Take(50)
            .Select(m => new HistoryEntry(new DateTimeOffset(m.Timestamp), m.WorkerName, m.Message))
            .ToListAsync(token);
    }
}
