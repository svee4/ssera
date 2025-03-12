using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Google.Apis.Sheets.v4.Data;

namespace Ssera.Api.Worker.Mappers;

// row where link and title are null or whitespace will be skipped
public sealed class MapperHelper
{
	public int DateColumn { get; set; } = 1;
	public int TitleColumn { get; set; } = 2;
	public int LinkColumn { get; set; } = 3;
	public int SkipRows { get; set; } = 2;
	
	public Func<RowData, string?>? TitleMapper { get; set; }

	public class Row
	{
		public required DateTime Date { get; init; }
		public required string? Title { get; init; }
		public required string? Link { get; init; }
	}

	public IEnumerable<Row> ParseRows(IEnumerable<RowData> rows)
	{
		ArgumentNullException.ThrowIfNull(rows);

		TitleMapper ??= row => row.GetNormalizedColumnValue(TitleColumn);

		return Core();

		IEnumerable<Row> Core()
		{
			var previousDate = DateTime.MinValue;
			foreach (var row in rows.Skip(SkipRows))
			{
				var title = TitleMapper(row);
				var link = row.GetNormalizedColumnValue(LinkColumn);

				if ((title, link) is (null, null)) continue;
				
				DateTime date;
				if (row.TryGetColumnValue(DateColumn, out var dateString)
				    && dateString != "Globalz in Cali" // known bad value in weverse sheet
				    )
				{
					date = previousDate = DateTime.Parse(dateString, CultureInfo.InvariantCulture);
				}
				else
				{
					date = previousDate;
				}
				
				yield return new Row { Date = date, Title = title, Link = link };
			}
		}
	}
}

