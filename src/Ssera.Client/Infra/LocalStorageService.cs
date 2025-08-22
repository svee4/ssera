using Microsoft.JSInterop;

namespace Ssera.Client.Infra;

public sealed class LocalStorageService(IJSRuntime jsRuntime, ILogger<LocalStorageService> logger)
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private readonly ILogger<LocalStorageService> _logger = logger;

    public async Task SetValue(string key, string value)
    {
        _logger.LogDebug("Setting {Key} = {Value}", key, value);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    public async Task<string?> GetValue(string key)
    {
        _logger.LogDebug("Getting {Key}", key);
        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
    }
}
