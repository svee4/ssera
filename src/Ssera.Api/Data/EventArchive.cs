using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Ssera.Api.Data;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum EventArchiveEventKind
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

public static class EventArchive
{

    public static string AsHuman(this EventArchiveEventKind eventType) =>
        Names.EnumToHuman.TryGetValue(eventType, out var result)
        ? result
        : throw new ArgumentException($"Invalid {nameof(EventArchiveEventKind)}: {eventType}");

    public static class Names
    {
        public static FrozenDictionary<string, EventArchiveEventKind> HumanToEnum { get; } = new Dictionary<string, EventArchiveEventKind>
        {
            { "Teasers/MV", EventArchiveEventKind.TeasersMV },
            { "Performance", EventArchiveEventKind.Performance },
            { "Music Shows", EventArchiveEventKind.MusicShows },
            { "Behind The Scene", EventArchiveEventKind.BehindTheScenes },
            { "Interview", EventArchiveEventKind.Interview },
            { "Variety", EventArchiveEventKind.Variety },
            { "Reality", EventArchiveEventKind.Reality },
            { "CF", EventArchiveEventKind.CF },
            { "Misc", EventArchiveEventKind.Misc },
            { "Mubank President", EventArchiveEventKind.MubankPresident },
            { "Weverse Live", EventArchiveEventKind.WeverseLive },
        }.ToFrozenDictionary();

        public static FrozenDictionary<EventArchiveEventKind, string> EnumToHuman { get; } = new Dictionary<EventArchiveEventKind, string>
        {
            { EventArchiveEventKind.TeasersMV, "Teasers/MV" },
            { EventArchiveEventKind.Performance, "Performance" },
            { EventArchiveEventKind.MusicShows, "Music Shows" },
            { EventArchiveEventKind.BehindTheScenes, "Behind The Scene" },
            { EventArchiveEventKind.Interview, "Interview" },
            { EventArchiveEventKind.Variety, "Variety" },
            { EventArchiveEventKind.Reality, "Reality" },
            { EventArchiveEventKind.CF, "CF" },
            { EventArchiveEventKind.Misc, "Misc" },
            { EventArchiveEventKind.MubankPresident, "Mubank President" },
            { EventArchiveEventKind.WeverseLive, "Weverse Live" },
        }.ToFrozenDictionary();
    }
}

