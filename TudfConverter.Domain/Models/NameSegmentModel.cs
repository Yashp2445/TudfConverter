using System;

namespace TudfConverter.Domain.Models;

/// <summary>
/// Represents the Name Segment containing demographic details of the borrower.
/// </summary>
public class NameSegmentModel
{
    public required string FullName { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public int? Gender { get; init; }
}
