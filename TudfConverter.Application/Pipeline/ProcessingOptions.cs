using System;

namespace TudfConverter.Application.Pipeline;

/// <summary>
/// Configurations and parameters governing the Excel to TUDF processing execution.
/// </summary>
public class ProcessingOptions
{
    public required string OutputFolder { get; init; }
    public required string ReportFolder { get; init; }
    public required string MemberUserId { get; init; }
    public string? MemberShortName { get; init; }
    public required string ReportingCycle { get; init; }
    public required DateOnly DateReportedAndCertified { get; init; }
    public bool GenerateReportOnSuccess { get; init; } = true;
    public bool GenerateReportOnError { get; init; } = true;
}
