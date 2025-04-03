using Google.Apis.Sheets.v4.Data;

namespace Ssera.Api.Ingestion.EventArchive.Mappers;

public sealed class WeverseMapper : IEventArchiveSheetMapper
{
    public IEnumerable<Event> ParseEvents(Sheet sheet)
    {
        ArgumentNullException.ThrowIfNull(sheet);
        var helper = new MapperHelper { SkipRows = 3 };
        return helper.ParseRows(sheet.Data[0].RowData).Select(row => new Event(row.Date, row.Title, row.Link));
    }
}
