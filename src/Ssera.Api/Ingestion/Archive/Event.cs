namespace Ssera.Api.Ingestion.Archive;

public record Event(DateTime Date, string? Title, string? Link);
