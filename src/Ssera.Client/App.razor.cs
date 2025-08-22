using Ssera.Client.Infra;

namespace Ssera.Client;

public partial class App(ThemeService themeService)
{
    private readonly ThemeService _themeService = themeService;

    protected override async Task OnInitializedAsync()
    {
        await _themeService.InitializeTheme();
    }
}
