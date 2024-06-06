using System.Diagnostics.CodeAnalysis;

namespace LsfArchiveHelper.Api.Database;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum EventType
{
	// sync with Features.Events.GetEvents.EventType
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

public static class EventTypeExtensions
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	public static EventType ParseEventType(string source) => source switch
	{
		"Teasers/MV" => EventType.TeasersMV,
		"Performance" => EventType.Performance,
		"Music Shows" => EventType.MusicShows,
		"Behind the scenes" or "Behind The Scene" => EventType.BehindTheScenes,
		"Interview" => EventType.Interview,
		"Variety" => EventType.Variety,
		"Reality" => EventType.Reality,
		"CF" => EventType.CF,
		"Misc" => EventType.Misc,
		"Mubank President" => EventType.MubankPresident,
		"Weverse live" or "Weverse Live" => EventType.WeverseLive,
		_ => throw new ArgumentException($"Cannot parse value '{source}' to {nameof(EventType)}", nameof(source))
	};
}
