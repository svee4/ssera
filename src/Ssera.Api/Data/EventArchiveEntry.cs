using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ssera.Api.Data;

public sealed class EventArchiveEntry
{
    public int Id { get; set; }
    public required DateTime Date { get; set; }
    public required EventArchiveEventKind Type { get; set; }
    public required string? Title { get; set; }
    public required string? Link { get; set; }
    public required DateTime CreatedUtc { get; set; }

    private EventArchiveEntry() { }

    public static EventArchiveEntry CreateNew(DateTime date, EventArchiveEventKind type, string? title, string? link)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(link))
        {
            var message = $"Both {nameof(Title)} and {nameof(Link)} cannot be null or empty, only one may";
            throw new ArgumentException(message);
        }

        return new EventArchiveEntry
        {
            Date = DateTime.SpecifyKind(date, DateTimeKind.Utc),
            Type = type,
            Title = title,
            Link = link,
            CreatedUtc = DateTime.UtcNow
        };
    }

    private sealed class Configuration : IEntityTypeConfiguration<EventArchiveEntry>
    {
        public void Configure(EntityTypeBuilder<EventArchiveEntry> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            _ = builder.HasIndex(m => m.Date);
            _ = builder.HasIndex(m => m.Type);
            _ = builder.HasIndex(m => m.Title);
        }
    }
}
