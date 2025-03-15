using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Ssera.Api.Data;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum EventArchiveEventKid
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

    public static string AsHuman(this EventArchiveEventKid eventType) =>
        Names.EnumToHuman.TryGetValue(eventType, out var result)
        ? result
        : throw new ArgumentException($"Invalid {nameof(EventArchiveEventKid)}: {eventType}");

    public static class Names
    {
        public static FrozenDictionary<string, EventArchiveEventKid> HumanToEnum { get; } = new Dictionary<string, EventArchiveEventKid>
        {
            { "Teasers/MV", EventArchiveEventKid.TeasersMV },
            { "Performance", EventArchiveEventKid.Performance },
            { "Music Shows", EventArchiveEventKid.MusicShows },
            { "Behind The Scene", EventArchiveEventKid.BehindTheScenes },
            { "Interview", EventArchiveEventKid.Interview },
            { "Variety", EventArchiveEventKid.Variety },
            { "Reality", EventArchiveEventKid.Reality },
            { "CF", EventArchiveEventKid.CF },
            { "Misc", EventArchiveEventKid.Misc },
            { "Mubank President", EventArchiveEventKid.MubankPresident },
            { "Weverse Live", EventArchiveEventKid.WeverseLive },
        }.ToFrozenDictionary();

        public static FrozenDictionary<EventArchiveEventKid, string> EnumToHuman { get; } = new Dictionary<EventArchiveEventKid, string>
        {
            { EventArchiveEventKid.TeasersMV, "Teasers/MV" },
            { EventArchiveEventKid.Performance, "Performance" },
            { EventArchiveEventKid.MusicShows, "Music Shows" },
            { EventArchiveEventKid.BehindTheScenes, "Behind The Scene" },
            { EventArchiveEventKid.Interview, "Interview" },
            { EventArchiveEventKid.Variety, "Variety" },
            { EventArchiveEventKid.Reality, "Reality" },
            { EventArchiveEventKid.CF, "CF" },
            { EventArchiveEventKid.Misc, "Misc" },
            { EventArchiveEventKid.MubankPresident, "Mubank President" },
            { EventArchiveEventKid.WeverseLive, "Weverse Live" },
        }.ToFrozenDictionary();
    }
}

