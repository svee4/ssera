using Ssera.Shared.Errors;
using System.Net;
using System.Text.Json;

namespace Ssera.Client.Infra;

public sealed class ApiException(
    HttpStatusCode? status,
    ApiProblemDetails? problem,
    string? responseBody,
    Exception? innerException = null)
    : Exception(BuildMessage(status, problem, responseBody), innerException)
{
    public HttpStatusCode? Status { get; } = status;

    public ApiProblemDetails? Problem { get; } = problem;

    public static ApiException FromResponse(HttpResponseMessage response, string responseBody)
        => new(response.StatusCode, TryParseProblemDetails(responseBody), responseBody);

    private static ApiProblemDetails? TryParseProblemDetails(string responseBody)
    {
        try
        {
            return JsonSerializer.Deserialize<ApiProblemDetails>(responseBody, JsonSerializerOptions.Web);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string BuildMessage(HttpStatusCode? status, ApiProblemDetails? problem, string? responseBody)
    {
        if (problem is not null)
        {
            return $"({(int?)status}) {problem.Title}: {problem.Detail}. Response body: {responseBody}";
        }

        if (status is not null)
        {
            return $"({(int)status}) Request failed. Response body: {responseBody}";
        }

        return "Request failed because the API could not be reached.";
    }
}
