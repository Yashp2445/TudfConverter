using System;

namespace TudfConverter.Domain.Models;

/// <summary>
/// Represents an Identification Segment containing borrower ID details.
/// </summary>
public class IdentificationModel
{
    public required int SegmentIndex { get; init; }
    public required int IdType { get; init; }
    public required string IdNumber { get; init; }
    public DateOnly? IssueDate { get; init; }
    public DateOnly? ExpirationDate { get; init; }
}
