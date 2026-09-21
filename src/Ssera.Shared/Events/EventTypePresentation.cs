using Ssera.Shared.Events.Filters;
using System.Diagnostics;

namespace Ssera.Shared.Events;

public static class EventTypePresentation
{
    public static string GetDisplayName(this EventType value)
        => value switch
        {
            EventType.TeasersMV => "Teasers/MV",
            EventType.Performance => "Performance",
            EventType.MusicShows => "Music Shows",
            EventType.BehindTheScenes => "Behind The Scenes",
            EventType.Interview => "Interview",
            EventType.Variety => "Variety",
            EventType.Reality => "Reality",
            EventType.CF => "CF",
            EventType.Misc => "Miscellaneous",
            EventType.MubankPresident => "Mubank President",
            EventType.WeverseLive => "Weverse Live",
            _ => throw new UnreachableException($"Unknown {nameof(EventType)} value '{value}'")
        };

    public static string GetColor(this EventType value)
        => value switch
        {
            EventType.TeasersMV => "#ff00ff",
            EventType.Performance => "#ff9900",
            EventType.MusicShows => "#d0e0e3",
            EventType.BehindTheScenes => "#0000ff",
            EventType.Interview => "#00ff00",
            EventType.Variety => "#9900ff",
            EventType.Reality => "#ffff00",
            EventType.CF => "#ffd966",
            EventType.Misc => "#4a86e8",
            EventType.MubankPresident => "#c27ba0",
            EventType.WeverseLive => "#0be6c1",
            _ => throw new UnreachableException($"Unknown {nameof(EventType)} value '{value}'")
        };

    public static string GetDarkColor(this EventType value)
        => value switch
        {
            EventType.TeasersMV => "#431d42",
            EventType.Performance => "#6b4317",
            EventType.MusicShows => "#2e3b3d",
            EventType.BehindTheScenes => "#192b56",
            EventType.Interview => "#2c5229",
            EventType.Variety => "#36224f",
            EventType.Reality => "#505014",
            EventType.CF => "#5d4b0c",
            EventType.Misc => "#19325b",
            EventType.MubankPresident => "#4f1f3a",
            EventType.WeverseLive => "#004e40",
            _ => throw new UnreachableException($"Unknown {nameof(EventType)} value '{value}'")
        };
}
