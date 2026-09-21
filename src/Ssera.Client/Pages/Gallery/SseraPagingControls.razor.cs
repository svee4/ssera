using Microsoft.AspNetCore.Components;

namespace Ssera.Client.Pages.Gallery;

public partial class SseraPagingControls
{
    private int _inputValue;
    private int _inputRevision;

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

    protected override void OnParametersSet()
    {
        _inputValue = Page;
    }

    private async Task SetPage(int value)
    {
        var clamped = Math.Clamp(value, 1, MaxPage);

        if (clamped == Page)
        {
            return;
        }

        await PageChanged.InvokeAsync(clamped);
    }

    private void ApplyInput()
    {
        var clamped = Math.Clamp(_inputValue, 1, MaxPage);

        if (clamped == Page)
        {
            _inputValue = clamped;
            _inputRevision++;
            return;
        }

        _ = PageChanged.InvokeAsync(clamped);
    }
}
