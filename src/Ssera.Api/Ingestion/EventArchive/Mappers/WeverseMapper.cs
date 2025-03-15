using Google.Apis.Sheets.v4.Data;

namespace Ssera.Api.Ingestion.EventArchive.Mappers;

public sealed class WeverseMapper : IEventArchiveSheetMapper
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
        var helper = new MapperHelper { SkipRows = 3 };
        return helper.ParseRows(sheet.Data[0].RowData).Select(row => new Event(row.Date, row.Title, row.Link));
    }
}
