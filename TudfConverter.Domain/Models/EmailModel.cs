namespace TudfConverter.Domain.Models;

/// <summary>
/// Represents an Email Segment containing borrower email addresses.
/// </summary>
public class EmailModel
{
    public required int SegmentIndex { get; init; }
    public required string EmailId { get; init; }
}
