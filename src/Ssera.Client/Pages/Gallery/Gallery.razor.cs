using Microsoft.AspNetCore.Components;
using Ssera.Client.Infra;
using Ssera.Shared.Images;

namespace Ssera.Client.Pages.Gallery;

public partial class Gallery
{
    [Inject]
    private ImageApiService ImageApiService { get; set; } = null!;

    private CancellationTokenSource _cts = new();

    private async Task OnFiltersChanged(Filters.FiltersModel viewModel)
    {
        GetImagesQueryTagsFilter? tagsFilter = null;

        {
            var tags = viewModel.Tags.ToArray();
            if (tags.Length > 0)
            {
                tagsFilter = new GetImagesQueryTagsFilter
                {
                    Tags = tags,
                    TagsSelectionType = viewModel.TagsSelectionType
                };
            }
        }

        var request = new GetImagesQuery
        {
            Page = 1,
            PageSize = 20,
            OrderBy = viewModel.OrderByType,
            Sort = viewModel.SortType,
            Eras = [.. viewModel.Eras],
            Members = [.. viewModel.Members],
            TagsFilter = tagsFilter
        };

        var images = await ImageApiService.GetImagesAsync(request, _cts.Token);
    }
}
