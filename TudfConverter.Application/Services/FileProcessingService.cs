using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TudfConverter.Application.Interfaces;
using TudfConverter.Application.Mapping;
using TudfConverter.Application.Pipeline;
using TudfConverter.Domain.Models;
using TudfConverter.Domain.Validation;
using TudfConverter.Domain.Validation.Results;

namespace TudfConverter.Application.Services;

public class FileProcessingService : IFileProcessingService
{
    private readonly IExcelReaderService _excelReader;
    private readonly ExcelToConsumerRecordMapper _mapper;
    private readonly IValidationOrchestrator _validator;
    private readonly ITudfGenerationService _generator;
    private readonly IFileExportService _fileExporter;
    private readonly IValidationReportWriter _reportWriter;
    private readonly ILogger<FileProcessingService> _logger;

    public FileProcessingService(
        IExcelReaderService excelReader,
        ExcelToConsumerRecordMapper mapper,
        IValidationOrchestrator validator,
        ITudfGenerationService generator,
        IFileExportService fileExporter,
        IValidationReportWriter reportWriter,
        ILogger<FileProcessingService> logger)
    {
        _excelReader = excelReader;
        _mapper = mapper;
        _validator = validator;
        _generator = generator;
        _fileExporter = fileExporter;
        _reportWriter = reportWriter;
        _logger = logger;
    }

