using Google.Apis.Sheets.v4.Data;
using Ssera.Api.Ingestion.Archive.Mappers;

namespace Ssera.Api.Ingestion.EventArchive.Mappers;


public sealed class PerformanceVarietyRealityMapper : IEventArchiveSheetMapper
{
    private static class Columns
    {
        public static int Date => 1;
        public static int Series => 2;
        public static int Title => 3;
        public static int Link => 4;
    }

    public IEnumerable<Event> ParseEvents(Sheet sheet)
    {
        ArgumentNullException.ThrowIfNull(sheet);

        var previousSeries = "";
        var helper = new MapperHelper()
        {
            DateColumn = Columns.Date,
            LinkColumn = Columns.Link,
            TitleMapper = row =>
            {
                if (!row.TryGetColumnValue(Columns.Title, out var title))
                {
                    return null;
                }

                var series = row.TryGetColumnValue(Columns.Series, out var curSeries)
                    ? previousSeries = curSeries
                    : previousSeries;

                return $"{series} - {title}";
            }
        };

        return helper.ParseRows(sheet.Data[0].RowData).Select(row => new Event(row.Date, row.Title, row.Link));
    }
}
