namespace TudfConverter.Domain.Validation.Results;

/// <summary>
/// Defines the outcome severity of a validation failure.
/// </summary>
public enum FailureOutcome
{
    RejectRecord,
    RejectField,
    RejectSegment,
    Ignore
}
