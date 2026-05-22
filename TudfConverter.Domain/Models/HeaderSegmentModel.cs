using System;

namespace TudfConverter.Domain.Models;

/// <summary>
/// Represents the Header Segment (TUDF) containing reporting member info and cycle parameters.
/// </summary>
public class HeaderSegmentModel
{
    public required string MemberUserId { get; init; }
    public string? ShortName { get; init; }
    public string? ReportingCycle { get; init; }
    public required DateOnly DateReportedAndCertified { get; init; }
    public string? MemberData { get; init; }
}
