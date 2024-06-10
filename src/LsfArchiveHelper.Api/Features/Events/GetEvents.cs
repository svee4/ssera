using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using LsfArchiveHelper.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace LsfArchiveHelper.Api.Features.Events;

[Handler]
[MapGet("/api/events")]
public sealed partial class GetEvents
{
	public sealed record Query
	{

		[Immediate.Validations.Shared.EnumValue]
		public OrderByType? OrderBy { get; init; }

		[Immediate.Validations.Shared.EnumValue]
		public SortType? Sort { get; init; }

		public EventType[]? EventTypes { get; init; }
		public string? Search { get; init; }
	}

	private static async ValueTask<ResponseModel> HandleAsync(
		Query requestQuery,
		AppDbContext dbContext,
		CancellationToken token)
	{
		var query = dbContext.Events.AsQueryable();

		if (requestQuery.EventTypes is { Length: > 0 })
		{
			// all valid api event types SHOULD be valid database event types
			var types = requestQuery.EventTypes.Where(Enum.IsDefined).Cast<Database.EventType>().ToArray();
			query = query.Where(m => types.Contains(m.Type));
		}

		if (!string.IsNullOrWhiteSpace(requestQuery.Search))
		{
			// TODO: escape the like properly - currently special characters like % are interpreted as part of the query
			// note that this does not introduce a real sql injection, just that you can fuck around with the query
			// sqlite is case-insensitive by default
			query = query.Where(m => EF.Functions.Like(m.Title, $"%{requestQuery.Search}%"));
		}

		var isDescending = requestQuery.Sort == SortType.Descending;
		query = (requestQuery.OrderBy, isDescending) switch
		{
			(OrderByType.Date, true) => query.OrderByDescending(entry => entry.DateUtc),
			(OrderByType.Date, false) => query.OrderBy(entry => entry.DateUtc),
			(OrderByType.Type, true) => query.OrderByDescending(entry => entry.Type),
			(OrderByType.Type, false) => query.OrderBy(entry => entry.Type),
			(OrderByType.Title, true) => query.OrderByDescending(entry => entry.Title),
			(OrderByType.Title, false) => query.OrderBy(entry => entry.Title),
			_ => query
		};

		var events = await query
			.Select(m => new EventModel(
				m.DateUtc,
				(EventType)m.Type,
				m.Title,
				m.Link))
			.ToListAsync(token);

		var lastUpdate = await dbContext.WorkerHistory
			.Select(entry => entry.CreatedUtc)
			.OrderByDescending(date => date)
			.FirstOrDefaultAsync(token);

		return new ResponseModel(events, lastUpdate == default ? null : DateTime.SpecifyKind(lastUpdate, DateTimeKind.Utc));
	}


	public sealed record ResponseModel(List<EventModel> Events, DateTime? LastUpdate);

	public sealed record EventModel(DateTime Date, EventType Type, string? Title, string? Link);
}
