using Microsoft.AspNetCore.Components;
using Ssera.Client.Infra;
using Ssera.Shared.Images;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.Json;
using System.Web;

namespace Ssera.Client.Pages.Gallery;

public partial class Gallery
{
    private const string FiltersKey = "filters";
    private const string PageKey = "page";

    [Inject]
    private HttpClient HttpClient { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private Ssera.Client.Pages.Gallery.Filters.Filters _filtersComponent = null!;

    private IReadOnlyList<GetImagesResponse.Image> Images { get; set; } = [];
    private bool Loading { get; set; } = true;

    private ApiException? _error;
    private Filters.FiltersModel _filters = new();
    private int _page = 1;
    private int _totalResults;

    private int MaxPage => _filters.PageSize <= 0
        ? 1
        : Math.Max(1, (int)Math.Ceiling((double)_totalResults / _filters.PageSize));

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // YES its cursed YES we need to yield first to let the page render
            // or otherwise we get a flash of unstyled document.
            await Task.Yield();

            _page = ReadPageFromUrl();
            _filters = DeserializeFiltersFromUrl();
            _filtersComponent.SetFilters(_filters);

            await FetchAsync();

            // YES we need StateHasChanged here NO i don't know why
            StateHasChanged();
        }
    }

    private async Task OnFiltersChanged(Filters.FiltersModel filters)
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
        Images = [];

        try
        {
            while (true)
            {
                var requestParameters = JsonSerializer.Serialize(BuildQuery(), JsonSerializerOptions.Web);
                var requestUri = $"api/images?requestParameters={Uri.EscapeDataString(requestParameters)}";

                using var response = await HttpClient.GetAsync(requestUri);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _error = ApiException.FromResponse(response, body);
                    return;
                }

                var result = JsonSerializer.Deserialize<GetImagesResponse>(body, JsonSerializerOptions.Web);
                Images = result?.Images ?? [];
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

    private GetImagesQuery BuildQuery()
    {
        GetImagesQueryTagsFilter? tagsFilter = null;
        var tags = _filters.Tags.ToArray();
        if (tags.Length > 0)
        {
            tagsFilter = new GetImagesQueryTagsFilter
            {
                Tags = tags,
                TagsSelectionType = _filters.TagsSelectionType
            };
        }

        return new GetImagesQuery
        {
            Page = _page,
            PageSize = _filters.PageSize,
            OrderBy = _filters.OrderByType,
            Sort = _filters.SortType,
            Eras = [.. _filters.Eras],
            Members = [.. _filters.Members],
            TagsFilter = tagsFilter
        };
    }

    private void SerializeFiltersToUrl()
    {
        var uri = new Uri(NavigationManager.Uri);
        NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
        query[FiltersKey] = JsonSerializer.Serialize(_filters, JsonSerializerOptions.Web);
        query[PageKey] = _page.ToString(CultureInfo.InvariantCulture);
        NavigationManager.NavigateTo($"{uri.GetLeftPart(UriPartial.Path)}?{query}");
    }

    private Filters.FiltersModel DeserializeFiltersFromUrl()
    {
        var json = HttpUtility.ParseQueryString(new Uri(NavigationManager.Uri).Query)[FiltersKey];

        if (string.IsNullOrWhiteSpace(json))
        {
            return new Filters.FiltersModel();
        }

        try
        {
            return JsonSerializer.Deserialize<Filters.FiltersModel>(json, JsonSerializerOptions.Web)
                ?? new Filters.FiltersModel();
        }
        catch (JsonException)
        {
            return new Filters.FiltersModel();
        }
    }

    private int ReadPageFromUrl()
    {
        var page = HttpUtility.ParseQueryString(new Uri(NavigationManager.Uri).Query)[PageKey];
        return int.TryParse(page, CultureInfo.InvariantCulture, out var value) && value >= 1 ? value : 1;
    }
}
