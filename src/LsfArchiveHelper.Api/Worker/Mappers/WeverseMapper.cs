using Google.Apis.Sheets.v4.Data;

namespace LsfArchiveHelper.Api.Worker.Mappers;

public sealed class WeverseMapper : IMapper<WeverseMapper>
{
	public string SheetName => "Weverse live";
	public static WeverseMapper Instance { get; } = new();
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
		return helper.ParseRows(sheet.Data[0].RowData).Select(row => new Event { Date = row.Date, Title = row.Title, Link = row.Link });
	}
}
