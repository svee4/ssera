using Google.Apis.Sheets.v4.Data;
using Ssera.Api.Ingestion.Archive.Mappers;

namespace Ssera.Api.Ingestion.EventArchive.Mappers;

public sealed class MapperHelper
{
    public int DateColumn { get; set; } = 1;
    public int TitleColumn { get; set; } = 2;
    public int LinkColumn { get; set; } = 3; // raw link column
    public int HyperlinkColumn { get; set; } = 4; // fallback hyperlink column if no raw link
    public int SkipRows { get; set; } = 2;

    public Func<RowData, string?>? TitleMapper { get; set; }

    public sealed record Row(DateTime Date, string? Title, string? Link);

    public IEnumerable<Row> ParseRows(IEnumerable<RowData> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        TitleMapper ??= row => row.GetNormalizedColumnValue(TitleColumn);

        return Core();

        IEnumerable<Row> Core()
        {
            var previousDate = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            foreach (var row in rows.Skip(SkipRows))
            {
                var title = TitleMapper(row);
                var link = row.GetNormalizedColumnValue(LinkColumn)
                    ?? row.Values[HyperlinkColumn].Hyperlink.Normalized();

                if (title is null && link is null)
                {
                    continue;
                }

                var date = row.TryGetColumnValueAsDate(DateColumn, out var curDate)
                    ? previousDate = DateTime.SpecifyKind(curDate, DateTimeKind.Utc)
                    : previousDate;

                yield return new Row(date, title, link);
            }
        }
    }
}

