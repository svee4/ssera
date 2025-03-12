using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace Ssera.Api.Database;

[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength", Justification = "Not possible in Sqlite")]
public sealed class Event
{
    public int Id { get; set; }
    public required DateTime DateUtc { get; set; }
    public required EventType Type { get; set; }
    public required string? Title { get; set; }
    public required string? Link { get; set; }
    public required DateTime CreatedUtc { get; set; }

    private Event()
    {
    }

    /// <summary>
    /// Validates parameters and creates new Event. Throws an ArgumentException on validation on failure
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public static Event CreateNew(DateTime dateUtc, EventType type, string? title, string? link)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(link))
        {
            var message = $"Both {nameof(Title)} and {nameof(Link)} cannot be null or empty, only one may";
            throw new ArgumentException(message);
        }

        return dateUtc.Kind != DateTimeKind.Utc
            ? throw new ArgumentException("DateTime.Kind must be DateTimeKind.Utc", nameof(dateUtc))
            : new Event
            {
                DateUtc = dateUtc,
                Type = type,
                Title = title,
                Link = link,
                CreatedUtc = DateTime.UtcNow
            };
    }

    public sealed class Configuration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            _ = builder.HasIndex(m => m.DateUtc);
            _ = builder.HasIndex(m => m.Type);
            _ = builder.HasIndex(m => m.Title);
        }
    }
}



