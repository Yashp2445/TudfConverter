using System;
using System.Collections.Generic;

namespace TudfConverter.Application.Pipeline;

/// <summary>
/// Represents a raw row parsed from an Excel sheet.
/// </summary>
public class RawExcelRow
{
    public required int RowNumber { get; init; }
    public Dictionary<string, string> Columns { get; }

    public RawExcelRow()
    {
        // Enforce case-insensitive key lookup natively
        Columns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the trimmed value for the given column name using case-insensitive lookup.
    /// Returns empty string if the column is not found or is null.
    /// </summary>
    public string Get(string columnName)
    {
        if (string.IsNullOrEmpty(columnName)) return string.Empty;
        
        if (Columns.TryGetValue(columnName, out var value))
        {
            return value?.Trim() ?? string.Empty;
        }
        
        return string.Empty;
    }
}
