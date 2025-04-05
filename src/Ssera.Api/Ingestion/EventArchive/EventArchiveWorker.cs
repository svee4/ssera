using Google.Apis.Services;
using Google.Apis.Util;
using Microsoft.EntityFrameworkCore;
using Ssera.Api.Data;
using Ssera.Api.Features.History;
using Ssera.Api.Infra.Configuration;
using Ssera.Api.Ingestion.EventArchive.Mappers;
using System.Globalization;

namespace Ssera.Api.Ingestion.EventArchive;

public sealed class EventArchiveWorker(
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration,
    ILogger<EventArchiveWorker> logger
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

        _delay = 1000 * _configuration.GetRequiredParsedValue<int>("WorkerDelay", CultureInfo.InvariantCulture);

        _ = Task.Run(RunLoop, CancellationToken.None);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken) =>
        await _stoppingSource.CancelAsync();

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

                if (!await addHistoryHandler.HandleAsync(new AddHistory.Command(
                        nameof(EventArchiveWorker),
                        "Uncaught exception terminated task prematurely"), token))
                {
                    _logger.LogError("Failed to add history entry");
                }
            }

            await Task.Delay(_delay, token);
        }
    }

    private async Task RunTask(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var startTime = DateTime.Now;

        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        using var service = new Google.Apis.Sheets.v4.SheetsService(
            new BaseClientService.Initializer 
            {
                ApiKey = _configuration.GetRequiredValue("GoogleApiKey"),
                ApplicationName = "Ssera"
            });

        var request = service.Spreadsheets.Get(LeSserafimArchiveSpreadsheetId);
        var spreadsheet = await request.ExecuteAsync(token);

        _logger.LogInformation("Received initial sheet");

        List<EventArchiveEntry> allEvents = [];

        foreach (var sheet in spreadsheet.Sheets)
        {
            token.ThrowIfCancellationRequested();

            var sheetName = sheet.Properties.Title;

            if (sheetName is "Directory" or "Tikok / Youtube Shorts")
            {
                continue;
            }

            if (!Data.EventArchive.Names.HumanToEnum.TryGetValue(sheetName, out var sheetType))
            {
                _logger.LogError("Sheet name {SheetName} could not be mapped to enum", sheetName);
                continue;
            }

            var mapper = scope.ServiceProvider.GetKeyedService<IEventArchiveSheetMapper>(sheetType.AsHuman());
            if (mapper is null)
            {
                _logger.LogError("No mapper registered for sheet {SheetType}", sheetType);
                continue;
            }

            _logger.LogInformation("Getting sheet {Sheet} with mapper {Mapper}", sheetType, mapper.GetType());

            // get sheet data individually because loading all at once takes too much memory
            var sheetRequest = service.Spreadsheets.Get(LeSserafimArchiveSpreadsheetId);
            sheetRequest.Ranges = new Repeatable<string>([sheetName]);
            sheetRequest.IncludeGridData = true;
            var spreadsheetData = await sheetRequest.ExecuteAsync(token);

            var sheetData = spreadsheetData.Sheets[0];

            token.ThrowIfCancellationRequested();

            var countBefore = allEvents.Count;

            var events = mapper
                .ParseEvents(sheetData)
                .Select(ev => EventArchiveEntry.CreateNew(ev.Date, sheetType, ev.Title, ev.Link));

            allEvents.AddRange(events);

            var eventCount = allEvents.Count - countBefore;
            _logger.LogInformation("Parsed sheet {Sheet} for {EventCount} events", sheetType, eventCount);
        }

        _logger.LogInformation("Got all data, total row count of {RowCount}", allEvents.Count);

        token.ThrowIfCancellationRequested();

        await using var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

        bool success;
        await using (var transaction = await dbContext.Database.BeginTransactionAsync(token))
        {
            _ = await dbContext.EventArchive.ExecuteDeleteAsync(token);
            await dbContext.EventArchive.AddRangeAsync(allEvents, token);
            _ = await dbContext.SaveChangesAsync(token);

            try
            {
                await transaction.CommitAsync(token);
                success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Transaction failed: {Exception}", ex);
                success = false;
            }
        }

        var endTime = DateTime.Now;
        var timeTaken = endTime - startTime;

        var addHistoryHandler = scope.ServiceProvider.GetRequiredService<AddHistory.Handler>();

        if (success)
        {
            _logger.LogInformation("Worker task completed succesfully");

            var message = $"""
Time taken: {timeTaken.ToString("mm':'ss", CultureInfo.InvariantCulture)}
Total events: {allEvents.Count.ToString(CultureInfo.InvariantCulture)}
""";

            if (!await addHistoryHandler.HandleAsync(
                    new AddHistory.Command(nameof(EventArchiveWorker), message),
                    token))
            {
                _logger.LogError("Failed to add history entry");
            }
        }
        else
        {
            _logger.LogError("Worker task failed to complete successfully");

            var message = $"""
Time taken: {timeTaken.ToString("HH':'mm", CultureInfo.InvariantCulture)}
Total events: {allEvents.Count.ToString(CultureInfo.InvariantCulture)}
Task failed to complete succesfully
""";

            if (!await addHistoryHandler.HandleAsync(
                    new AddHistory.Command(nameof(EventArchiveWorker), message),
                    token))
            {
                _logger.LogError("Failed to add history entry");
            }
        }
    }
}
