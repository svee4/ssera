using Ssera.Shared.Events.Filters;

namespace Ssera.Client.Pages.Events;

public sealed record EventsFiltersModel
{
    public OrderByType OrderByType { get; set; } = OrderByType.Date;
    public SortType SortType { get; set; } = SortType.Descending;
    public ISet<EventType> EventTypes { get; set; } = new HashSet<EventType>();
    public int PageSize { get; set; } = 100;

    public string Search
    {
        get;
        set => field = value ?? "";
    } = "";
}
