using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace LsfArchiveHelper.Api.Features.Events;

partial class GetEvents
{
	// sync everything with frontend
	
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

}
