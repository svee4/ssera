using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.EntityFrameworkCore;
using Ssera.Api.Data;
using System.Globalization;

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
    ) =>
        await dbContext.WorkerHistory
            .OrderByDescending(m => m.CreatedUtc)
            .Take(50)
            .Select(m => new HistoryModel(
                m.CreatedUtc,
                m.TotalEvents,
                m.TimeTaken.ToString("hh':'mm':'ss", CultureInfo.InvariantCulture),
                m.Message))
            .ToListAsync(token);

    // TODO figure out home timespans work in javascript
    public sealed record HistoryModel(DateTime Date, int TotalEvents, string TimeTaken, string? Message);
}
