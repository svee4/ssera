using Microsoft.AspNetCore.Components;

namespace Ssera.Client.Infra.Components;

public partial class SseraPagingControls
{
    private static readonly IReadOnlyDictionary<string, object> _pageInputAttributes =
        new Dictionary<string, object>
        {
            ["title"] = "Choose page",
            ["aria-label"] = "Choose page",
        };

    [Parameter]
    public int Page { get; set; }

    [Parameter]
    public EventCallback<int> PageChanged { get; set; }

    [Parameter, EditorRequired]
    public int PageSize { get; set; }

    [Parameter, EditorRequired]
    public int TotalResults { get; set; }

    private int MaxPage => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling((double)TotalResults / PageSize));

    private int Start => ((Page - 1) * PageSize) + 1;

    private int End => Math.Min(Page * PageSize, TotalResults);

    private async Task SetPage(int value)
    {
        var clamped = Math.Clamp(value, 1, MaxPage);

        if (clamped == Page)
        {
            return;
        }

        await PageChanged.InvokeAsync(clamped);
    }

    private async Task OnPageInput(int value)
        => await SetPage(value);
}
