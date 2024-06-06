using System.Globalization;
using Google.Apis.Services;
using Google.Apis.Util;
using LsfArchiveHelper.Api.Database;
using LsfArchiveHelper.Api.Features.Events;
using LsfArchiveHelper.Api.Features.History;
using LsfArchiveHelper.Api.Worker.Mappers;
using Microsoft.EntityFrameworkCore;

namespace LsfArchiveHelper.Api.Worker;

public sealed class Worker(
	IServiceScopeFactory serviceScopeFactory,
	IConfiguration configuration,
	ILogger<Worker> logger
) : IHostedService
{
	private const string LeSserafimArchiveSpreadsheetId = "14NeZu0cI5Tkd8jkGuORRdukFtAtGPnNFUSzVoLZnZuc";

	private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
	private readonly IConfiguration _configuration = configuration;
	private readonly ILogger _logger = logger;

	private CancellationTokenSource _stoppingSource = null!;
	private int _delay;

	public Task StartAsync(CancellationToken cancellationToken)
	{
		_stoppingSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

		_delay = 1000 * int.Parse(
			_configuration["WorkerDelay"] ?? throw new InvalidOperationException("No timeout configured"),
			CultureInfo.InvariantCulture);

		_ = Task.Run(RunLoop, CancellationToken.None);

		return Task.CompletedTask;
	}

	public async Task StopAsync(CancellationToken cancellationToken) => await _stoppingSource.CancelAsync();

	private async Task RunLoop()
	{
		var token = _stoppingSource.Token;
		while (!token.IsCancellationRequested)
		{
			try
			{
				await RunTask(token);
			}
			catch (Exception ex)
			{
				_logger.LogCritical("Worker task uncaught exception: {Exception}", ex);

				await using var scope = _serviceScopeFactory.CreateAsyncScope();
				var addHistoryHandler = scope.ServiceProvider.GetRequiredService<AddHistory.Handler>();

				if (!await addHistoryHandler.HandleAsync(
					    new AddHistory.Command
					    {
						    TimeTaken = TimeSpan.Zero,
						    TotalEvents = 0,
						    Message = "Uncaught worker task exception has terminated the host service"
					    }, token))
				{
					_logger.LogError("Failed to add history entry");
				}

				await _stoppingSource.CancelAsync();
				return;
			}

			await Task.Delay(_delay, token);
		}
	}

	private async Task RunTask(CancellationToken token)
	{
		_logger.LogInformation("Starting task");
		if (token.IsCancellationRequested) return;

		var startTime = DateTime.Now;

		using var service =
			new Google.Apis.Sheets.v4.SheetsService(
				new BaseClientService.Initializer { ApiKey = _configuration["ApiKey"] });

		var request = service.Spreadsheets.Get(LeSserafimArchiveSpreadsheetId);
		var spreadsheet = await request.ExecuteAsync(token);

		_logger.LogInformation("Received initial sheet");

		List<Database.Event> entries = []; // using database model directly i know

		foreach (var sheet in spreadsheet.Sheets)
		{
			if (token.IsCancellationRequested) return;

			var sheetName = sheet.Properties.Title;
			var knownSheet = IMapper.KnownSheets.FirstOrDefault(knownSheet => knownSheet.Name == sheetName);
			if (knownSheet is null)
			{
				_logger.LogInformation("Skipping unknown sheet {SheetName}", sheetName);
				continue;
			}

			var type = EventTypeExtensions.ParseEventType(sheetName);

			_logger.LogInformation("Getting sheet {SheetName} with mapper {MapperName}", sheetName,
				knownSheet.Mapper.GetType().Name);

			// get sheet data individually because loading all at once takes too much memory
			var sheetRequest = service.Spreadsheets.Get(LeSserafimArchiveSpreadsheetId);
			sheetRequest.Ranges = new Repeatable<string>([sheetName]);
			sheetRequest.IncludeGridData = true;
			var spreadsheetData = await sheetRequest.ExecuteAsync(token);

			_logger.LogInformation("Got data for sheet {SheetName}", sheetName);

			var sheetData = spreadsheetData.Sheets[0];

			if (token.IsCancellationRequested) return;

			var countBefore = entries.Count;
			entries.AddRange(
				knownSheet.Mapper
					.ParseEvents(sheetData)
					.Select(entry => Database.Event.CreateNew(DateTime.SpecifyKind(entry.Date, DateTimeKind.Utc), type, entry.Title, entry.Link)));

			var addedRowCount = entries.Count - countBefore;
			_logger.LogInformation("Successfully parsed sheet {SheetName} for a total of {RowCount} rows", sheetName,
				addedRowCount);
		}

		_logger.LogInformation("Got all data, total row count of {RowCount}", entries.Count);

		if (token.IsCancellationRequested) return;

		var dbFilename = _configuration["SqliteFilename"] ??
		                 throw new InvalidOperationException("Sqlite file name not configured");
		var backupDbFilename = _configuration["BackupSqliteFilename"] ??
		                       throw new InvalidOperationException("Backup sqlite file name not configured");

		_logger.LogInformation("Backing up sqlite from {SourceFilename} to {DestinationFilename}", dbFilename,
			backupDbFilename);
		File.Delete(backupDbFilename);
		File.Copy(dbFilename, backupDbFilename);

		if (token.IsCancellationRequested) return;

		await using var scope = _serviceScopeFactory.CreateAsyncScope();
		await using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

		bool success;
		await using (var transaction = await dbContext.Database.BeginTransactionAsync(token))
		{
			try
			{
				_logger.LogInformation("Clearing database");
				await dbContext.Events.ExecuteDeleteAsync(token);

				_logger.LogInformation("Saving entries to database");
				await dbContext.Events.AddRangeAsync(entries, token);
				await dbContext.SaveChangesAsync(token);
				await transaction.CommitAsync(token);
				success = true;
			}
			catch (Exception ex)
			{
				_logger.LogError("Failed to clear or save entries to database: {Exception}", ex);
				success = false;
			}
		}

		var endTime = DateTime.Now;
		var timeTaken = endTime - startTime;

		var addHistoryHandler = scope.ServiceProvider.GetRequiredService<AddHistory.Handler>();

		if (success)
		{
			_logger.LogInformation("Successfully completed task");

			if (!await addHistoryHandler.HandleAsync(
				    new AddHistory.Command { TotalEvents = entries.Count, TimeTaken = timeTaken },
				    token
			    ))
			{
				_logger.LogError("Failed to add history entry");
			}
		}
		else
		{
			_logger.LogCritical("Worker task failed to complete successfully, will terminate");

			if (!await addHistoryHandler.HandleAsync(
				    new AddHistory.Command
				    {
					    TimeTaken = timeTaken,
					    TotalEvents = 0,
					    Message = "Task failed"
				    },
				    token
			    ))
			{
				_logger.LogError("Failed to add history entry");
			}

			throw new WorkerException("Task did not complete successfully");
		}
	}

	public sealed class WorkerException : Exception
	{
		public WorkerException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public WorkerException()
		{
		}

		public WorkerException(string message) : base(message)
		{
		}
	}
}
