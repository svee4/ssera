using Microsoft.AspNetCore.Components;
using Radzen;
using Ssera.Shared.Data;
using Ssera.Shared.Images.Filters;

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
    private IReadOnlyList<string> _tagsDropdownData = ["meow", "preview", "press", "something else"];

    private Variant _designVariant = Variant.Outlined;

    private FiltersModel _filters { get; set; } = new();

    [Parameter, EditorRequired]
    public EventCallback<FiltersModel> OnApplyFilters { get; set; }

    [Parameter, EditorRequired]
    public bool Loading { get; set; }

    public void SetFilters(FiltersModel filters)
    {
        _filters = filters;
        StateHasChanged();
    }

    private async Task TagsDropdownLoadData(LoadDataArgs args)
    {
        await Task.Yield();
    }

    private async Task ApplyFilters()
        => await OnApplyFilters.InvokeAsync(_filters);

}
