using System;

namespace TudfConverter.Application.Models;

public class ProcessingHistoryItem
{
    public DateTime ProcessedAt { get; set; }
    public string InputFileName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int AcceptedRows { get; set; }
    public int RejectedRows { get; set; }
    public string? OutputFilePath { get; set; }
    public string? ReportFilePath { get; set; }
}
