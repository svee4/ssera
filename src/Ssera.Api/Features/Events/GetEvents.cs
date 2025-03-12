using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Immediate.Validations.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssera.Api.Data;

namespace Ssera.Api.Features.Events;

[Handler]
[MapGet("/api/events")]
public sealed partial class GetEvents
{

    [Validate]
    public sealed partial record Query : IValidationTarget<Query>
    {
        public OrderByType? OrderBy { get; init; }
        public SortType? Sort { get; init; }

        [FromQuery]
        public EventType[]? EventTypes { get; init; }
        public string? Search { get; init; }

        [GreaterThanOrEqual(1)]
        public int Page { get; init; }

        [GreaterThanOrEqual(10), LessThanOrEqual(5000)]
        public int PageSize { get; init; }
    }

    public sealed record ResponseModel(List<EventModel> Results, DateTime? LastUpdate, int TotalResults);
    public sealed record EventModel(DateTime Date, EventType Type, string? Title, string? Link);

    private static async ValueTask<ResponseModel> HandleAsync(
        Query requestQuery,
        ApiDbContext dbContext,
        CancellationToken token)
    {
        var query = dbContext.EventSheetEvents.AsQueryable();

        if (requestQuery.EventTypes is { Length: > 0 })
        {
            // all valid api event types SHOULD be valid database event types
            var types = requestQuery.EventTypes.Where(Enum.IsDefined).Cast<EventSheetEventKind>().ToArray();
            query = query.Where(m => types.Contains(m.Type));
        }

        if (!string.IsNullOrWhiteSpace(requestQuery.Search))
        {
            query = query.Where(m => EF.Functions.Like(m.Title, $"%{requestQuery.Search}%"));
        }

        var isDescending = requestQuery.Sort == SortType.Descending;
        var orderedQuery = (requestQuery.OrderBy, isDescending) switch
        {
            (OrderByType.Date, true) => query.OrderByDescending(entry => entry.Date),
            (OrderByType.Date, false) => query.OrderBy(entry => entry.Date),
            (OrderByType.Type, true) => query.OrderByDescending(entry => entry.Type),
            (OrderByType.Type, false) => query.OrderBy(entry => entry.Type),
            (OrderByType.Title, true) => query.OrderByDescending(entry => entry.Title),
            (OrderByType.Title, false) => query.OrderBy(entry => entry.Title),
            _ => null
        };

        query = orderedQuery is not null
            ? orderedQuery.ThenByDescending(entry => entry.Id)
            : query.OrderByDescending(entry => entry.Id);

        var skips = (requestQuery.Page - 1) * requestQuery.PageSize;

        var count = await query.CountAsync(token);

        var events = skips > count
            ? []
            : await query
                .Select(m => new EventModel(
                    m.Date,
                    (EventType)m.Type,
                    m.Title,
                    m.Link))
                .Skip(skips)
                .Take(requestQuery.PageSize)
                .ToListAsync(token);

        var lastUpdate = await dbContext.WorkerHistory
            .Select(entry => entry.CreatedUtc)
            .OrderByDescending(date => date)
            .FirstOrDefaultAsync(token);

        return new ResponseModel(
            events,
            lastUpdate == default ? null : DateTime.SpecifyKind(lastUpdate, DateTimeKind.Utc),
            count
        );
    }
}
