using Microsoft.AspNetCore.Components;
using Radzen;
using Ssera.Shared.Data;
using Ssera.Shared.Images.Filters;
using System.Text.Json;

namespace Ssera.Client.Pages.Gallery.Filters;

public partial class Filters
{
    private static readonly IReadOnlyList<KeyValuePair<string, OrderByType>> _allOrderByTypes =
        Enum.GetValues<OrderByType>()
        .Select(v => KeyValuePair.Create(v.GetDisplayName(), v))
        .ToArray();

    private static readonly IReadOnlyList<KeyValuePair<string, SortType>> _allSortTypes =
        Enum.GetValues<SortType>()
        .Select(v => KeyValuePair.Create(v.GetDisplayName(), v))
        .ToArray();

    private static readonly IReadOnlyList<KeyValuePair<string, Era>> _allEras =
        Enum.GetValues<Era>()
        .Select(v => KeyValuePair.Create(v.GetDisplayName(), v))
        .ToArray();

    private static readonly IReadOnlyList<KeyValuePair<string, GroupMember>> _allMembers =
        Enum.GetValues<GroupMember>()
        .Select(v => KeyValuePair.Create(v.GetDisplayName(), v))
        .ToArray();

    private static readonly IReadOnlyList<KeyValuePair<string, TagsFilterType>> _allTagsSelectionTypes =
        [
            KeyValuePair.Create("Include only selected tags", TagsFilterType.Include),
            KeyValuePair.Create("Exclude selected tags", TagsFilterType.Exclude)
        ];

    private static readonly IReadOnlyList<int> _allPageSizes = [50, 100, 500, 1000];

    private Variant _designVariant = Variant.Outlined;

    private FiltersModel _filters = new();
    private FiltersModel _appliedFilters = new();
    private IReadOnlyList<string> _tagsDropdownData = [];

    [Inject]
    private HttpClient HttpClient { get; set; } = null!;

    [Parameter, EditorRequired]
    public EventCallback<FiltersModel> OnApplyFilters { get; set; }

    [Parameter, EditorRequired]
    public bool Loading { get; set; }

    private bool IsDirty =>
        _filters.OrderByType != _appliedFilters.OrderByType
        || _filters.SortType != _appliedFilters.SortType
        || _filters.PageSize != _appliedFilters.PageSize
        || _filters.TagsSelectionType != _appliedFilters.TagsSelectionType
        || !_filters.Eras.SetEquals(_appliedFilters.Eras)
        || !_filters.Members.SetEquals(_appliedFilters.Members)
        || !_filters.Tags.SequenceEqual(_appliedFilters.Tags);

    public void SetFilters(FiltersModel filters)
    {
        _filters = filters;
        _appliedFilters = filters with
        {
            Eras = new HashSet<Era>(filters.Eras),
            Members = new HashSet<GroupMember>(filters.Members),
            Tags = [.. filters.Tags],
        };
        StateHasChanged();
    }

    private async Task TagsDropdownLoadData(LoadDataArgs args)
    {
        var requestUri = "api/images/tags";

        if (!string.IsNullOrWhiteSpace(args.Filter))
        {
            requestUri += $"?search={Uri.EscapeDataString(args.Filter)}";
        }

        try
        {
            var body = await HttpClient.GetStringAsync(requestUri);
            var tags = JsonSerializer.Deserialize<List<string>>(body, JsonSerializerOptions.Web) ?? [];

            // selected tags must stay in the list, otherwise the dropdown drops them
            _tagsDropdownData = [.. tags, .. _filters.Tags.Where(tag => !tags.Contains(tag))];
        }
        catch (HttpRequestException)
        {
            // keep the tags that are already loaded
        }
    }

    private async Task ApplyFilters()
        => await OnApplyFilters.InvokeAsync(_filters);
}
