using Microsoft.AspNetCore.Components;
using Ssera.Client.Infra;

namespace Ssera.Client.Layout;

public partial class MainLayout
{
    [Inject]
    private ThemeService ThemeService { get; set; } = null!;

    private string _theme = ThemeService.DefaultTheme;

    private async Task SetTheme(string theme)
    {
        await ThemeService.SetTheme(theme);
        _theme = theme;
    }

    protected override async Task OnInitializedAsync()
    {
        _theme = await ThemeService.CurrentTheme();
    }
}
