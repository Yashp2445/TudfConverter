using System.Collections.Generic;

namespace TudfConverter.Application.Pipeline;

/// <summary>
/// Holds the results of parsing an Excel workbook, separating successful rows from structural errors.
/// </summary>
public class ExcelReadResult
{
    public List<RawExcelRow> Rows { get; }
    public List<string> Errors { get; }
    public Dictionary<string, string> HeaderData { get; }
    public bool IsSuccess => Errors.Count == 0;

    public ExcelReadResult()
    {
        Rows = new List<RawExcelRow>();
        Errors = new List<string>();
        HeaderData = new Dictionary<string, string>();
    }
}
