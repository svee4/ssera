using Google.Apis.Sheets.v4.Data;
using Ssera.Api.Ingestion.Archive.Mappers;
using System.Diagnostics;

namespace Ssera.Api.Ingestion.EventArchive.Mappers;

public sealed class MusicShowsMapper : IEventArchiveSheetMapper
{
    private static class Columns
    {
        public static int Date => 1;
        public static int Show => 2;
        public static int Song => 3;
        public static int Link => 4;
        public static int Remarks => 6;
    }

    public IEnumerable<Event> ParseEvents(Sheet sheet)
    {
        Debug.Assert(sheet is not null);

        var data = sheet.Data[0];

        return Core();

        IEnumerable<Event> Core()
        {
            var previousShow = "";
            var helper = new MapperHelper()
            {
                DateColumn = Columns.Date,
                LinkColumn = Columns.Link,
                TitleMapper = row =>
                {
                    if (!row.TryGetColumnValue(Columns.Song, out var song))
                    {
                        return null;
                    }

                    var show = row.TryGetColumnValue(Columns.Show, out var curShow)
                        ? previousShow = curShow
                        : previousShow;

                    var title = $"{show} - {song}";

                    if (row.TryGetColumnValue(Columns.Remarks, out var remarks))
                    {
                        title += $" - {remarks}";
                    }

                    return title;
                }
            };

            return helper.ParseRows(data.RowData).Select(row => new Event(row.Date, row.Title, row.Link));
        }
        ;

    }
}
