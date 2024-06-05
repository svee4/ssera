using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LsfArchiveHelper.Api.Database;

[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength",
	Justification = "Not possible in Sqlite")]
public sealed class WorkerHistory
{
	public int Id { get; set; }
	public required DateTime CreatedUtc { get; set; }
	public required int TotalEvents { get; set; }
	public required TimeSpan TimeTaken { get; set; }
	public string? Message { get; set; }

	private WorkerHistory()
	{
	}

	public static WorkerHistory CreateNew(int totalEvents, TimeSpan timeTaken, string? message)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(totalEvents);
		return new WorkerHistory
		{
			CreatedUtc = DateTime.UtcNow,
			TotalEvents = totalEvents,
			TimeTaken = timeTaken,
			Message = message
		};
	}

	public sealed class Configuration : IEntityTypeConfiguration<WorkerHistory>
	{
		public void Configure(EntityTypeBuilder<WorkerHistory> builder)
		{
			ArgumentNullException.ThrowIfNull(builder);

			builder.HasIndex(m => m.CreatedUtc);
		}
	}
}
