using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Ssera.Shared.Events.Filters;


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
