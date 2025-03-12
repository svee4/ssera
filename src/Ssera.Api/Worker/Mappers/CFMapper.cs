using System.Diagnostics;Ssera
using System.Globalization;
using Google.Apis.Sheets.v4.Data;

namespace Ssera.Api.Worker.Mappers;

public sealed class CFMapper : IMapper<CFMapper>
{
	public string SheetName => "CF";
	public static CFMapper Instance { get; } = new();

	public IEnumerable<Event> ParseEvents(Sheet sheet)
	{
		Debug.Assert(sheet is not null);

		var columns = new
		{
			Date = 1,
			CFType = 2,
			Title = 3,
			Members = 4,
			Link = 5
		};

		var data = sheet.Data[0];

		return Core();

		IEnumerable<Event> Core()
		{
			var previousDate = DateTime.MinValue;
			foreach (var row in data.RowData.Skip(2))
			{
				var link = row.GetNormalizedColumnValue(columns.Link);
				var title = row.GetNormalizedColumnValue(columns.Title);
				var cfType = row.GetNormalizedColumnValue(columns.CFType);
				var members = row.GetNormalizedColumnValue(columns.Members);

				IReadOnlyList<string?> all = [link, title, cfType, members];
				if (all.All(s => s is null))
				{
					continue;
				}

				Debug.Assert(link!.StartsWith("https", StringComparison.OrdinalIgnoreCase));

				var date = row.TryGetColumnValue(columns.Date, out var dateString)
					? previousDate = DateTime.Parse(dateString, CultureInfo.InvariantCulture)
					: previousDate;

				string fullTitle = null!;
				if ((members, cfType, title) is (not null, not null, not null))
				{
					fullTitle = $"{members} - {cfType} - {title}";
				}
				else
				{
					// loop and check null and append because im lazy
					var first = true;
					foreach (var s in all)
					{
						if (s is null) continue;
						if (first)
						{
							first = false;
							fullTitle = s;
						}
						else
						{
							fullTitle += " - " + s;
						}
					}
				}

				yield return new Event { Date = date, Title = fullTitle, Link = link };
			}
		}
	}
}
