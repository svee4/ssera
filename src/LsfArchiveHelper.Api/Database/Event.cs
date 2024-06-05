using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LsfArchiveHelper.Api.Database;

[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength", Justification = "Not possible in Sqlite")]
public sealed class Event
{
	
	public int Id { get; set; }
	
	public required DateTime Date { get; set; }
	public required EventType Type { get; set; }
	public required string? Title { get; set; }
	public required string? Link { get; set; }
	public required DateTime CreatedUtc { get; set; }

	private Event()
	{
	}

	public static Event CreateNew(DateTime date, EventType type, string? title, string? link)
	{
		if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(link))
		{
			Throw();

			[DoesNotReturn, StackTraceHidden]
			static void Throw() => throw new ArgumentException($"Both {nameof(title)} and {nameof(link)} cannot be null or empty, only one may");
		}

		return new Event { Date = date, Type = type, Title = title, Link = link, CreatedUtc = DateTime.UtcNow };
	}

	public sealed class Configuration : IEntityTypeConfiguration<Event>
	{
		public void Configure(EntityTypeBuilder<Event> builder)
		{
			ArgumentNullException.ThrowIfNull(builder);
			
			builder.HasIndex(m => m.Date);
			builder.HasIndex(m => m.Type);
			builder.HasIndex(m => m.Title);
		}
	}
}
