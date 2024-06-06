using System.Globalization;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using LsfArchiveHelper.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace LsfArchiveHelper.Api.Features.History;

[Handler]
[MapGet("/api/history")]
public sealed partial class GetHistory
{
	public sealed record Query;

	private static async ValueTask<List<HistoryModel>> HandleAsync(
		Query _,
		AppDbContext dbContext,
		CancellationToken token
	) =>
		await dbContext.WorkerHistory
			.OrderByDescending(m => m.CreatedUtc)
			.Select(m => new HistoryModel(
				m.CreatedUtc, 
				m.TotalEvents,
				m.TimeTaken.ToString("hh':'mm':'ss", CultureInfo.InvariantCulture),
				m.Message))
			.Take(50)
			.ToListAsync(token);

	// TODO figure out home timespans work in javascript
	public sealed record HistoryModel(DateTime Date, int TotalEvents, string TimeTaken, string? Message);
}
