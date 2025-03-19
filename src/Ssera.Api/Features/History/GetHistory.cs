using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Ssera.Api.Data;

namespace Ssera.Api.Features.History;

[Handler]
[MapGet("/api/history")]
public sealed partial class GetHistory
{
    public sealed record Query;

    private static ValueTask<List<HistoryModel>> HandleAsync(
        Query _,
        ApiDbContext dbContext,
        CancellationToken token
    ) => throw new NotImplementedException();
    //await dbContext.WorkerHistory
    //    .OrderByDescending(m => m.Timestamp)
    //    .Take(50)
    //    .Select(m => new HistoryModel(
    //        m.CreatedUtc,
    //        m.TotalEvents,
    //        m.TimeTaken.ToString("hh':'mm':'ss", CultureInfo.InvariantCulture),
    //        m.Message))
    //    .ToListAsync(token);

    // TODO figure out home timespans work in javascript
    public sealed record HistoryModel(DateTime Date, int TotalEvents, string TimeTaken, string? Message);
}
