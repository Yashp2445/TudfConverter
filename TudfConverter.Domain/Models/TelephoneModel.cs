namespace TudfConverter.Domain.Models;

/// <summary>
/// Represents a Telephone Segment containing borrower telephone numbers.
/// </summary>
public class TelephoneModel
{
    public required int SegmentIndex { get; init; }
    public required string TelephoneNumber { get; init; }
    public string? TelephoneExtension { get; init; }
    public required string TelephoneType { get; init; }
}
