using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ssera.Api.Data;

public sealed class EventArchiveEntry
{
    public int Id { get; private set; }

    /// <summary>
    /// Store as UTC even if value is not actually UTC
    /// </summary>
    public DateTime Date => DateTime.SpecifyKind(_date, DateTimeKind.Utc);

    // we have to use a backing field with a getter property that specifies UTC
    // because when reading from db, the kind is unspecified which is problematic
    private DateTime _date;

    public EventArchiveEventKind Type { get; private set; }
    public string? Title { get; private set; }
    public string? Link { get; private set; }

    /// <summary>
    /// UTC
    /// </summary>
    public DateTime Created => DateTime.SpecifyKind(_created, DateTimeKind.Utc);
    private DateTime _created;

    private EventArchiveEntry() { }

    public static EventArchiveEntry CreateNew(DateTime date, EventArchiveEventKind type, string? title, string? link)
    {
        if (date.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("DateTime Kind must be UTC");
        }

        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(link))
        {
            var message = $"Both {nameof(Title)} and {nameof(Link)} cannot be null or empty, only one may";
            throw new ArgumentException(message);
        }

        return new EventArchiveEntry
        {
            _date = date,
            Type = type,
            Title = title,
            Link = link,
            _created = DateTime.UtcNow
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
