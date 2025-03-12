using Google.Apis.Sheets.v4.Data;
using System.Diagnostics;

namespace Ssera.Api.Worker.Mappers;

public sealed class DefaultMapper : IMapper<DefaultMapper>
{
    public string SheetName => "DEFAULT";
    public static DefaultMapper Instance { get; } = new();

    public IEnumerable<Event> ParseEvents(Sheet sheet)
    {
        Debug.Assert(sheet is not null);

        var data = sheet.Data[0];

        var helper = new MapperHelper();
        return helper.ParseRows(data.RowData).Select(row => new Event { Date = row.Date, Title = row.Title, Link = row.Link });
    }
}
