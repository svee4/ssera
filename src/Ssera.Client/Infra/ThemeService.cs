using System.Diagnostics;

namespace Ssera.Client.Infra;

public sealed class ThemeService(LocalStorageService localStorage, Radzen.ThemeService radzenThemeService)
{
    public static IReadOnlyList<string> Themes { get; } = [.. Radzen.Themes.Free.Select(t => t.Value)];

    private const string DefaultTheme = "material-dark";
    private const string ThemeKey = "Ssera.ThemeService.Theme";

    private readonly LocalStorageService _localStorage = localStorage;
    private readonly Radzen.ThemeService _radzenThemeService = radzenThemeService;

    private async Task<string> GetStoredTheme()
    {
        var theme = await _localStorage.GetValue(ThemeKey);

        if (string.IsNullOrEmpty(theme) || !Themes.Contains(theme))
        {
            theme = DefaultTheme;
            await _localStorage.SetValue(ThemeKey, theme);
        }

        return theme;
    }

    public async Task<string> CurrentTheme()
        => await GetStoredTheme();

    public async Task SetTheme(string theme)
    {
        if (!Themes.Contains(theme))
        {
            throw new ArgumentException($"Theme '{theme}' is not a valid theme.");
        }

        _radzenThemeService.SetTheme(theme);
        await _localStorage.SetValue(ThemeKey, theme);
    }

    public async Task InitializeTheme()
    {
        var theme = await GetStoredTheme();
        _radzenThemeService.SetTheme(theme);
    }
}
