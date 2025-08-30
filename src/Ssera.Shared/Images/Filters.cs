
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Ssera.Shared.Images.Filters;

[JsonConverter(typeof(JsonStringEnumConverter<OrderByType>))]
public enum OrderByType
{
    Date = 1,
    Tags,
}

[JsonConverter(typeof(JsonStringEnumConverter<SortType>))]
public enum SortType
{
    Ascending = 1,
    Descending
}

[JsonConverter(typeof(JsonStringEnumConverter<Era>))]
public enum Era
{
    Fearless = 1,
    Antifragile,
    Unforgiven,
    PerfectNight,
    Easy,
    Crazy,
}

public static class ImagesFiltersExtensions
{
    /// <summary>Gets the display name of the given <see cref="OrderByType"/>.</summary>
    /// <exception cref="UnreachableException"></exception>
    public static string GetDisplayName(this OrderByType value)
        => value switch
        {
            OrderByType.Date => "Date",
            OrderByType.Tags => "Tags",
            _ => throw new UnreachableException($"Unknown {nameof(OrderByType)} value '{value}'")
        };

    /// <summary>Gets the display name of the given <see cref="SortType"/>.</summary>
    /// <exception cref="UnreachableException" />
    public static string GetDisplayName(this SortType value)
        => value switch
        {
            SortType.Ascending => "Ascending",
            SortType.Descending => "Descending",
            _ => throw new UnreachableException($"Unknown {nameof(SortType)} value '{value}'")
        };

    /// <summary>Gets the display name of the given <see cref="Era"/>.</summary>
    /// <exception cref="UnreachableException" />"
    public static string GetDisplayName(this Era value)
        => value switch
        {
            Era.Fearless => "Fearless",
            Era.Antifragile => "Antifragile",
            Era.Unforgiven => "Unforgiven",
            Era.PerfectNight => "Perfect Night",
            Era.Easy => "Easy",
            Era.Crazy => "Crazy",
            _ => throw new UnreachableException($"Unknown {nameof(Era)} value '{value}'")
        };

}
