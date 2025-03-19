using Google.Apis.Drive.v3;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Ssera.Api.Data;
using Ssera.Api.Features.History;
using Ssera.Api.Infra.Configuration;
using System.Collections.Immutable;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using static Ssera.Api.Data.ImageArchive;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace Ssera.Api.Ingestion.ImageArchive;

public sealed partial class ImageArchiveWorker(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<ImageArchiveWorker> logger) : BackgroundService
{

    // https://drive.google.com/drive/folders/1Bdv2NdIyIwNbW-CTicgVIvTwQCc_Gkvs
    private const string ChaewonDriveId = "1Bdv2NdIyIwNbW-CTicgVIvTwQCc_Gkvs";
    // https://drive.google.com/drive/folders/1nG5cPDN_tm7QwQxOepZYDB9e-pvMyJ60
    private const string SakuraDriveId = "1nG5cPDN_tm7QwQxOepZYDB9e-pvMyJ60";
    // https://drive.google.com/drive/folders/1YzA7dJYuJKTLXqMt8DaFHtUsIMdO8ss7
    private const string YunjinDriveId = "1YzA7dJYuJKTLXqMt8DaFHtUsIMdO8ss7";
    // https://drive.google.com/drive/folders/17VlZ9XVr6DKB1o5pu1eo0BQoYXqfqjD9
    private const string KazuhaDriveId = "17VlZ9XVr6DKB1o5pu1eo0BQoYXqfqjD9";
    // https://drive.google.com/drive/folders/133Df8XIdz2fD6yMboAELj5mnxQILZ8VX
    private const string EunchaeDriveId = "133Df8XIdz2fD6yMboAELj5mnxQILZ8VX";

    private readonly IServiceScopeFactory _sserviceScopeFactory = scopeFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<ImageArchiveWorker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var token = stoppingToken;
        var delay = 1000 * _configuration.GetRequiredParsedValue<int>("WorkerDelay", CultureInfo.InvariantCulture);

