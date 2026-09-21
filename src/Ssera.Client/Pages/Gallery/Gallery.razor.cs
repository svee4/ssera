using Microsoft.AspNetCore.Components;
using Ssera.Client.Infra;
using Ssera.Shared.Images;
using System.Collections.Specialized;
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

    private CancellationTokenSource _cts = new();
    private ApiException? _error;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // YES its cursed YES we need to yield first to let the page render
            // or otherwise we get a flash of unstyled document.
            await Task.Yield();

            var filters = DeserializeFiltersFromUrl();
            _filtersComponent.SetFilters(filters);

            await OnFiltersChanged(filters);

            // YES we need StateHasChanged here NO i don't know why
            StateHasChanged();
        }
    }

    private async Task OnFiltersChanged(Filters.FiltersModel filters)
    {
        GetImagesQueryTagsFilter? tagsFilter = null;

        {
            var tags = filters.Tags.ToArray();
            if (tags.Length > 0)
            {
                tagsFilter = new GetImagesQueryTagsFilter
                {
                    Tags = tags,
                    TagsSelectionType = filters.TagsSelectionType
                };
            }
        }

        var query = new GetImagesQuery
        {
            PageSize = filters.PageSize,
            OrderBy = filters.OrderByType,
            Sort = filters.SortType,
            Eras = [.. filters.Eras],
            Members = [.. filters.Members],
            TagsFilter = tagsFilter
        };

        Loading = true;

        try
        {
            var requestParameters = JsonSerializer.Serialize(
                query with { Page = 1 },
                JsonSerializerOptions.Web);

            var requestUri = $"api/images?requestParameters={Uri.EscapeDataString(requestParameters)}";

            using var response = await HttpClient.GetAsync(requestUri, _cts.Token);
            var body = await response.Content.ReadAsStringAsync(_cts.Token);

            if (response.IsSuccessStatusCode)
            {
                Images = JsonSerializer.Deserialize<GetImagesResponse>(body, JsonSerializerOptions.Web)
                    ?.Images ?? [];

                _error = null;
            }
            else
            {
                _error = ApiException.FromResponse(response, body);
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

        SerializeFiltersToUrl(filters);
        _filtersComponent.SetFilters(filters);
    }

    private void SerializeFiltersToUrl(Filters.FiltersModel filters)
    {
        var uri = new Uri(NavigationManager.Uri);
        var query = HttpUtility.ParseQueryString(uri.Query);

        query[FiltersKey] = JsonSerializer.Serialize(filters, JsonSerializerOptions.Web);
        query[PageKey] = "1";
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
}
