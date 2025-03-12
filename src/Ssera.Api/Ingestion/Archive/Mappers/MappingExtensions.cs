using Google.Apis.Sheets.v4.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Ssera.Api.Ingestion.Archive.Mappers;

public static partial class MappingExtensions
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
    public static bool TryGetColumnValue(this RowData rowData, int columnIndex, [NotNullWhen(true)] out string? value)
    {
        value = default;

        var realValue = rowData.GetNormalizedColumnValue(columnIndex);
        if (realValue is null) return false;

        value = realValue;
        return true;
    }

    public const string DefaultDateFormat = "yyyy'.'MM'.'dd";

    /// <summary>
    /// Attempts to parse the value in the column into a date with <see cref="DefaultDateFormat"/>
    /// </summary>
    public static bool TryGetColumnValueAsDate(this RowData rowData, int columnIndex, out DateTime date) =>
        rowData.TryGetColumnValueAsDate(columnIndex, DefaultDateFormat, out date);

    /// <summary>
    /// Attempts to parse the value in the column into a date with the given format
    /// </summary>
    public static bool TryGetColumnValueAsDate(this RowData rowData, int columnIndex, string format, out DateTime date)
    {
        if (rowData.TryGetColumnValue(columnIndex, out var stringDate)
            && DateTime.TryParseExact(
                stringDate,
                format,
                CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var result))
        {
            date = result;
            return true;
        }

        date = default;
        return false;
    }

    [LoggerMessage(Message = "Invalid date format (Sheet: {Sheet}, DateValue: {DateValue}, Row: {Row})",
        Level = LogLevel.Error)]
    public static partial void LogInvalidDateFormat(ILogger logger, string sheet, string dateValue, int row);
}

