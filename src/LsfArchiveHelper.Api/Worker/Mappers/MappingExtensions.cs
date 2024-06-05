using System.Diagnostics.CodeAnalysis;
using Google.Apis.Sheets.v4.Data;

namespace LsfArchiveHelper.Api.Worker.Mappers;

public static class MappingExtensions
{
	
	/// <summary>
	/// Normalized empty or whitespace strings to null
	/// </summary>
	/// <param name="rowData"></param>
	/// <param name="columnIndex"></param>
	/// <returns></returns>
	public static string? GetNormalizedColumnValue(this RowData rowData, int columnIndex)
	{
		ArgumentNullException.ThrowIfNull(rowData);
		var row = rowData.Values[columnIndex]?.EffectiveValue?.StringValue; // this library has not null ref types anywhere btw
		return string.IsNullOrWhiteSpace(row) ? null : row;
	}

	/// <summary>
	/// Gets column value if it is not null or whitespace
	/// </summary>
	/// <param name="rowData"></param>
	/// <param name="columnIndex"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public static bool TryGetColumnValue(this RowData rowData, int columnIndex, [NotNullWhen(true)] out string? value)
	{
		value = default;
		
		var realValue = rowData.GetNormalizedColumnValue(columnIndex);
		if (realValue is null) return false;

		value = realValue;
		return true;
	}
}

