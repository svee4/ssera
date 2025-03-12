using Google.Apis.Sheets.v4.Data;
using Ssera.Api.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Ssera.Api.Ingestion.Archive.Mappers;

public sealed class CFMapper(ILogger<CFMapper> logger) : IEventSheetMapper
{
    private readonly ILogger<CFMapper> _logger = logger;

    private static class Columns
    {
        public static int Date => 1;
        public static int CFType => 2;
        public static int Title => 3;
        public static int Members => 4;
        public static int Link => 5;
    };

    public IEnumerable<Event> ParseEvents(Sheet sheet)
    {
        Debug.Assert(sheet is not null);

        var data = sheet.Data[0];

        return Core();

        IEnumerable<Event> Core()
        {
            var previousDate = DateTime.MinValue;
            foreach (var (rowIndex, row) in data.RowData.Skip(2).Index())
            {
                var link = row.GetNormalizedColumnValue(Columns.Link);
                var title = row.GetNormalizedColumnValue(Columns.Title);
                var cfType = row.GetNormalizedColumnValue(Columns.CFType);
                var members = row.GetNormalizedColumnValue(Columns.Members);
                
                if (link is null || (title is null && cfType is null && members is null))
                {
                    continue;
                }

                Debug.Assert(link.StartsWith("https", StringComparison.OrdinalIgnoreCase));

                // try get date from this row, if fails fallback to previous row
                DateTime date;
                if (row.TryGetColumnValue(Columns.Date, out var dateString))
                {
                    if (DateTime.TryParseExact(
                        dateString,
                        MappingExtensions.DefaultDateFormat,
                        CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out var result))
                    {
                        date = previousDate = result;
                    }
                    else
                    {
                        MappingExtensions.LogInvalidDateFormat(_logger,
                            EventSheetEventKind.CF.AsHuman(), dateString, rowIndex);
                        date = previousDate;
                    }
                }
                else
                {
                    date = previousDate;
                }

                const string Separator = " - ";

                var titleBuilder = new StringBuilder();
                if (link is not null) _ = titleBuilder.Append(link).Append(Separator);
                if (title is not null) _ = titleBuilder.Append(title).Append(Separator);
                if (cfType is not null) _ = titleBuilder.Append(cfType).Append(Separator);
                if (members is not null) _ = titleBuilder.Append(members).Append(Separator);

                _ = titleBuilder.Remove(titleBuilder.Length - Separator.Length, Separator.Length);

                yield return new Event(date, titleBuilder.ToString(), link);
            }
        }
    }
}
