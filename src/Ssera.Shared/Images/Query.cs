using Immediate.Validations.Shared;
using Ssera.Shared.Data;
using Ssera.Shared.Images.Filters;
using System.Text.Json;

namespace Ssera.Shared.Images;

internal static class Helpers
{
    public static bool TryParseFromJson<T>(string value, out T? result)
        where T : IValidationTarget<T>
    {
        try
        {
            result = JsonSerializer.Deserialize<T>(value);

            if (!T.Validate(result).IsValid)
            {
                result = default;
                return false;
            }

            return true;
        }
        catch (JsonException)
        {
            result = default;
            return false;
        }
    }
}

[Validate]
public sealed partial record GetImagesQuery : IValidationTarget<GetImagesQuery>
{
    [GreaterThanOrEqual(1)]
    public int Page { get; init; }

    [GreaterThanOrEqual(10), LessThanOrEqual(1000)]
    public int PageSize { get; init; }

    public OrderByType? OrderBy { get; init; }
    public SortType? Sort { get; init; }

    public GetImagesQueryTagsFilter? TagsFilter { get; init; }

    public Era[]? Eras { get; init; }

    public GroupMember[]? Members { get; init; }

    /// <summary>Tries to parse a <see cref="GetImagesQuery"/> from json.</summary>
    /// <remarks>Exists for minimal api support.</remarks>
    public static bool TryParse(string value, out GetImagesQuery? result)
        => Helpers.TryParseFromJson(value, out result);
}

[Validate]
public sealed partial record GetImagesQueryTagsFilter : IValidationTarget<GetImagesQueryTagsFilter>
{
    [MinLength(1)]
    public string[] Tags { get; init; } = null!;

    public TagsFilterType TagsSelectionType { get; init; }
}

public sealed record GetImagesResponse(List<GetImagesResponse.Image> Images, int TotalResults)
{
    public sealed record Image(
        string Id,
        GroupMember Member,
        Era? Era,
        int Width,
        int Height,
        IReadOnlyList<string> Tags,
        DateTime Date);
}