        while (!token.IsCancellationRequested)
        {
            try
            {
                await Run(token);
            }
            catch
            {
                if (!token.IsCancellationRequested)
                {
                    await using var serviceScope = _sserviceScopeFactory.CreateAsyncScope();
                    var handler = serviceScope.ServiceProvider.GetRequiredService<AddHistory.Handler>();

                    if (!await handler.HandleAsync(new AddHistory.Command(
                            nameof(ImageArchiveWorker),
                            "Uncaught exception has terminated the worker"),
                        token))
                    {
                        _logger.LogError("Failed to add uncaught exception history entry");
                    }
                }

                throw;
            }

            await Task.Delay(delay, token);
        }
    }

    private sealed record IngestionState(
        GroupMember Current,
        DriveService Service,
        string ApiKey,
        List<string> PublicLog);

    private async Task Run(CancellationToken token)
    {
        var start = DateTime.UtcNow;
        await using var serviceScope = _sserviceScopeFactory.CreateAsyncScope();

        // this implementation is terribly inefficient;
        // we create a cool hierarchy of a ton of objects, just to flatten it into the database.
        // we also, you know, put EVERY SINGLE ENTRY in memory before saving.
        // if it becomes a problem, its easy to save individually for each archive,
        // however this breaks the transactionality we currently have.
        // either way, in testing the app peaked at 250mb and speed is a low concern,
        // so optimizations will be left for later.

        List<string> publicLog = [];
        List<ArchiveEntry> archiveEntries = [];

        try
        {
            var apiKey = _configuration.GetRequiredValue("GoogleApiKey");

            using var service = new DriveService(new Google.Apis.Services.BaseClientService.Initializer
            {
                ApplicationName = "Ssera",
                ApiKey = apiKey,
            });

            var util = new IngestionState(0, service, apiKey, publicLog);

            IReadOnlyList<ImageArchiveInfo> archives = [
                new ImageArchiveInfo(GroupMember.Chaewon, ChaewonDriveId),
                new ImageArchiveInfo(GroupMember.Sakura, SakuraDriveId),
                new ImageArchiveInfo(GroupMember.Yunjin, YunjinDriveId),
                new ImageArchiveInfo(GroupMember.Kazuha, KazuhaDriveId),
                new ImageArchiveInfo(GroupMember.Eunchae, EunchaeDriveId),
            ];

            foreach (var archive in archives)
            {
                archiveEntries.Add(await IngestArchive(archive, util with { Current = archive.Member }, token));
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Image archive ingestion failed: {Exception}", e);
            throw;
        }

        await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApiDbContext>();


        var dbEntryCount = 0;
        var subLevelCount = 0;

        foreach (var ae in archiveEntries)
        {
            foreach (var tle in ae.TopLevelEntries)
            {
                foreach (var sle in tle.SubLevelEntries)
                {
                    subLevelCount++;
                    foreach (var e in sle.Entries)
                    {
                        dbEntryCount++;
                    }
                }
            }
        }

        _logger.LogInformation("Sublevel count: {SubLevelCount}", subLevelCount);

        var dbEntries = new List<ImageArchiveEntry>(dbEntryCount);
        foreach (var archiveEntry in archiveEntries)
        {
            foreach (var topLevelEntry in archiveEntry.TopLevelEntries)
            {
                foreach (var subLevelEntry in topLevelEntry.SubLevelEntries)
                {
                    foreach (var entry in subLevelEntry.Entries)
                    {
                        dbEntries.Add(ImageArchiveEntry.Create(
                            entry.FileId,
                            archiveEntry.Member,
                            topLevelEntry.Kind,
                            subLevelEntry.Date,
                            [subLevelEntry.Name, .. entry.Tags]));
                    }
                }
            }
        }

        bool success;
        try
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync(token);
            _ = await dbContext.ImageArchive.ExecuteDeleteAsync(token);
            dbContext.ImageArchive.AddRange(dbEntries);
            _ = await dbContext.SaveChangesAsync(token);

            await transaction.CommitAsync(token);
            success = true;

            _logger.LogInformation("Ingestion run successful");
        }
        catch (Exception e)
        {
            success = false;
            _logger.LogError("Save failed: {Exception}", e);
        }

        var end = DateTime.UtcNow;
        var timeTaken = end - start;

        var message = new StringBuilder()
            .Append("Time taken: ")
            .Append(timeTaken.ToString("mm':'ss", CultureInfo.InvariantCulture))
            .AppendLine()
            .Append("Total entries: ")
            .Append(dbEntries.Count.ToString(CultureInfo.InvariantCulture))
            .AppendLine();

        if (!success)
        {
            _ = message.AppendLine("Worker task failed to complete succesfully");
        }

        foreach (var log in publicLog)
        {
            _ = message.AppendLine(log);
        }

        var handler = serviceScope.ServiceProvider.GetRequiredService<AddHistory.Handler>();

        if (!await handler.HandleAsync(
                new AddHistory.Command(nameof(ImageArchiveWorker), message.ToString()), token))
        {
            _logger.LogError("Failed to add history entry");
        }
    }

    private async Task<ArchiveEntry> IngestArchive(
        ImageArchiveInfo archive,
        IngestionState util,
        CancellationToken token)
    {
        using var scope = _logger.BeginScope("{ArchiveName}", archive.Member);

        var files = await GetFilesInFolder(archive.DriveId, util, token);
        var filenameRegex = TopLevelFolderRegex();

        var topLevelFolders = files.Files
            .Where(f => filenameRegex.IsMatch(f.Name));

        List<TopLevelEntry> entries = [];

        foreach (var folder in topLevelFolders)
        {
            var entry = await IngestTopLevelFolder(folder, util, token);
            if (entry is null)
            {
                continue;
            }

            entries.Add(entry);
        }

        return new ArchiveEntry(archive.Member, [.. entries]);
    }

    private async Task<TopLevelEntry?> IngestTopLevelFolder(
        DriveFile folder,
        IngestionState state,
        CancellationToken token)
    {
        using var _scope = _logger.BeginScope("{TopLevelFolderName}", folder.Name);

        if (!IsFolder(folder))
        {
            throw new ArgumentException("File is not a folder", nameof(folder));
        }

        if (!TryParseTopLevelKind(folder.Name, out var folderKind))
        {
            state.PublicLog.Add($"Skipping top level folder {folder.Name} in {state.Current}");
            _logger.LogWarning("Could not parse kind of top level folder {TopLevelFolderName}", folder.Name);
            return null;
        }

        var filesResult = await GetFilesInFolder(folder.Id, state, token);
        var folderNameRegex = DatedFilenameRegex();

        List<SubLevelEntry> subEntries = [];

        _logger.LogInformation("Ingesting {Count} subfolders", filesResult.Files.Count);

        foreach (var subFolder in filesResult.Files)
        {
            using var _scope2 = _logger.BeginScope("{SubLevelFolderName}", subFolder.Name);
            _logger.LogDebug("Subfolder: {SubLevelFolderName}", subFolder.Name);

            var match = folderNameRegex.Match(subFolder.Name);
            if (!match.Success)
            {
                _logger.LogWarning("Regex name match failed");
                state.PublicLog.Add($"Failed to parse folder name " +
                    $"{subFolder.Name} in {folder.Name} in {state.Current}");

                continue;
            }

            var dateSpan = match.Groups["Date"].ValueSpan;
            if (!TryParseDate(dateSpan, out var date))
            {
                _logger.LogError("Date parse failed (input: {DateString})", dateSpan.ToString());
                state.PublicLog.Add($"Failed to parse date {dateSpan} " +
                    $"in {subFolder.Name} in {folder.Name} in {state.Current}");

                continue;
            }

            var name = match.Groups["Name"].Value;

            var entries = await IngestSubLevelFolder(subFolder, state, token);
            subEntries.Add(new SubLevelEntry(date, name, [.. entries]));
        }

        return new TopLevelEntry(folderKind, subEntries.ToImmutableArray());
    }

    private async Task<List<Entry>> IngestSubLevelFolder(
            DriveFile folder,
            IngestionState state,
            CancellationToken token) =>
        await IngestFolder(folder, [], state, token);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1822:Mark members as static", Justification = "No!")]
    private async Task<List<Entry>> IngestFolder(
        DriveFile folder,
        ImmutableArray<string> tags,
        IngestionState state,
        CancellationToken token)
    {
        if (!IsFolder(folder))
        {
            throw new ArgumentException($"File {folder.Name} is not a folder", nameof(folder));
        }

        List<Entry> entries = [];

        var files = await GetFilesInFolder(folder.Id, state, token);
        var datedFilenameRegex = DatedFilenameRegex();

        foreach (var file in files.Files)
        {
            var match = datedFilenameRegex.Match(file.Name);

            var name = match.Success ? match.Groups["Name"].Value : file.Name;
            name = Path.GetFileNameWithoutExtension(name);

            if (IsFolder(file))
            {
                var innerEntries = await IngestFolder(
                    file,
                    tags.Add(name),
                    state,
                    token);

                entries.AddRange(innerEntries);
            }
            else
            {
                entries.Add(new Entry(name, file.Name, file.Id, tags));
            }
        }

        return entries;
    }

    private static async Task<Google.Apis.Drive.v3.Data.FileList> GetFilesInFolder(
        string folderId,
        IngestionState util,
        CancellationToken token)
    {
        var request = util.Service.Files.List();
        request.Key = util.ApiKey;
        request.Q = $"'{folderId}' in parents";
        return await request.ExecuteAsync(token);
    }

    private static bool IsFolder(DriveFile file) =>
        file.MimeType == "application/vnd.google-apps.folder";

    private static bool TryParseDate(ReadOnlySpan<char> input, out DateTime date) =>
        DateTime.TryParseExact(input, "yyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    private static bool TryParseTopLevelKind(string input, out TopLevelKind kind)
    {
        ArgumentException.ThrowIfNullOrEmpty(input);

        List<(string Match, TopLevelKind Kind)> options =
        [
            ("SNS",TopLevelKind.SNS),
            ("Fearless",TopLevelKind.Fearless),
            ("Antifragile",TopLevelKind.Antifragile),
            ("Unforgiven",TopLevelKind.Unforgiven),
            ("Perfect Night",TopLevelKind.PerfectNight),
            ("Easy",TopLevelKind.Easy),
            ("Crazy",TopLevelKind.Crazy),
            ("Hot",TopLevelKind.Hot),
            ("Misc",TopLevelKind.Misc),
        ];

        foreach (var (match, resultKind) in options)
        {
            if (input.Contains(match, StringComparison.OrdinalIgnoreCase))
            {
                kind = resultKind;
                return true;
            }
        }

        kind = default;
        return false;
    }

    // the actual regex we want to use eventually is (\d{1,2}.)|(Misc)|(Photoshoots)
    // misc and sns will need some special handling, so [1-9]+. for now
    // for testing, 1. works for getting only fearless
    [GeneratedRegex(@"[1-9]+.")]
    private static partial Regex TopLevelFolderRegex();

    [GeneratedRegex(@"(?<Date>\d{6})\s+(?<Name>.+)")]
    private static partial Regex DatedFilenameRegex();
}
