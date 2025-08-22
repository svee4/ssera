using Ssera.Client.Infra;

namespace Ssera.Client.Layout;

public partial class ThemePicker(ThemeService themeService)
{
    private readonly ThemeService _themeService = themeService;
    private string _theme;

    private async Task SetTheme(string theme)
    {
        await _themeService.SetTheme(theme);
        _theme = theme;
    }

    protected override async Task OnInitializedAsync()
    {
        _theme = await _themeService.CurrentTheme();
    }
}
