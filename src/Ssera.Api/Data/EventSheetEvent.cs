using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace Ssera.Api.Data;

[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength", Justification = "Not possible in Sqlite")]
public sealed class EventSheetEvent
{
    public int Id { get; set; }
    public required DateTime Date { get; set; }
    public required EventSheetEventKind Type { get; set; }
    public required string? Title { get; set; }
    public required string? Link { get; set; }
    public required DateTime CreatedUtc { get; set; }

    private EventSheetEvent() { }

    /// <summary>
    /// date.Kind must be UTC even if the value itself is not UTC
    /// </summary>
    public static EventSheetEvent CreateNew(DateTime date, EventSheetEventKind type, string? title, string? link)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(link))
        {
            var message = $"Both {nameof(Title)} and {nameof(Link)} cannot be null or empty, only one may";
            throw new ArgumentException(message);
        }

        if (date.Kind != DateTimeKind.Utc)
        {

            throw new ArgumentException("DateTime.Kind must be DateTimeKind.Utc", nameof(date));
        }

        return new EventSheetEvent
        {
            Date = date,
            Type = type,
            Title = title,
            Link = link,
            CreatedUtc = DateTime.UtcNow
        };
    }

    private sealed class Configuration : IEntityTypeConfiguration<EventSheetEvent>
    {
        public void Configure(EntityTypeBuilder<EventSheetEvent> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            _ = builder.HasIndex(m => m.Date);
            _ = builder.HasIndex(m => m.Type);
            _ = builder.HasIndex(m => m.Title);
        }
    }
}



