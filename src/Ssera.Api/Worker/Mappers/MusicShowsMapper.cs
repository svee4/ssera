using Google.Apis.Sheets.v4.Data;
using System.Diagnostics;

namespace Ssera.Api.Worker.Mappers;

public sealed class MusicShowsMapper : IMapper<MusicShowsMapper>
{
    public string SheetName => "MusicShows";
    public static MusicShowsMapper Instance { get; } = new();

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
                    var song = row.GetNormalizedColumnValue(Columns.Song);
                    if (song is null) return null;

                    var show = row.TryGetColumnValue(Columns.Show, out var show2)
                        ? previousShow = show2
                        : previousShow;

                    var remarks = row.GetNormalizedColumnValue(Columns.Remarks);

                    var fullTitle = $"{show} - {song}";
                    if (remarks is not null) fullTitle += $" - {remarks}";
                    return fullTitle;
                }
            };

            return helper.ParseRows(data.RowData).Select(row => new Event { Date = row.Date, Title = row.Title, Link = row.Link });
        }
        ;

    }
}
