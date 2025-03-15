using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace Ssera.Api.Data;

[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength",
    Justification = "Not possible in Sqlite")]
public sealed class EventArchiveEntry
{
    public int Id { get; set; }
    public required DateTime Date { get; set; }
    public required EventArchiveEventKid Type { get; set; }
    public required string? Title { get; set; }
    public required string? Link { get; set; }
    public required DateTime CreatedUtc { get; set; }

    private EventArchiveEntry() { }

    /// <summary>
    /// date.Kind must be UTC even if the value itself is not UTC
    /// </summary>
    public static EventArchiveEntry CreateNew(DateTime date, EventArchiveEventKid type, string? title, string? link)
    {
        if (date.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("DateTime.Kind must be DateTimeKind.Utc", nameof(date));
        }

        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(link))
        {
            var message = $"Both {nameof(Title)} and {nameof(Link)} cannot be null or empty, only one may";
            throw new ArgumentException(message);
        }

        return new EventArchiveEntry
        {
            Date = date,
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
