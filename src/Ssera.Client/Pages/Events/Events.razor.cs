using Microsoft.AspNetCore.Components;
using Ssera.Client.Infra;
using Ssera.Shared.Events;
using Ssera.Shared.Events.Filters;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.Json;
using System.Web;

namespace Ssera.Client.Pages.Events;

public partial class Events
{
    private const string FiltersKey = "filters";
    private const string PageKey = "page";

    [Inject]
    private HttpClient HttpClient { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private EventsFilters _filtersComponent = null!;

    private IReadOnlyList<Event> _events = [];
    private bool Loading { get; set; } = true;

    private ApiException? _error;
    private EventsFiltersModel _filters = new();
    private int _page = 1;
    private int _totalResults;

    private int MaxPage => _filters.PageSize <= 0
        ? 1
        : Math.Max(1, (int)Math.Ceiling((double)_totalResults / _filters.PageSize));

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Yield();

            _page = ReadPageFromUrl();
            _filters = DeserializeFiltersFromUrl();
            _filtersComponent.SetFilters(_filters);

            await FetchAsync();

            StateHasChanged();
        }
    }

    private async Task OnFiltersChanged(EventsFiltersModel filters)
    {
        _filters = filters;
        _page = 1;

        await FetchAsync();

        SerializeFiltersToUrl();
        _filtersComponent.SetFilters(_filters);
    }

    private async Task OnPageChanged(int page)
    {
        _page = page;

        await FetchAsync();

        SerializeFiltersToUrl();
    }

    private async Task FetchAsync()
    {
        Loading = true;
        _events = [];

        try
        {
            while (true)
            {
                using var response = await HttpClient.GetAsync(BuildRequestUri());
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _error = ApiException.FromResponse(response, body);
                    return;
                }

                var result = JsonSerializer.Deserialize<GetEventsResponse>(body, JsonSerializerOptions.Web);
                _events = result?.Results ?? [];
                _totalResults = result?.TotalResults ?? 0;
                _error = null;

                if (_page > MaxPage)
                {
                    _page = MaxPage;
                    continue;
                }

                break;
            }
        }
        catch (HttpRequestException exception)
        {
            _error = new ApiException(null, null, null, exception);
        }
        finally
        {
            Loading = false;
        }
    }

    private string BuildRequestUri()
    {
        var parameters = new List<string>
        {
            $"orderBy={_filters.OrderByType}",
            $"sort={_filters.SortType}",
        };

        foreach (var eventType in _filters.EventTypes)
        {
            parameters.Add($"eventTypes={eventType}");
        }

        if (!string.IsNullOrWhiteSpace(_filters.Search))
        {
            parameters.Add($"search={Uri.EscapeDataString(_filters.Search)}");
        }

        parameters.Add($"page={_page}");
        parameters.Add($"pageSize={_filters.PageSize}");

        return $"api/events?{string.Join("&", parameters)}";
    }

    private void SerializeFiltersToUrl()
    {
        var uri = new Uri(NavigationManager.Uri);
        NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
        query[FiltersKey] = JsonSerializer.Serialize(_filters, JsonSerializerOptions.Web);
        query[PageKey] = _page.ToString(CultureInfo.InvariantCulture);
        NavigationManager.NavigateTo($"{uri.GetLeftPart(UriPartial.Path)}?{query}");
    }

    private EventsFiltersModel DeserializeFiltersFromUrl()
    {
        var json = HttpUtility.ParseQueryString(new Uri(NavigationManager.Uri).Query)[FiltersKey];

        if (string.IsNullOrWhiteSpace(json))
        {
            return new EventsFiltersModel();
        }

        try
        {
            return JsonSerializer.Deserialize<EventsFiltersModel>(json, JsonSerializerOptions.Web)
                ?? new EventsFiltersModel();
        }
        catch (JsonException)
        {
            return new EventsFiltersModel();
        }
    }

    private int ReadPageFromUrl()
    {
        var page = HttpUtility.ParseQueryString(new Uri(NavigationManager.Uri).Query)[PageKey];
        return int.TryParse(page, CultureInfo.InvariantCulture, out var value) && value >= 1 ? value : 1;
    }
}
