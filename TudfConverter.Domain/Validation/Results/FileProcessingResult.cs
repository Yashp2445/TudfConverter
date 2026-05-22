using System;
using System.Collections.Generic;

namespace TudfConverter.Domain.Validation.Results;

/// <summary>
/// Contains the complete processing summary, including diagnostics, file paths, and metrics.
/// </summary>
public class FileProcessingResult
{
    public bool IsSuccess { get; set; }
    public int TotalRows { get; set; }
    public int AcceptedRows { get; set; }
    public int RejectedRows { get; set; }
    public List<RecordValidationResult> ValidationResults { get; }
    public string? GeneratedFilePath { get; set; }
    public string? ReportFilePath { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; }

    public FileProcessingResult()
    {
        ValidationResults = new List<RecordValidationResult>();
        ProcessedAt = DateTime.Now;
    }
}