    public async Task<FileProcessingResult> ProcessFileAsync(
        string excelFilePath, 
        ProcessingOptions options, 
        IProgress<ProcessingProgress>? progress = null, 
        CancellationToken ct = default)
    {
        try
        {
            // Stage 1 — Validate input
            if (string.IsNullOrEmpty(excelFilePath) || !File.Exists(excelFilePath))
            {
                return new FileProcessingResult { IsSuccess = false, ErrorMessage = $"Excel file not found: {excelFilePath}" };
            }
            if (string.IsNullOrEmpty(options.OutputFolder))
            {
                return new FileProcessingResult { IsSuccess = false, ErrorMessage = "Output folder is not configured." };
            }

            var outputFolder = Path.GetFullPath(options.OutputFolder);
            var reportFolder = string.IsNullOrEmpty(options.ReportFolder) ? outputFolder : Path.GetFullPath(options.ReportFolder);

            // Stage 2 — Read Excel file
            progress?.Report(new ProcessingProgress { Message = "Reading Excel file...", Percentage = 5, ProcessedRows = 0, TotalRows = 0 });
            var readResult = await _excelReader.ReadAsync(excelFilePath, ct);
            if (readResult.Errors.Any())
            {
                foreach (var err in readResult.Errors) _logger.LogError(err);
                return new FileProcessingResult { IsSuccess = false, ErrorMessage = readResult.Errors.First() };
            }

            // Stage 3 — Map rows to domain models
            progress?.Report(new ProcessingProgress { Message = "Mapping Excel rows to domain models...", Percentage = 15, ProcessedRows = 0, TotalRows = 0 });
            var records = new List<ConsumerRecord>();
            foreach (var row in readResult.Rows)
            {
                records.Add(_mapper.Map(row));
            }
            if (records.Count == 0)
            {
                return new FileProcessingResult { IsSuccess = false, ErrorMessage = "No data rows found in the Excel file." };
            }
            _logger.LogInformation("Mapped {Count} records from Excel.", records.Count);

            // Stage 4 — Build header model
            var memberId = readResult.HeaderData.TryGetValue("Reporting Member ID", out var mid) && !string.IsNullOrEmpty(mid) ? mid : options.MemberUserId;
            
            // Append _DATASUBMISSION suffix to match production TUDF format
            if (!memberId.EndsWith("_DATASUBMISSION", StringComparison.OrdinalIgnoreCase))
                memberId += "_DATASUBMISSION";

            var shortName = readResult.HeaderData.TryGetValue("Short Name", out var sname) ? sname : options.MemberShortName;
            var cycle = readResult.HeaderData.TryGetValue("Cycle Identification", out var cyc) && !string.IsNullOrEmpty(cyc) ? cyc : options.ReportingCycle;
            
            DateOnly headerDate = options.DateReportedAndCertified == default 
                ? DateOnly.FromDateTime(DateTime.Today) 
                : options.DateReportedAndCertified;

            if (readResult.HeaderData.TryGetValue("Date Reported", out var drStr) && !string.IsNullOrEmpty(drStr))
            {
                if (DateOnly.TryParseExact(drStr, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out var d1)) headerDate = d1;
                else if (DateOnly.TryParseExact(drStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var d2)) headerDate = d2;
                else if (DateOnly.TryParseExact(drStr, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var d3)) headerDate = d3;
            }
                
            var header = new HeaderSegmentModel
            {
                MemberUserId = memberId,
                ShortName = shortName,
                ReportingCycle = cycle,
                DateReportedAndCertified = headerDate,
                MemberData = string.Empty
            };

            // Stage 5 — Validate all records

            // Default DateReportedAndCertified on all records if it is default/missing, mirroring the test suite behavior.
            foreach (var record in records)
            {
                if (record.Account != null && (record.Account.DateReportedAndCertified == default || record.Account.DateReportedAndCertified == DateOnly.MinValue))
                {
                    record.Account.DateReportedAndCertified = headerDate;
                }
            }

            progress?.Report(new ProcessingProgress { Message = "Validating records against UCRF rules...", Percentage = 25, ProcessedRows = 0, TotalRows = records.Count });
            var validationProgress = new Progress<ProcessingProgress>(p => 
            {
                // Map inner progress 0-100 to outer 25-65
                var scaledPercentage = 25 + (int)(p.Percentage * 0.40);
                progress?.Report(new ProcessingProgress { Message = p.Message, Percentage = scaledPercentage, ProcessedRows = p.ProcessedRows, TotalRows = p.TotalRows });
            });
            var validationResults = await _validator.ValidateAllAsync(records, headerDate, validationProgress, ct);
            
            int acceptedCount = validationResults.Count(r => !r.IsRecordRejected);
            int rejectedCount = validationResults.Count(r => r.IsRecordRejected);
            _logger.LogInformation("Validation completed. Accepted: {Accepted}, Rejected: {Rejected}", acceptedCount, rejectedCount);

            var summary = new FileProcessingResult
            {
                IsSuccess = true,
                TotalRows = records.Count,
                AcceptedRows = acceptedCount,
                RejectedRows = rejectedCount,
            };
            summary.ValidationResults.AddRange(validationResults);

            // Stage 6 — Generate TUDF file
            progress?.Report(new ProcessingProgress { Message = "Generating TUDF file...", Percentage = 70, ProcessedRows = records.Count, TotalRows = records.Count });
            var validRecords = records.Where(r => 
            {
                var v = validationResults.FirstOrDefault(vr => vr.RowNumber == r.RowNumber);
                return v != null && !v.IsRecordRejected;
            }).ToList();

            string tudfContent = string.Empty;
            if (validRecords.Count > 0)
            {
                tudfContent = _generator.Generate(validRecords, header);
            }
            else
            {
                summary.IsSuccess = false;
                summary.ErrorMessage = "All records were rejected. No TUDF file generated.";
            }

            string generatedFilePath = string.Empty;
            // Stage 7 — Write TUDF file
            if (summary.IsSuccess && !string.IsNullOrEmpty(tudfContent))
            {
                progress?.Report(new ProcessingProgress { Message = "Writing TUDF output file...", Percentage = 80, ProcessedRows = records.Count, TotalRows = records.Count });
                generatedFilePath = await _fileExporter.BuildOutputFilePathAsync(outputFolder, header.MemberUserId, headerDate);
                await _fileExporter.WriteAsync(generatedFilePath, tudfContent, ct);
                _logger.LogInformation("TUDF file generated at {Path}", generatedFilePath);
                summary.GeneratedFilePath = generatedFilePath;
            }

            // Stage 8 — Write validation report
            progress?.Report(new ProcessingProgress { Message = "Writing validation report...", Percentage = 90, ProcessedRows = records.Count, TotalRows = records.Count });
            if (!Directory.Exists(reportFolder)) Directory.CreateDirectory(reportFolder);
            
            string baseFileName = string.IsNullOrEmpty(generatedFilePath) 
                ? $"{header.MemberUserId}_{headerDate:ddMMyyyy}_{DateTime.Now:HHmmss}"
                : Path.GetFileNameWithoutExtension(generatedFilePath);
                
            string reportFilePath = Path.Combine(reportFolder, $"{baseFileName}_ValidationReport.csv");
            await _reportWriter.WriteAsync(reportFilePath, validationResults, summary, ct);
            summary.ReportFilePath = reportFilePath;

            // Stage 9 — Build and return result
            progress?.Report(new ProcessingProgress { Message = "Processing complete.", Percentage = 100, ProcessedRows = records.Count, TotalRows = records.Count });
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unexpected error during processing.");
            return new FileProcessingResult 
            { 
                IsSuccess = false, 
                ErrorMessage = $"Unexpected error during processing: {ex.Message}" 
            };
        }
    }
}
