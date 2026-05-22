using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TudfConverter.Application.Interfaces;
using TudfConverter.Domain.Validation.Results;

namespace TudfConverter.Infrastructure.Reports;

public class ValidationReportWriter : IValidationReportWriter
{
    private readonly ILogger<ValidationReportWriter> _logger;

    public ValidationReportWriter(ILogger<ValidationReportWriter> logger)
    {
        _logger = logger;
    }

    public async Task WriteAsync(string reportFilePath, List<RecordValidationResult> results, FileProcessingResult summary, CancellationToken ct = default)
    {
        var directory = Path.GetDirectoryName(reportFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var writer = new StreamWriter(reportFilePath, false, new UTF8Encoding(false));

        await writer.WriteLineAsync("TUDF Converter Validation Report");
        await writer.WriteLineAsync($"Generated: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        await writer.WriteLineAsync($"Total Records: {summary.TotalRows}");
        await writer.WriteLineAsync($"Accepted Records: {summary.AcceptedRows}");
        await writer.WriteLineAsync($"Rejected Records: {summary.RejectedRows}");
        await writer.WriteLineAsync();

        await writer.WriteLineAsync("Row Number,Record Status,Segment,Error Code,Field Name,Error Message,Failure Outcome");

        foreach (var result in results)
        {
            var status = result.IsRecordRejected ? "Rejected" : "Accepted";

            if (result.Errors.Count == 0)
            {
                await writer.WriteLineAsync($"{result.RowNumber},{Escape(status)},,,,,");
            }
            else
            {
                foreach (var err in result.Errors)
                {
                    await writer.WriteLineAsync($"{result.RowNumber},{Escape(status)},{Escape(err.SegmentTag)},{Escape(err.ErrorCode)},{Escape(err.FieldName)},{Escape(err.ErrorMessage)},{Escape(err.Outcome.ToString())}");
                }
            }
        }

        _logger.LogInformation("Validation report written to {Path}", reportFilePath);
    }

    private string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (value.Contains(","))
        {
            return $"\"{value}\"";
        }
        return value;
    }
}
