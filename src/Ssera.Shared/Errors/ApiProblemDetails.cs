namespace Ssera.Shared.Errors;

public sealed record ApiProblemDetails
{
    public int Status { get; init; }
    public string? Type { get; init; }
    public string? Title { get; init; }
    public string? Detail { get; init; }
    public string? TraceId { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }
}
