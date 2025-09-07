using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Ssera.Client.Infra;
using Ssera.Shared.Images;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Web;

namespace Ssera.Client.Pages.Gallery;

public partial class Gallery
{
    [Inject]
    private ImageApiService ImageApiService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private Ssera.Client.Pages.Gallery.Filters.Filters _filtersComponent = null!;

    private IReadOnlyList<GetImagesResponse.Image> Images { get; set; } = [];
    private bool Loading { get; set; } = true;

    private CancellationTokenSource _cts = new();

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

        var request = new GetImagesQuery
        {
            Page = 1,
            PageSize = filters.PageSize,
            OrderBy = filters.OrderByType,
            Sort = filters.SortType,
            Eras = [.. filters.Eras],
            Members = [.. filters.Members],
            TagsFilter = tagsFilter
        };

        Loading = true;
        Images = [];

        var result = await ImageApiService.GetImagesAsync(request, _cts.Token);

        Images = result.Images;
        Loading = false;

        // i am not fucking around with desynced state
        SerializeFiltersToUrl(filters);
        _filtersComponent.SetFilters(DeserializeFiltersFromUrl());
    }

    private void SerializeFiltersToUrl(Filters.FiltersModel filters)
    {
        var serializableRequest = filters with
        {
            // Tags are arbitrary user input - they are from the drive folders.
            // We need to encode them to prevent issues.
            Tags = filters.Tags.Select(s => HttpUtility.UrlEncode(s))
        };

        var json = JsonSerializer.Serialize(serializableRequest);

        var uri = new Uri(NavigationManager.Uri);
        var newUri = $"{uri.GetLeftPart(UriPartial.Path)}?filters={json}";
        NavigationManager.NavigateTo(newUri);
    }

    private Filters.FiltersModel DeserializeFiltersFromUrl()
    {
        var uri = new Uri(NavigationManager.Uri);
        var query = HttpUtility.ParseQueryString(uri.Query);
        var filtersJson = query.Get("filters");

        if (string.IsNullOrWhiteSpace(filtersJson))
        {
            return new Filters.FiltersModel();
        }
        try
        {
            var deserialized = JsonSerializer.Deserialize<Filters.FiltersModel>(filtersJson);
            if (deserialized is null)
            {
                return new Filters.FiltersModel();
            }

            // Decode tags
            deserialized = deserialized with
            {
                Tags = deserialized.Tags.Select(s => HttpUtility.UrlDecode(s))
            };

            return deserialized;
        }
        catch
        {
            return new Filters.FiltersModel();
        }
    }
}
