namespace TudfConverter.Domain.Validation.Results;

/// <summary>
/// Represents a single validation error mapped to a specific row and field.
/// </summary>
public class ValidationError
{
    public required int RowNumber { get; init; }
    public required string FieldName { get; init; }
    public required string ErrorMessage { get; init; }
    public required string ErrorCode { get; init; }
    public required FailureOutcome Outcome { get; init; }
    public string? SegmentTag { get; init; }
}
