using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using LsfArchiveHelper.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace LsfArchiveHelper.Api.Features.Events;

[Handler]
[MapGet("/api/events")]
public sealed partial class GetEvents
{
	[JsonConverter(typeof(JsonStringEnumConverter<OrderByType>))]
	public enum OrderByType
	{
		Date = 1,
		Type,
		Title,
	}

	[JsonConverter(typeof(JsonStringEnumConverter<SortType>))]
	public enum SortType
	{
		Ascending = 1,
		Descending
	}

	// sync with database one
	[JsonConverter(typeof(JsonStringEnumConverter<EventType>))]
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public enum EventType
	{
		TeasersMV = 1,
		Performance,
		MusicShows,
		BehindTheScenes,
		Interview,
		Variety,
		Reality,
		CF,
		Misc,
		MubankPresident,
		WeverseLive
	}

	public sealed record Query
	{
		public OrderByType? OrderBy { get; init; }

		public SortType? Sort { get; init; }

		public EventType[]? EventTypes { get; init; }
		public string? Search { get; init; }
	}

	private static async ValueTask<List<EventModel>> HandleAsync(
		Query requestQuery,
		AppDbContext dbContext,
		CancellationToken token)
	{
		var query = dbContext.Entries.AsQueryable();

		if (requestQuery.EventTypes is not null && requestQuery.EventTypes.Length > 0)
		{
			var types = requestQuery.EventTypes.Cast<Database.EventType>().ToArray();
			query = query.Where(m => types.Contains(m.Type));
		}

		if (!string.IsNullOrWhiteSpace(requestQuery.Search))
		{
			// TODO: escape the like properly
			query = query.Where(m => EF.Functions.Like(m.Title, $"%{requestQuery.Search}%"));
		}

		var isDescending = requestQuery.Sort == SortType.Descending;

		query = (requestQuery.OrderBy, isDescending) switch
		{
			(OrderByType.Date, true) => query.OrderByDescending(entry => entry.Date),
			(OrderByType.Date, false) => query.OrderBy(entry => entry.Date),
			(OrderByType.Type, true) => query.OrderByDescending(entry => entry.Type),
			(OrderByType.Type, false) => query.OrderBy(entry => entry.Type),
			(OrderByType.Title, true) => query.OrderByDescending(entry => entry.Title),
			(OrderByType.Title, false) => query.OrderBy(entry => entry.Title),
			_ => query
		};

		return await query
			.Select(m => new EventModel(
				m.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), 
				(EventType)m.Type,
				m.Title, 
				m.Link))
			.ToListAsync(token);
	}

	// IM NOT DOING JAVASCRIPT DATES
	public sealed record EventModel(string DateUtc, EventType Type, string? Title, string? Link);
}
