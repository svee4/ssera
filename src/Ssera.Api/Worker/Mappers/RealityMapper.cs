using Google.Apis.Sheets.v4.Data;

namespace Ssera.Api.Worker.Mappers;

public sealed class RealityMapper : IMapper<RealityMapper>
{
	public string SheetName => "Reality";
	public static RealityMapper Instance { get; } = new();
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
				var title = row.GetNormalizedColumnValue(Columns.Title);
				if (title is null) return null;

				var series = row.TryGetColumnValue(Columns.Series, out var series2)
					? previousSeries = series2
					: previousSeries;
				
				return $"{series} - {title}";
			}
		};

		return helper.ParseRows(sheet.Data[0].RowData).Select(row => new Event { Date = row.Date, Title = row.Title, Link = row.Link });
	}
	
}
