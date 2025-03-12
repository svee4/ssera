namespace Ssera.Api.Worker;

public class Event
{
    public required DateTime Date { get; init; }
    public required string? Title { get; init; }
    public required string? Link { get; init; }
}
