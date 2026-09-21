using Microsoft.AspNetCore.Components;
using Ssera.Shared.Events;
using Ssera.Shared.Events.Filters;

namespace Ssera.Client.Pages.Events;

public partial class EventsFilters
{
    private static readonly IReadOnlyList<KeyValuePair<string, OrderByType>> _allOrderByTypes =
    [
        .. Enum.GetValues<OrderByType>()
            .Select(v => KeyValuePair.Create(v.ToString(), v))
    ];

    private static readonly IReadOnlyList<KeyValuePair<string, SortType>> _allSortTypes =
    [
        .. Enum.GetValues<SortType>()
            .Select(v => KeyValuePair.Create(v.ToString(), v))
    ];

    private static readonly IReadOnlyList<KeyValuePair<string, EventType>> _allEventTypes =
    [
        .. Enum.GetValues<EventType>()
            .Select(v => KeyValuePair.Create(v.GetDisplayName(), v))
    ];

    private const string DirtyBackgroundColor = "color-mix(in srgb, var(--rz-primary) 25%, transparent)";

    private EventsFiltersModel _filters = new();
    private EventsFiltersModel _appliedFilters = new();

    [Parameter, EditorRequired]
    public EventCallback<EventsFiltersModel> OnApplyFilters { get; set; }

    [Parameter, EditorRequired]
    public bool Loading { get; set; }

    private bool IsDirty =>
        _filters.OrderByType != _appliedFilters.OrderByType
        || _filters.SortType != _appliedFilters.SortType
        || _filters.PageSize != _appliedFilters.PageSize
        || _filters.Search != _appliedFilters.Search
        || !_filters.EventTypes.SetEquals(_appliedFilters.EventTypes);

    public void SetFilters(EventsFiltersModel filters)
    {
        _filters = filters;
        _appliedFilters = filters with
        {
            EventTypes = new HashSet<EventType>(filters.EventTypes),
        };
        StateHasChanged();
    }

    private static string DirtyBackground(bool dirty)
        => dirty ? $"background-color: {DirtyBackgroundColor}" : "";

    private string GetEventTypeBackground(EventType eventType)
        => DirtyBackground(
            _filters.EventTypes.Contains(eventType) != _appliedFilters.EventTypes.Contains(eventType));

    private async Task ApplyFilters()
        => await OnApplyFilters.InvokeAsync(_filters);
}
