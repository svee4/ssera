using System.Diagnostics.CodeAnalysis;

namespace LsfArchiveHelper.Api.Database;

// sync with Features.Events.GetEvents.EventType
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

public static class EventTypeExtensions
{
	
	/// <summary>
	/// 
	/// </summary>
	/// <param name="this"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public static string AsString(this EventType @this) => @this switch
	{
		EventType.TeasersMV => "Teasers/MV",
		EventType.Performance => "Performance",
		EventType.MusicShows => "Music shows",
		EventType.BehindTheScenes => "Behind the scenes",
		EventType.Interview => "Interview",
		EventType.Variety => "Variety",
		EventType.Reality => "Reality",
		EventType.CF => "CF",
		EventType.Misc => "Misc",
		EventType.MubankPresident => "Mubank President",
		EventType.WeverseLive => "Weverse live",
		_ => throw new ArgumentOutOfRangeException(nameof(@this), @this, null)
	};

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
