using Ssera.Shared.Data;
using Ssera.Shared.Images.Filters;

namespace Ssera.Client.Pages.Gallery.Filters;

public sealed record FiltersModel
{
    public OrderByType OrderByType { get; set; } = OrderByType.Date;
    public SortType SortType { get; set; } = SortType.Descending;
    public int PageSize { get; set; } = 50;

    public ISet<Era> Eras { get; set; } = new HashSet<Era>();
    public ISet<GroupMember> Members { get; set; } = new HashSet<GroupMember>();

    public IEnumerable<string> Tags { get; set; } = [];
    public TagsFilterType TagsSelectionType { get; set; } = TagsFilterType.Include;
}
