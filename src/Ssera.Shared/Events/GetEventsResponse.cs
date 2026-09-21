using Ssera.Shared.Events.Filters;

namespace Ssera.Shared.Events;

public sealed record GetEventsResponse(List<Event> Results, DateTime? LastUpdate, int TotalResults);

public sealed record Event(DateTime Date, EventType Type, string? Title, string? Link);
