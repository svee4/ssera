using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Ssera.Api.Data;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum EventSheetEventKind
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

public static class EventSheet
{

    public static string AsHuman(this EventSheetEventKind eventType) =>
        Names.EnumToHuman.TryGetValue(eventType, out var result)
        ? result
        : throw new ArgumentException($"Invalid {nameof(EventSheetEventKind)}: {eventType}"); 

    public static class Names
    {
        public static FrozenDictionary<string, EventSheetEventKind> HumanToEnum { get; } = new Dictionary<string, EventSheetEventKind>
        {
            { "Teasers/MV", EventSheetEventKind.TeasersMV },
            { "Performance", EventSheetEventKind.Performance },
            { "Music Shows", EventSheetEventKind.MusicShows },
            { "Behind The Scene", EventSheetEventKind.BehindTheScenes },
            { "Interview", EventSheetEventKind.Interview },
            { "Variety", EventSheetEventKind.Variety },
            { "Reality", EventSheetEventKind.Reality },
            { "CF", EventSheetEventKind.CF },
            { "Misc", EventSheetEventKind.Misc },
            { "Mubank President", EventSheetEventKind.MubankPresident },
            { "Weverse Live", EventSheetEventKind.WeverseLive },
        }.ToFrozenDictionary();

        public static FrozenDictionary<EventSheetEventKind, string> EnumToHuman { get; } = new Dictionary<EventSheetEventKind, string>
        {
            { EventSheetEventKind.TeasersMV, "Teasers/MV" },
            { EventSheetEventKind.Performance, "Performance" },
            { EventSheetEventKind.MusicShows, "Music Shows" },
            { EventSheetEventKind.BehindTheScenes, "Behind The Scene" },
            { EventSheetEventKind.Interview, "Interview" },
            { EventSheetEventKind.Variety, "Variety" },
            { EventSheetEventKind.Reality, "Reality" },
            { EventSheetEventKind.CF, "CF" },
            { EventSheetEventKind.Misc, "Misc" },
            { EventSheetEventKind.MubankPresident, "Mubank President" },
            { EventSheetEventKind.WeverseLive, "Weverse Live" },            
        }.ToFrozenDictionary();
    }
}

