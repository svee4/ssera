using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Ssera.Shared.Images;
using Ssera.Shared.Images.Filters;

namespace Ssera.Client.Pages.Gallery;

public partial class Results
{
    [Parameter, EditorRequired]
    public IReadOnlyList<GetImagesResponse.Image> Images { get; set; } = [];

    [Parameter, EditorRequired]
    public bool Loading { get; set; }

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    // these values are duplicated in masonry.js
    private static int RowHeight => 1;
    private static int RowGap => 5;
    private static int ImageWidth => 200;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await JsRuntime.InvokeVoidAsync("window.updateMasonry");
    }

    private static List<string> GetTags(GetImagesResponse.Image image)
    {
        var tags = new List<string>();

        if (image.Era is { } era)
        {
            tags.Add(era.GetDisplayName());
        }

        tags.AddRange(image.Tags);
        return tags;
    }

    private static string GetDriveThumbnailUrl(string fileId)
        => $"https://drive.google.com/thumbnail?id={fileId}&sz=w200";

    private static string GetDriveUrl(string fileId)
        => $"https://drive.google.com/file/d/{fileId}/view";
}
