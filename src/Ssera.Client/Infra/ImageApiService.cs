using Ssera.Shared.Images;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ssera.Client.Infra;

public sealed class ImageApiService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<GetImagesResponse> GetImagesAsync(
        GetImagesQuery query,
        CancellationToken token)
    {
        var data = JsonSerializer.Serialize(query);
        var encoded = Uri.EscapeDataString(data);

        await Task.Delay(TimeSpan.FromSeconds(2), token);

        var response = await _httpClient.GetFromJsonAsync<GetImagesResponse>(
            $"?requestParameters={encoded}", token)
            ?? throw new InvalidOperationException("Response was null");

        return response;
    }
}
