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

    private static readonly IReadOnlyList<int> _allPageSizes = [50, 100, 500, 1000];

    private FiltersViewModel _viewModel = new();

    private IReadOnlyList<string> _tagsDropdownData = ["meow", "preview", "press", "something else"];

    private async Task TagsDropdownLoadData(LoadDataArgs args)
    {
        await Task.Yield();
    }

}
